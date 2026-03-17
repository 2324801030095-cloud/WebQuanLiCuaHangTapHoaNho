using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using WebQuanLiCuaHangTapHoa.Helpers;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class AdminAuthController : BaseController
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

            ViewBag.Error = TempData["LoginError"];
            ViewBag.RegError = TempData["RegError"];
            ViewBag.RegSuccess = TempData["RegSuccess"];
            ViewBag.ForgotError = TempData["ErrorForgot"];
            ViewBag.ForgotSuccess = TempData["ForgotSuccess"];
            ViewBag.ShowSuccess = TempData["ShowSuccess"];

            return View();
        }

        // ============================================
        // LOGIN (POST) — ADMIN + KHÁCH HÀNG
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

            // ===========================
            // 1) KIỂM TRA ADMIN
            // ===========================
            var admin = _db.TaiKhoan.FirstOrDefault(x =>
                x.TenDangNhap == userName &&
                (x.MatKhau == passRaw || x.MatKhau == hash)
            );

            if (admin != null)
            {
                // Lưu session Admin
                Session["UserName"] = admin.TenDangNhap;
                Session["Role"] = admin.Quyen;
                Session["MaNV"] = admin.MaNV;

                var nv = _db.NhanVien.Find(admin.MaNV);
                Session["UserAvatar"] = nv?.HinhAnh ?? "default.png";

                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Admin");
            }

            // ===========================
            // 2) KIỂM TRA KHÁCH HÀNG
            // ===========================
            var userKH = _db.TaiKhoanKH.FirstOrDefault(k =>
                k.TenDangNhap == userName &&
                k.MatKhau == hash &&
                k.HoatDong == true
            );

            if (userKH != null)
            {
                var kh = _db.KhachHang.FirstOrDefault(k => k.MaKH == userKH.MaKH);

                // Lưu SESSION KHÁCH HÀNG
                Session["KH"] = new
                {
                    kh.MaKH,
                    kh.TenKH,
                    userKH.TenDangNhap,
                    userKH.Email
                };

                // Không cho khách hàng quay vào khu vực Admin
                if (!string.IsNullOrEmpty(returnUrl) &&
                    !returnUrl.ToLower().Contains("/admin"))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // ===========================
            // 3) SAI TÀI KHOẢN
            // ===========================
            TempData["LoginError"] = "Sai tên đăng nhập hoặc mật khẩu.";
            return RedirectToAction("Login", new { returnUrl });
        }

        // ======================================================
        // REGISTER — Tạo Nhân Viên + Tạo Tài Khoản ADMIN
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

            if (_db.TaiKhoan.Any(x => x.TenDangNhap == TenDangNhap))
            {
                TempData["RegError"] = "Tên đăng nhập đã tồn tại.";
                return RedirectToAction("Login");
            }

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

            if (AvatarFile != null && AvatarFile.ContentLength > 0)
            {
                string fileName = "avt_" + DateTime.Now.Ticks +
                                  System.IO.Path.GetExtension(AvatarFile.FileName);

                string savePath = Server.MapPath("~/Content/avatars/" + fileName);
                AvatarFile.SaveAs(savePath);

                nv.HinhAnh = fileName;
            }

            _db.NhanVien.Add(nv);
            _db.SaveChanges();

            string hash = PasswordHelper.HashSha256(MatKhau);

            var tk = new TaiKhoan
            {
                TenDangNhap = TenDangNhap.Trim(),
                MatKhau = hash,
                MaNV = nv.MaNV,
                Quyen = "QuanTri"
            };

            _db.TaiKhoan.Add(tk);
            _db.SaveChanges();

            TempData["ShowSuccess"] = true;
            return RedirectToAction("Login");
        }

        // ============================================
        // FORGOT PASSWORD
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
            Session.Abandon();

            // Điều hướng về trang đăng nhập của khách (TaiKhoan/DangNhap)
            return RedirectToAction("DangNhap", "TaiKhoan", new { area = "" });
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
