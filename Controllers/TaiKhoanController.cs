using System;
using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using WebQuanLiCuaHangTapHoa.Models.ViewModels.Auth;
using WebQuanLiCuaHangTapHoa.Helpers;

namespace WebQuanLiCuaHangTapHoa.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // ====== ĐĂNG NHẬP ======
        [HttpGet]
        public ActionResult DangNhap(string returnUrl)
        {
            return View(new DangNhapVM { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhap(DangNhapVM model)
        {
            if (!ModelState.IsValid) return View(model);

            // Băm mật khẩu nhập vào
            var hash = PasswordHelper.HashSha256(model.MatKhau);

            // Tìm trong bảng TaiKhoanKH
            var tk = _db.TaiKhoanKH.FirstOrDefault(x =>
                x.TenDangNhap == model.TenDangNhap &&
                x.MatKhau == hash &&
                x.HoatDong == true);

            if (tk == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng!");
                return View(model);
            }

            // Lấy thông tin khách hàng tương ứng
            var kh = _db.KhachHang.FirstOrDefault(k => k.MaKH == tk.MaKH);

            // Lưu session người dùng
            Session["KH"] = new
            {
                kh.MaKH,
                kh.TenKH,
                tk.TenDangNhap,
                tk.Email
            };

            // Chuyển hướng
            return Redirect(string.IsNullOrEmpty(model.ReturnUrl)
                ? Url.Action("Index", "Home")
                : model.ReturnUrl);
        }

        // ====== ĐĂNG KÝ ======
        [HttpGet]
        public ActionResult DangKy()
        {
            return View(new DangKyVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(DangKyVM model)
        {
            if (!ModelState.IsValid) return View(model);

            // Kiểm tra trùng tên đăng nhập
            if (_db.TaiKhoanKH.Any(x => x.TenDangNhap == model.TenDangNhap))
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
                return View(model);
            }

            // 1. Tạo khách hàng
            var kh = new KhachHang
            {
                TenKH = model.TenKH,
                SoDT = model.SoDT,
                DiaChi = model.DiaChi
            };
            _db.KhachHang.Add(kh);
            _db.SaveChanges(); // Sinh MaKH

            // 2. Tạo tài khoản
            var tk = new TaiKhoanKH
            {
                TenDangNhap = model.TenDangNhap,
                MatKhau = PasswordHelper.HashSha256(model.MatKhau),
                Email = model.Email,
                MaKH = kh.MaKH,
                NgayDangKy = DateTime.Now,
                HoatDong = true
            };
            _db.TaiKhoanKH.Add(tk);
            _db.SaveChanges();

            // 3. Tự động đăng nhập
            Session["KH"] = new { kh.MaKH, kh.TenKH, tk.TenDangNhap, tk.Email };
            return RedirectToAction("Index", "Home");
        }

        // ====== THÔNG TIN TÀI KHOẢN ======
        [HttpGet]
        public ActionResult ThongTin()
        {
            if (Session["KH"] == null)
                return RedirectToAction("DangNhap");

            var user = (dynamic)Session["KH"];
            int maKH = (int)user.MaKH;
            var kh = _db.KhachHang.Find(maKH);
            var tk = _db.TaiKhoanKH.FirstOrDefault(x => x.MaKH == maKH);

            ViewBag.Email = tk?.Email;
            return View(kh);
        }

        // ====== ĐĂNG XUẤT ======
        public ActionResult DangXuat()
        {
            Session.Remove("KH");
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
