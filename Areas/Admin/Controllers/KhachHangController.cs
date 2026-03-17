// File: Areas/Admin/Controllers/KhachHangController.cs
using System;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using WebQuanLiCuaHangTapHoa.Models;                         // đổi namespace nếu khác
using WebQuanLiCuaHangTapHoa.Areas.Admin.Models.ViewModels;  // KhachHangVM
using System.Data.Entity;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class KhachHangController : BaseController
    {
        // Khởi tạo DbContext (auto-generated). readonly tránh gán nhầm.
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // ================================
        // INDEX: danh sách + tìm kiếm + filter + sort + phân trang
        // ================================
        // search: chuỗi tìm kiếm tên/SDT
        // filter: giá trị preset ("don1","don5","tien1","tien5","tien10")
        // sort: "ma_asc","ma_desc","ten_az"
        public ActionResult Index(string search, string filter, string sort, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;

            // 1) Lấy base query từ KhachHang
            var q = _db.KhachHang.AsQueryable();

            // 2) Nếu có search -> lọc tên hoặc số điện thoại
            if (!string.IsNullOrEmpty(search))
            {
                string s = search.Trim();
                q = q.Where(k => k.TenKH.Contains(s) || k.SoDT.Contains(s));
            }

            // 3) Tính thống kê (TongDon, TongTien) bằng LINQ trên DB cho hiệu năng tốt
            var thongKeDonHang = _db.HoaDon
                .GroupBy(h => h.MaKH)
                .Select(g => new
                {
                    MaKH = g.Key,
                    TongDon = g.Count(),
                    TongTien = g.SelectMany(h => h.ChiTietHoaDon).Sum(ct => (decimal?)(ct.SoLuong * ct.DonGia)) ?? 0
                });

            // 4) Left join KhachHang với thongKeDonHang -> project về KhachHangVM
            var listVM = q
                .GroupJoin(
                    thongKeDonHang,
                    kh => kh.MaKH,
                    tk => tk.MaKH,
                    (kh, tks) => new { kh, tks }
                )
                .SelectMany(
                    x => x.tks.DefaultIfEmpty(),
                    (x, tk) => new KhachHangVM
                    {
                        MaKH = x.kh.MaKH,
                        TenKH = x.kh.TenKH,
                        SoDT = x.kh.SoDT,
                        DiaChi = x.kh.DiaChi,
                        TongDon = tk == null ? 0 : tk.TongDon,
                        TongTien = tk == null ? 0 : tk.TongTien
                    }
                );

            // 5) Áp dụng filter preset (dropdown)
            if (!string.IsNullOrEmpty(filter))
            {
                switch (filter)
                {
                    case "don1":
                        listVM = listVM.Where(x => x.TongDon >= 1);
                        break;
                    case "don5":
                        listVM = listVM.Where(x => x.TongDon >= 5);
                        break;
                    case "tien1":
                        listVM = listVM.Where(x => x.TongTien >= 1_000_000);
                        break;
                    case "tien5":
                        listVM = listVM.Where(x => x.TongTien >= 5_000_000);
                        break;
                    case "tien10":
                        listVM = listVM.Where(x => x.TongTien >= 10_000_000);
                        break;
                }
            }

            // 6) Áp dụng sort
            switch (sort)
            {
                case "ma_asc":
                    listVM = listVM.OrderBy(x => x.MaKH);
                    break;
                case "ma_desc":
                    listVM = listVM.OrderByDescending(x => x.MaKH);
                    break;
                case "ten_az":
                    listVM = listVM.OrderBy(x => x.TenKH);
                    break;
                default:
                    listVM = listVM.OrderByDescending(x => x.MaKH); // default: mã mới nhất trước
                    break;
            }

            // 7) Chuyển sang PagedList và trả view
            var paged = listVM.ToPagedList(pageNumber, pageSize);
            return View(paged);
        }

        // ================================
        // PARTIAL: form thêm
        // ================================
        [HttpGet]
        public ActionResult Them()
        {
            // Trả PartialView chứa modal add
            return PartialView("_ThemKhachHang");
        }

        // ================================
        // POST: thêm (bind model bình thường)
        // ================================
        [HttpPost]
        public JsonResult Them(KhachHang model)
        {
            try
            {
                // Validate model theo DataAnnotations trên entity (nếu có)
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
                }

                // Thêm vào DB
                _db.KhachHang.Add(model);
                _db.SaveChanges();

                return Json(new { success = true, message = "Đã thêm khách hàng." });
            }
            catch (Exception ex)
            {
                // Trả thông tin lỗi để debug, production nên log lại thay vì trả trực tiếp
                return Json(new { success = false, message = "Lỗi khi thêm: " + ex.Message });
            }
        }

        // ================================
        // PARTIAL: form sửa
        // ================================
        [HttpGet]
        public ActionResult Sua(int id)
        {
            var kh = _db.KhachHang.Find(id);
            if (kh == null) return HttpNotFound();
            return PartialView("_SuaKhachHang", kh);
        }

        // ================================
        // POST: sửa
        // ================================
        [HttpPost]
        public JsonResult Sua(KhachHang model)
        {
            try
            {
                // Tìm bản ghi gốc
                var old = _db.KhachHang.Find(model.MaKH);
                if (old == null) return Json(new { success = false, message = "Không tìm thấy khách hàng." });

                // Cập nhật từng trường an toàn
                old.TenKH = model.TenKH;
                old.SoDT = model.SoDT;
                old.DiaChi = model.DiaChi;

                // Lưu
                _db.SaveChanges();
                return Json(new { success = true, message = "Cập nhật thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật: " + ex.Message });
            }
        }

        // ================================
        // PARTIAL: chi tiết (modal)
        // ================================
        [HttpGet]
        public ActionResult ChiTiet(int id)
        {
            var kh = _db.KhachHang.Find(id);
            if (kh == null) return HttpNotFound();

            // Tính thống kê cho cá nhân (tổng đơn, tổng tiền)
            var thongKe = _db.HoaDon
                .Where(h => h.MaKH == id)
                .GroupBy(h => h.MaKH)
                .Select(g => new
                {
                    TongDon = g.Count(),
                    TongTien = g.SelectMany(h => h.ChiTietHoaDon).Sum(ct => (decimal?)(ct.SoLuong * ct.DonGia)) ?? 0
                })
                .FirstOrDefault();

            ViewBag.TongDon = thongKe?.TongDon ?? 0;
            ViewBag.TongTien = thongKe?.TongTien ?? 0M;

            return PartialView("_ChiTietKhachHang", kh);
        }

        // ================================
        // XÓA: kiểm tra ràng buộc trước khi xóa thật
        // ================================
        [HttpPost]
        public JsonResult Xoa(int id)
        {
            try
            {
                var kh = _db.KhachHang.Find(id);
                if (kh == null) return Json(new { success = false, message = "Không tìm thấy khách hàng." });

                // Nếu có hoá đơn liên quan -> không xóa
                var hasOrders = _db.HoaDon.Any(h => h.MaKH == id);
                if (hasOrders)
                {
                    return Json(new { success = false, message = "Khách hàng có hoá đơn liên quan. Không thể xóa." });
                }

                // Nếu có tài khoản khách hàng (bảng TaiKhoanKH) -> không xóa
                var hasAccount = _db.TaiKhoanKH.Any(t => t.MaKH == id);
                if (hasAccount)
                {
                    return Json(new { success = false, message = "Khách hàng có tài khoản liên quan. Vui lòng xóa tài khoản trước." });
                }

                // An toàn -> xóa
                _db.KhachHang.Remove(kh);
                _db.SaveChanges();

                return Json(new { success = true, message = "Đã xóa khách hàng." });
            }
            catch (Exception ex)
            {
                // Nếu vẫn lỗi, trả nội dung inner exception sẽ giúp debug
                return Json(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
            }
        }

        // Dispose DB context
        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }

        // ================================
        // API AUTOCOMPLETE KHÁCH HÀNG
        // ================================
        public JsonResult Tim(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return Json(new object[0], JsonRequestBehavior.AllowGet);

            keyword = keyword.Trim();

            var list = _db.KhachHang
                .Where(k => k.TenKH.Contains(keyword) || k.SoDT.Contains(keyword))
                .OrderBy(k => k.TenKH)
                .Select(k => new
                {
                    k.MaKH,
                    k.TenKH
                })
                .Take(10)
                .ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

    }
}
