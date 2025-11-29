using System;
using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using WebQuanLiCuaHangTapHoa.Areas.Admin.Models.ViewModels;
using PagedList;
using System.Collections.Generic;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        /* ============================================================
             INDEX – DASHBOARD CHÍNH
         ============================================================ */
        public ActionResult Index(DateTime? from, DateTime? to, int? page, int? pageKH)
        {
            int pageNumber = page ?? 1;
            int pageSize = 5;

            int pageNumberKH = pageKH ?? 1;
            int pageSizeKH = 10;

            /* ================= KPI + Doanh thu ================= */
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            int deltaToMonday = ((int)today.DayOfWeek + 6) % 7;
            DateTime weekStart = today.AddDays(-deltaToMonday);
            DateTime weekEnd = weekStart.AddDays(7);

            ViewBag.DoanhThuNgay = _db.HoaDon
                .Where(h => h.Ngay >= today && h.Ngay < tomorrow)
                .SelectMany(h => h.ChiTietHoaDon)
                .Sum(ct => (int?)ct.SoLuong * ct.DonGia) ?? 0;

            ViewBag.DoanhThuTuan = _db.HoaDon
                .Where(h => h.Ngay >= weekStart && h.Ngay < weekEnd)
                .SelectMany(h => h.ChiTietHoaDon)
                .Sum(ct => (int?)ct.SoLuong * ct.DonGia) ?? 0;

            ViewBag.TongSP = _db.SanPham.Count();
            ViewBag.TongKH = _db.KhachHang.Count();
            ViewBag.TongNV = _db.NhanVien.Count();
            ViewBag.TongHD = _db.HoaDon.Count();

            /* ================= Biểu đồ lợi nhuận ================= */
            var now = DateTime.Now;
            var start = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
            var end = new DateTime(now.Year, now.Month, 1).AddMonths(1);

            var labels = Enumerable.Range(0, 6)
                .Select(i => start.AddMonths(i).ToString("MM/yyyy"))
                .ToList();

            var thuChiThang = _db.ThuChi
                .Where(t => t.Ngay >= start && t.Ngay < end)
                .ToList()
                .GroupBy(t => t.Ngay.ToString("MM/yyyy"))
                .Select(g => new
                {
                    Label = g.Key,
                    Thu = g.Where(x => x.Loai == "Thu").Sum(x => x.SoTien),
                    Chi = g.Where(x => x.Loai == "Chi").Sum(x => x.SoTien)
                })
                .ToList();

            var thu = labels.Select(l => thuChiThang.FirstOrDefault(x => x.Label == l)?.Thu ?? 0).ToList();
            var chi = labels.Select(l => thuChiThang.FirstOrDefault(x => x.Label == l)?.Chi ?? 0).ToList();
            var loi = thu.Zip(chi, (t, c) => t - c).ToList();

            ViewBag.ProfitLabelsJson = System.Web.Helpers.Json.Encode(labels);
            ViewBag.ProfitValuesJson = System.Web.Helpers.Json.Encode(loi);

            /* ================= TOP SẢN PHẨM ================= */
            var queryTopSP = _db.ChiTietHoaDon
                .Where(ct => (from == null || ct.HoaDon.Ngay >= from)
                          && (to == null || ct.HoaDon.Ngay <= to))
                .GroupBy(ct => ct.MaSP)
                .Select(g => new
                {
                    MaSP = g.Key,
                    SoLuongBan = g.Sum(s => s.SoLuong)
                });

            var topSP = queryTopSP
                .Join(_db.SanPham,
                      g => g.MaSP,
                      sp => sp.MaSP,
                      (g, sp) => new TopBanChayVM
                      {
                          MaSP = sp.MaSP,
                          TenSP = sp.TenSP,
                          GiaBan = sp.GiaBan,
                          TenDanhMuc = sp.DanhMuc != null ? sp.DanhMuc.TenDM : "",
                          HoatDong = sp.HoatDong,
                          HinhAnh = sp.HinhAnh,
                          SoLuongBan = g.SoLuongBan,
                          TongDoanhThu = g.SoLuongBan * sp.GiaBan
                      })
                .OrderByDescending(x => x.SoLuongBan)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.TopBanChay = topSP;

            /* ================= TOP KHÁCH HÀNG – MẶC ĐỊNH A ================= */
            var topKH_query = _db.HoaDon
                .Join(_db.ChiTietHoaDon, hd => hd.MaHD, ct => ct.MaHD, (hd, ct) => new { hd, ct })
                .Join(_db.KhachHang, x => x.hd.MaKH, kh => kh.MaKH, (x, kh) => new
                {
                    kh.MaKH,
                    kh.TenKH,
                    kh.SoDT,
                    Tien = x.ct.SoLuong * x.ct.DonGia,
                    SL = x.ct.SoLuong,
                    MaHD = x.hd.MaHD
                });

            var topKH = topKH_query
                .GroupBy(x => new { x.MaKH, x.TenKH, x.SoDT })
                .Select(g => new TopKhachHangVM
                {
                    MaKH = g.Key.MaKH,
                    TenKH = g.Key.TenKH,
                    SoDT = g.Key.SoDT,
                    TongTien = g.Sum(x => x.Tien),
                    TongSoLuong = g.Sum(x => x.SL),
                    TongHoaDon = g.Select(z => z.MaHD).Distinct().Count()
                })
                .OrderByDescending(x => x.TongTien)
                .ToPagedList(pageNumberKH, pageSizeKH);

            ViewBag.TopKhachHang = topKH;
            ViewBag.CustomerMetric = 1; // ban đầu
            if (pageKH != null)
            {
                ViewBag.CustomerMetric = (int)(TempData["CustomerMetric"] ?? 1);
            }

            return View();
        }


        /* ============================================================
            FILTER DASHBOARD (AJAX)
        ============================================================ */
        public ActionResult FilterDashboard(int? month, int? year, DateTime? from, DateTime? to,
                                            int? page, int? pageKH, int customerMetric = 1)
        {

            TempData["CustomerMetric"] = customerMetric; // <-- BẮT BUỘC

            int pageNumber = page ?? 1;
            int pageSize = 5;

            int pageNumberKH = pageKH ?? 1;
            int pageSizeKH = 10;

            /* Tự tính ngày khi chọn tháng/năm */
            if (month.HasValue && year.HasValue)
            {
                try
                {
                    var start = new DateTime(year.Value, month.Value, 1);
                    var end = start.AddMonths(1).AddDays(-1);
                    from = start;
                    to = end;
                }
                catch { }
            }

            /* ================= KPI + BIỂU ĐỒ ================= */
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            int deltaToMonday = ((int)today.DayOfWeek + 6) % 7;
            DateTime weekStart = today.AddDays(-deltaToMonday);
            DateTime weekEnd = weekStart.AddDays(7);

            ViewBag.DoanhThuNgay = _db.HoaDon
                .Where(h => h.Ngay >= today && h.Ngay < tomorrow)
                .SelectMany(h => h.ChiTietHoaDon)
                .Sum(ct => (int?)ct.SoLuong * ct.DonGia) ?? 0;

            ViewBag.DoanhThuTuan = _db.HoaDon
                .Where(h => h.Ngay >= weekStart && h.Ngay < weekEnd)
                .SelectMany(h => h.ChiTietHoaDon)
                .Sum(ct => (int?)ct.SoLuong * ct.DonGia) ?? 0;

            ViewBag.TongSP = _db.SanPham.Count();
            ViewBag.TongKH = _db.KhachHang.Count();
            ViewBag.TongNV = _db.NhanVien.Count();
            ViewBag.TongHD = _db.HoaDon.Count();

            /* ================= BIỂU ĐỒ ================= */
            var now = DateTime.Now;
            var startMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
            var endMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);

            var labels = Enumerable.Range(0, 6)
                .Select(i => startMonth.AddMonths(i).ToString("MM/yyyy"))
                .ToList();

            var thuChiThang = _db.ThuChi
                .Where(t => t.Ngay >= startMonth && t.Ngay < endMonth)
                .ToList()
                .GroupBy(t => t.Ngay.ToString("MM/yyyy"))
                .Select(g => new
                {
                    Label = g.Key,
                    Thu = g.Where(x => x.Loai == "Thu").Sum(x => x.SoTien),
                    Chi = g.Where(x => x.Loai == "Chi").Sum(x => x.SoTien)
                })
                .ToList();

            var thu = labels.Select(l => thuChiThang.FirstOrDefault(x => x.Label == l)?.Thu ?? 0).ToList();
            var chi = labels.Select(l => thuChiThang.FirstOrDefault(x => x.Label == l)?.Chi ?? 0).ToList();
            var loi = thu.Zip(chi, (t, c) => t - c).ToList();

            ViewBag.ProfitLabelsJson = System.Web.Helpers.Json.Encode(labels);
            ViewBag.ProfitValuesJson = System.Web.Helpers.Json.Encode(loi);

            /* ================= TOP SẢN PHẨM ================= */
            var queryTopSP = _db.ChiTietHoaDon
                .Where(ct => (from == null || ct.HoaDon.Ngay >= from)
                          && (to == null || ct.HoaDon.Ngay <= to))
                .GroupBy(ct => ct.MaSP)
                .Select(g => new
                {
                    MaSP = g.Key,
                    SoLuongBan = g.Sum(s => s.SoLuong)
                });

            var topSP = queryTopSP
                .Join(_db.SanPham,
                      g => g.MaSP,
                      sp => sp.MaSP,
                      (g, sp) => new TopBanChayVM
                      {
                          MaSP = sp.MaSP,
                          TenSP = sp.TenSP,
                          GiaBan = sp.GiaBan,
                          TenDanhMuc = sp.DanhMuc != null ? sp.DanhMuc.TenDM : "",
                          HoatDong = sp.HoatDong,
                          HinhAnh = sp.HinhAnh,
                          SoLuongBan = g.SoLuongBan,
                          TongDoanhThu = g.SoLuongBan * sp.GiaBan
                      })
                .OrderByDescending(x => x.SoLuongBan)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.TopBanChay = topSP;

            /* ================= TOP KHÁCH HÀNG A/B/C ================= */
            var queryKH = _db.HoaDon
                .Join(_db.ChiTietHoaDon, hd => hd.MaHD, ct => ct.MaHD, (hd, ct) => new { hd, ct })
                .Join(_db.KhachHang, x => x.hd.MaKH, kh => kh.MaKH, (x, kh) => new
                {
                    kh.MaKH,
                    kh.TenKH,
                    kh.SoDT,
                    Tien = x.ct.SoLuong * x.ct.DonGia,
                    SL = x.ct.SoLuong,
                    MaHD = x.hd.MaHD
                });

            List<TopKhachHangVM> rawKH = queryKH
                .GroupBy(x => new { x.MaKH, x.TenKH, x.SoDT })
                .Select(g => new TopKhachHangVM
                {
                    MaKH = g.Key.MaKH,
                    TenKH = g.Key.TenKH,
                    SoDT = g.Key.SoDT,
                    TongTien = g.Sum(x => x.Tien),
                    TongSoLuong = g.Sum(x => x.SL),
                    TongHoaDon = g.Select(z => z.MaHD).Distinct().Count()
                })
                .ToList();

            /* Metric */
            if (customerMetric == 1)
                rawKH = rawKH.OrderByDescending(x => x.TongTien).ToList();
            else if (customerMetric == 2)
                rawKH = rawKH.OrderByDescending(x => x.TongSoLuong).ToList();
            else if (customerMetric == 3)
                rawKH = rawKH.OrderByDescending(x => x.TongHoaDon).ToList();

            var topKH = rawKH.ToPagedList(pageNumberKH, pageSizeKH);

            ViewBag.TopKhachHang = topKH;
            ViewBag.CustomerMetric = customerMetric;

            return PartialView("_DashboardStatsPartial");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
