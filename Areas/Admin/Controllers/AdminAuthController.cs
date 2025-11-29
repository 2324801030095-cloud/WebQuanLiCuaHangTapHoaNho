using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using WebQuanLiCuaHangTapHoa.Helpers;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class AdminAuthController : Controller
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db =
            new QuanLyTapHoaThanhNhanEntities1();

        // ============================================
        // LOGIN (GET)
        // ============================================
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            // Gom toàn bộ thông báo từ Register, Forgot, Login
            ViewBag.Error = TempData["LoginError"];
            ViewBag.RegError = TempData["RegError"];
            ViewBag.RegSuccess = TempData["RegSuccess"];
            ViewBag.ForgotError = TempData["ErrorForgot"];
            ViewBag.ForgotSuccess = TempData["ForgotSuccess"];

            ViewBag.ShowSuccess = TempData["ShowSuccess"];

            return View();
        }


        // ============================================
        // LOGIN (POST)
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string tenDangNhap, string matKhau, string returnUrl)
        {
            var userName = (tenDangNhap ?? "").Trim();
            var passRaw = (matKhau ?? "").Trim();

            if (userName == "" || passRaw == "")
            {
                TempData["LoginError"] = "Vui lòng nhập đầy đủ thông tin.";
                return RedirectToAction("Login", new { returnUrl });
            }

            string hash = PasswordHelper.HashSha256(passRaw);

            var user = _db.TaiKhoan.FirstOrDefault(x =>
                x.TenDangNhap == userName &&
                (x.MatKhau == passRaw || x.MatKhau == hash)
            );

            if (user == null)
            {
                TempData["LoginError"] = "Sai tên đăng nhập hoặc mật khẩu.";
                return RedirectToAction("Login", new { returnUrl });
            }

            // LƯU SESSION ĐĂNG NHẬP
            Session["UserName"] = user.TenDangNhap;
            Session["Role"] = user.Quyen;
            Session["MaNV"] = user.MaNV;

            // Lấy avatar nhân viên
            var nv = _db.NhanVien.Find(user.MaNV);
            Session["UserAvatar"] = nv?.HinhAnh ?? "default.png";

            // Điều hướng
            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Admin");
        }


        // ======================================================
        // REGISTER — Tạo Nhân Viên + Tạo Tài Khoản (PHƯƠNG ÁN A)
        // ======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(
            string TenDangNhap,
            string MatKhau,
            string TenNV,
            string ChucVu,
            string Email,
            string SoDT,
            HttpPostedFileBase AvatarFile
        )
        {
            if (string.IsNullOrWhiteSpace(TenDangNhap) ||
                string.IsNullOrWhiteSpace(MatKhau))
            {
                TempData["RegError"] = "Vui lòng nhập đầy đủ thông tin.";
                return RedirectToAction("Login");
            }

            // Check username tồn tại
            if (_db.TaiKhoan.Any(x => x.TenDangNhap == TenDangNhap))
            {
                TempData["RegError"] = "Tên đăng nhập đã tồn tại.";
                return RedirectToAction("Login");
            }

            // =============================
            // 1) Tạo nhân viên mới
            // =============================
            var nv = new NhanVien
            {
                TenNV = TenNV?.Trim(),
                ChucVu = ChucVu?.Trim(),
                Email = Email?.Trim(),
                SoDT = SoDT?.Trim(),
                NgayVaoLam = DateTime.Now,
                NgaySinh = DateTime.Now.AddYears(-22),
                GioiTinh = "Không rõ",
                DiaChi = "",
                CCCD = "",
                GhiChu = "",
                MucLuong = 0,
                HinhAnh = "default.png"
            };

            // Avatar upload (optional)
            if (AvatarFile != null && AvatarFile.ContentLength > 0)
            {
                string fileName = "avt_" + DateTime.Now.Ticks +
                                  System.IO.Path.GetExtension(AvatarFile.FileName);

                string savePath = Server.MapPath("~/Content/avatars/" + fileName);
                AvatarFile.SaveAs(savePath);

                nv.HinhAnh = fileName;
            }

            _db.NhanVien.Add(nv);
            _db.SaveChanges(); // => nv.MaNV có giá trị

            // =============================
            // 2) Tạo tài khoản
            // =============================
            string hash = PasswordHelper.HashSha256(MatKhau);

            var tk = new TaiKhoan
            {
                TenDangNhap = TenDangNhap.Trim(),
                MatKhau = hash,
                MaNV = nv.MaNV,
                Quyen = "QuanTri" // hoặc NhanVien tùy ý
            };

            _db.TaiKhoan.Add(tk);
            _db.SaveChanges();

            TempData["ShowSuccess"] = true; // Trigger animation
            return RedirectToAction("Login");
        }


        // ============================================
        // FORGOT PASSWORD (POST)
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string account)
        {
            if (string.IsNullOrWhiteSpace(account))
            {
                TempData["ErrorForgot"] = "Vui lòng nhập email hoặc tên đăng nhập.";
                return RedirectToAction("Login");
            }

            var user = _db.TaiKhoan.FirstOrDefault(x =>
                x.TenDangNhap == account ||
                (x.NhanVien != null && x.NhanVien.Email == account)
            );

            if (user == null)
            {
                TempData["ErrorForgot"] = "Không tìm thấy tài khoản.";
                return RedirectToAction("Login");
            }

            TempData["ForgotSuccess"] = "Liên hệ quản trị để khôi phục mật khẩu.";
            return RedirectToAction("Login");
        }


        // ============================================
        // LOGOUT
        // ============================================
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
