// File: Areas/Admin/Controllers/HoaDonController.cs
using System;
using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using PagedList;
using System.Collections.Generic;

// Thêm using để dùng Include<T> extension (EF6)
using System.Data.Entity;

// Thêm using tới namespace chứa ChiTietItemVM
using WebQuanLiCuaHangTapHoa.Areas.Admin.Models.ViewModels;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class HoaDonController : Controller
    {
        // Kết nối DbContext (EF)
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // ==========================
        // Index: danh sách hóa đơn
        // ==========================
        public ActionResult Index(int? page, string search, string from, string to, string sort)
        {
            // page + pageSize cho phân trang
            int pageNumber = page ?? 1;
            int pageSize = 10;

            // IMPORTANT: Include navigation properties để tránh lazy-load/proxy lỗi khi view truy cập
            // Include NhanVien, KhachHang, ChiTietHoaDon để view có thể dùng hd.NhanVien?.TenNV và hd.ChiTietHoaDon
            var q = _db.HoaDon
                       .Include(h => h.NhanVien)       // load nhân viên (TenNV)
                       .Include(h => h.KhachHang)      // load khách hàng (TenKH)
                       .Include(h => h.ChiTietHoaDon)  // load chi tiết (nếu cần tính toán)
                       .AsQueryable();

            // 1) Tìm kiếm: theo MaHD / MaKH nếu nhập số, nếu chuỗi thì theo tên KH hoặc tên NV
            if (!string.IsNullOrEmpty(search))
            {
                string s = search.Trim();
                if (int.TryParse(s, out int id))
                {
                    q = q.Where(h => h.MaHD == id || h.MaKH == id);
                }
                else
                {
                    q = q.Where(h =>
                        (h.KhachHang != null && h.KhachHang.TenKH.Contains(s)) ||
                        (h.NhanVien != null && h.NhanVien.TenNV.Contains(s))
                    );
                }
            }

            // 2) Lọc ngày: từ -> đến (tính đúng thời điểm bắt đầu/kết thúc)
            if (!string.IsNullOrEmpty(from) && DateTime.TryParse(from, out DateTime tuNgay))
            {
                var minDate = tuNgay.Date; // 00:00:00
                q = q.Where(h => h.Ngay >= minDate);
            }

            if (!string.IsNullOrEmpty(to) && DateTime.TryParse(to, out DateTime denNgay))
            {
                var maxDate = denNgay.Date.AddDays(1).AddTicks(-1); // 23:59:59.999...
                q = q.Where(h => h.Ngay <= maxDate);
            }

            // 3) Sắp xếp (mặc định: MaHD tăng dần theo yêu cầu)
            if (string.IsNullOrEmpty(sort))
            {
                q = q.OrderBy(h => h.MaHD);
            }
            else
            {
                switch (sort)
                {
                    case "ngay_asc":
                        q = q.OrderBy(h => h.Ngay);
                        break;
                    case "ngay_desc":
                        q = q.OrderByDescending(h => h.Ngay);
                        break;
                    case "mahd_desc":
                        q = q.OrderByDescending(h => h.MaHD);
                        break;
                    case "mahd_asc":
                        q = q.OrderBy(h => h.MaHD);
                        break;
                    default:
                        q = q.OrderBy(h => h.MaHD);
                        break;
                }
            }

            // 4) Phân trang và trả view
            var list = q.ToList(); // thực thi query
            return View(list.ToPagedList(pageNumber, pageSize));
        }


        // =====================================================
        // GET: Partial - form thêm (trả PartialView)
        // =====================================================
        [HttpGet]
        public ActionResult Them()
        {
            // load selectlist KH và NV cho form thêm
            ViewBag.KhachHang = new SelectList(_db.KhachHang.OrderBy(k => k.TenKH).ToList(), "MaKH", "TenKH");
            ViewBag.NhanVien = new SelectList(_db.NhanVien.OrderBy(n => n.TenNV).ToList(), "MaNV", "TenNV");
            return PartialView("_ThemHoaDon");
        }

        // =====================================================
        // POST: Thêm hóa đơn (JSON result)
        // =====================================================
        [HttpPost]
        public JsonResult Them(HoaDon model)
        {
            try
            {
                if (model == null)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

                // nếu không truyền ngày → set hiện tại
                model.Ngay = model.Ngay == default(DateTime) ? DateTime.Now : model.Ngay;

                // nếu không truyền MaNV → mặc định 1 (bạn có thể thay)
                if (model.MaNV == 0) model.MaNV = 1;

                _db.HoaDon.Add(model);
                _db.SaveChanges();

                return Json(new { success = true, message = "Đã tạo hóa đơn (Mã: " + model.MaHD + ")." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi thêm: " + ex.Message });
            }
        }

        // =====================================================
        // GET: Partial - form sửa
        // =====================================================
        [HttpGet]
        public ActionResult Sua(int id)
        {
            var hd = _db.HoaDon.Find(id);
            if (hd == null) return HttpNotFound();

            // load selectlist với giá trị hiện tại được chọn
            ViewBag.KhachHang = new SelectList(_db.KhachHang.OrderBy(k => k.TenKH).ToList(), "MaKH", "TenKH", hd.MaKH);
            ViewBag.NhanVien = new SelectList(_db.NhanVien.OrderBy(n => n.TenNV).ToList(), "MaNV", "TenNV", hd.MaNV);
            return PartialView("_SuaHoaDon", hd);
        }

        // =====================================================
        // POST: Sửa hóa đơn
        // =====================================================
        [HttpPost]
        public JsonResult Sua(HoaDon model)
        {
            try
            {
                if (model == null) return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

                var old = _db.HoaDon.Find(model.MaHD);
                if (old == null) return Json(new { success = false, message = "Không tìm thấy hóa đơn." });

                // cập nhật các trường cho phép
                old.Ngay = model.Ngay == default(DateTime) ? old.Ngay : model.Ngay;
                old.MaKH = model.MaKH;
                old.MaNV = model.MaNV;

                _db.SaveChanges();
                return Json(new { success = true, message = "Đã cập nhật hóa đơn." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật: " + ex.Message });
            }
        }

        // =====================================================
        // GET: Chi tiết hóa đơn (TRẢ List<ChiTietItemVM> — KHÔNG trả anonymous)
        // =====================================================
        [HttpGet]
        public ActionResult ChiTiet(int id)
        {
            // Lấy hóa đơn kèm navigation (dùng trong partial)
            var hoaDon = _db.HoaDon
                            .Include(h => h.NhanVien)
                            .Include(h => h.KhachHang)
                            .FirstOrDefault(h => h.MaHD == id);

            if (hoaDon == null) return HttpNotFound();

            // Lấy chi tiết và project vào ChiTietItemVM (loại bỏ anonymous/dynamic)
            var chiTietVm = _db.ChiTietHoaDon
                .Where(ct => ct.MaHD == id)
                .Select(ct => new ChiTietItemVM
                {
                    MaSP = ct.MaSP,
                    TenSP = ct.SanPham != null ? ct.SanPham.TenSP : "(SP mất)",
                    SoLuong = ct.SoLuong,
                    DonGia = (decimal)ct.DonGia // cast sang decimal nếu DonGia là int
                })
                .ToList();

            // Tính tổng tiền từ VM
            var tong = chiTietVm.Sum(x => x.ThanhTien);

            // Gắn vào ViewBag để partial dùng (hoặc bạn có thể tạo full VM chứa cả HoaDon + ChiTiet list)
            ViewBag.TongTien = tong;
            ViewBag.HoaDon = hoaDon;

            // Trả partial view với kiểu rõ ràng IEnumerable<ChiTietItemVM>
            return PartialView("_ChiTietHoaDon", chiTietVm);
        }

        // =====================================================
        // POST: Xóa 1 chi tiết hóa đơn (theo MaCTHD)
        // Bạn có thể thay bằng MaSP nếu DB/logic của bạn dùng MaSP làm unique key trong ChiTiet
        // =====================================================
        [HttpPost]
        public JsonResult XoaChiTiet(int maCTHD)
        {
            try
            {
                var ct = _db.ChiTietHoaDon.Find(maCTHD);
                if (ct == null) return Json(new { success = false, message = "Không tìm thấy chi tiết." });

                _db.ChiTietHoaDon.Remove(ct);
                _db.SaveChanges();
                return Json(new { success = true, message = "Đã xóa dòng chi tiết." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa chi tiết: " + ex.Message });
            }
        }

        // =====================================================
        // POST: Xóa hóa đơn (cả chi tiết) - giữ nguyên
        // =====================================================
        [HttpPost]
        public JsonResult Xoa(int id)
        {
            try
            {
                var hoaDon = _db.HoaDon.Find(id);
                if (hoaDon == null) return Json(new { success = false, message = "Không tìm thấy hóa đơn để xóa!" });

                _db.HoaDon.Remove(hoaDon);
                _db.SaveChanges();
                return Json(new { success = true, message = "🗑️ Đã xóa hóa đơn thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "❌ Lỗi: " + ex.Message });
            }
        }

        // =====================================================
        // POST: Lọc theo ngày (trả partial danh sách - bạn đã có _DanhSachHoaDon)
        // =====================================================
        [HttpPost]
        public ActionResult LocTheoNgay(DateTime tuNgay, DateTime denNgay)
        {
            var hoaDons = _db.HoaDon
                .Where(h => h.Ngay >= tuNgay && h.Ngay <= denNgay)
                .OrderByDescending(h => h.Ngay)
                .ToList();

            return PartialView("_DanhSachHoaDon", hoaDons);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }


        public ActionResult ExportPDF(int id)
        {
            var hd = _db.HoaDon.Find(id);

            if (hd == null)
                return HttpNotFound();

            // (Bạn sẽ dùng Rotativa hoặc iTextSharp)
            return new Rotativa.ActionAsPdf("ChiTietPDF", new { id = id })
            {
                FileName = "HoaDon_" + id + ".pdf"
            };
        }

    }
}
