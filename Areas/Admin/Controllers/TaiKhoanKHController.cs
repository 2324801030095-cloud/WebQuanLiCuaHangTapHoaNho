using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using WebQuanLiCuaHangTapHoa.Models;
using WebQuanLiCuaHangTapHoa.Helpers;
using PagedList;
using System.Collections.Generic;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class TaiKhoanKHController : BaseController
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // ===========================================================
        // INDEX - Danh sách tài khoản khách hàng
        // ===========================================================
        public ActionResult Index(int? page, string search, bool? hoatDong)
        {
            int pageNumber = page ?? 1;
            int pageSize = 15;

            var query = _db.TaiKhoanKH
                .Include("KhachHang")
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => 
                    t.TenDangNhap.Contains(search) || 
                    t.Email.Contains(search) ||
                    (t.KhachHang != null && t.KhachHang.TenKH.Contains(search))
                );
            }

            // Lọc theo hoạt động
            if (hoatDong.HasValue)
            {
                // t.HoatDong is bool? in the model; compare safely
                query = query.Where(t => (t.HoatDong ?? false) == hoatDong.Value);
            }

            var list = query
                .OrderByDescending(t => t.NgayDangKy)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.Search = search;
            ViewBag.HoatDong = hoatDong;

            return View(list);
        }

        // ===========================================================
        // THÊM TÀI KHOẢN
        // ===========================================================
        [HttpGet]
        public ActionResult Them()
        {
            // Load danh sách khách hàng chưa có tài khoản
            var khachHangChuaCoTK = _db.KhachHang
                .Where(kh => !_db.TaiKhoanKH.Any(tk => tk.MaKH == kh.MaKH))
                .Select(kh => new { kh.MaKH, kh.TenKH })
                .ToList();
            
            ViewBag.KhachHangList = new SelectList(khachHangChuaCoTK, "MaKH", "TenKH");
            
            return PartialView("_ThemKH");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Them(FormCollection form)
        {
            try
            {
                if (form == null)
                    return Json(new { success = false, message = "Không nhận được dữ liệu." });

                int maKH;
                if (!int.TryParse(form["MaKH"], out maKH))
                    return Json(new { success = false, message = "Vui lòng chọn khách hàng." });

                string tenDangNhap = form["TenDangNhap"]?.Trim();
                string matKhau = form["MatKhau"]?.Trim();
                string email = form["Email"]?.Trim();

                if (string.IsNullOrWhiteSpace(tenDangNhap))
                    return Json(new { success = false, message = "Tên đăng nhập không được trống." });

                if (string.IsNullOrWhiteSpace(matKhau))
                    return Json(new { success = false, message = "Mật khẩu không được trống." });

                // Kiểm tra tên đăng nhập đã tồn tại
                if (_db.TaiKhoanKH.Any(t => t.TenDangNhap == tenDangNhap))
                    return Json(new { success = false, message = "Tên đăng nhập đã tồn tại." });

                // Kiểm tra email đã tồn tại
                if (!string.IsNullOrWhiteSpace(email) && _db.TaiKhoanKH.Any(t => t.Email == email))
                    return Json(new { success = false, message = "Email đã được sử dụng." });

                // Kiểm tra khách hàng đã có tài khoản
                if (_db.TaiKhoanKH.Any(t => t.MaKH == maKH))
                    return Json(new { success = false, message = "Khách hàng này đã có tài khoản." });

                var tk = new TaiKhoanKH
                {
                    TenDangNhap = tenDangNhap,
                    MatKhau = PasswordHelper.HashSha256(matKhau),
                    Email = email,
                    MaKH = maKH,
                    NgayDangKy = DateTime.Now,
                    HoatDong = form["HoatDong"] == "true" || form["HoatDong"] == "on"
                };

                _db.TaiKhoanKH.Add(tk);
                _db.SaveChanges();

                return Json(new { success = true, message = "Thêm tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ===========================================================
        // SỬA TÀI KHOẢN
        // ===========================================================
        [HttpGet]
        public ActionResult Sua(int id)
        {
            var tk = _db.TaiKhoanKH
                .Include("KhachHang")
                .FirstOrDefault(t => t.MaKH == id);

            if (tk == null) return HttpNotFound();

            return PartialView("_SuaKH", tk);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Sua(FormCollection form)
        {
            try
            {
                int maKH;
                if (!int.TryParse(form["MaKH"], out maKH))
                    return Json(new { success = false, message = "Mã khách hàng không hợp lệ." });

                var old = _db.TaiKhoanKH.Find(maKH);
                if (old == null)
                    return Json(new { success = false, message = "Không tìm thấy tài khoản." });

                old.Email = form["Email"]?.Trim();

                // Nếu có mật khẩu mới
                string matKhau = form["MatKhau"]?.Trim();
                if (!string.IsNullOrWhiteSpace(matKhau))
                {
                    old.MatKhau = PasswordHelper.HashSha256(matKhau);
                }

                old.HoatDong = form["HoatDong"] == "true" || form["HoatDong"] == "on";

                _db.SaveChanges();

                return Json(new { success = true, message = "Đã cập nhật tài khoản." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ===========================================================
        // KHÓA/MỞ KHÓA TÀI KHOẢN
        // ===========================================================
        [HttpPost]
        public JsonResult KhoaMoKhoa(int id)
        {
            try
            {
                var tk = _db.TaiKhoanKH.Find(id);
                if (tk == null)
                    return Json(new { success = false, message = "Không tìm thấy tài khoản." });

                // HoatDong is nullable bool; toggle safely using GetValueOrDefault
                tk.HoatDong = !(tk.HoatDong ?? false);
                _db.SaveChanges();

                return Json(new { 
                    success = true, 
                    message = tk.HoatDong == true ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản.",
                    hoatDong = tk.HoatDong
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}

