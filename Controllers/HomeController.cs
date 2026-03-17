using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;

namespace WebQuanLiCuaHangTapHoa.Controllers
{
    public class HomeController : Controller
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db
            = new QuanLyTapHoaThanhNhanEntities1();

        // ===================== TRANG CHỦ =====================
        public ActionResult Index(string kw = null, int? maDM = null)
        {
            ViewBag.Keyword = kw;
            ViewBag.MaDM = maDM;
            return View();
        }

        // ============================================================
        // 1️⃣ NEW PRODUCTS – LẤY SẢN PHẨM MỚI (MaSP mới nhất)
        // ============================================================
        [ChildActionOnly]
        public ActionResult NewProducts(int take = 100)
        {
            var ds = _db.SanPham
                .Where(s => s.HoatDong == true)
                .OrderByDescending(s => s.MaSP)
                .Take(take)
                .Select(s => new SanPhamView
                {
                    MaSP = s.MaSP,
                    TenSP = s.TenSP,
                    GiaBan = s.GiaBan,
                    HinhAnh = s.HinhAnh,
                    MaDM = s.MaDM,
                    Ton = _db.Kho.Where(k => k.MaSP == s.MaSP)
                                 .Select(k => (int?)k.Ton)
                                 .FirstOrDefault() ?? 0
                })
                .ToList();

            return PartialView("~/Views/Shared/_NewProductsPartial.cshtml", ds);
        }

        // ============================================================
        // 2️⃣ TOP TODAY – SẢN PHẨM BÁN CHẠY HÔM NAY (Có Flip 3D)
        // ============================================================
        // ============================================================
        // 2️⃣ TOP TODAY – SẢN PHẨM BÁN CHẠY HÔM NAY (THIẾT KẾ GỐC: _TopFlipPartial)
        // ============================================================
        [ChildActionOnly]
        public ActionResult TopToday(int take = 5)
        {
            var today = DateTime.Today;

            var ds = (from hd in _db.HoaDon
                      join ct in _db.ChiTietHoaDon on hd.MaHD equals ct.MaHD
                      join sp in _db.SanPham on ct.MaSP equals sp.MaSP
                      where sp.HoatDong == true && hd.Ngay >= today
                      group ct by new
                      {
                          sp.MaSP,
                          sp.TenSP,
                          sp.GiaBan,
                          sp.HinhAnh,
                          sp.MaDM,
                          sp.MoTaNgan
                      } into g
                      orderby g.Sum(x => x.SoLuong) descending
                      select new SanPhamView
                      {
                          MaSP = g.Key.MaSP,
                          TenSP = g.Key.TenSP,
                          GiaBan = g.Key.GiaBan,
                          HinhAnh = g.Key.HinhAnh,
                          MaDM = g.Key.MaDM,
                          MoTaNgan = g.Key.MoTaNgan
                      })
                      .Take(take)
                      .ToList();

            // báo cho partial biết có hay không dữ liệu
            ViewBag.NoDataTopToday = ds.Count == 0;

            // TRẢ VỀ ĐÚNG PHIÊN BẢN GỐC: _TopFlipPartial.cshtml
            return PartialView("~/Views/Shared/_TopFlipPartial.cshtml", ds);
        }

        // ============================================================
        // 3️⃣ TOP SELLING – SẢN PHẨM BÁN CHẠY NHẤT (Tổng doanh số)
        // ============================================================
        [ChildActionOnly]
        public ActionResult TopSelling(int take = 100)
        {
            var ds = (from ct in _db.ChiTietHoaDon
                      join sp in _db.SanPham on ct.MaSP equals sp.MaSP
                      where sp.HoatDong == true
                      group ct by new
                      {
                          sp.MaSP,
                          sp.TenSP,
                          sp.GiaBan,
                          sp.HinhAnh,
                          sp.MaDM
                      } into g
                      orderby g.Sum(x => x.SoLuong) descending
                      select new SanPhamView
                      {
                          MaSP = g.Key.MaSP,
                          TenSP = g.Key.TenSP,
                          GiaBan = g.Key.GiaBan,
                          HinhAnh = g.Key.HinhAnh,
                          MaDM = g.Key.MaDM,
                          Ton = _db.Kho.Where(k => k.MaSP == g.Key.MaSP)
                                       .Select(k => (int?)k.Ton)
                                       .FirstOrDefault() ?? 0
                      })
                      .Take(take)
                      .ToList();

            return PartialView("~/Views/Shared/_TopSellingPartial.cshtml", ds);
        }

        // ======================== KHUYẾN MÃI ========================
        [ChildActionOnly]
        public ActionResult KhuyenMai()
        {
            var ds = _db.SanPham
                .Where(sp => sp.HoatDong == true && sp.MaKM != null)
                .Include("KhuyenMai")
                .ToList();

            return PartialView("~/Views/Shared/_KhuyenMaiPartial.cshtml", ds);
        }

        // ======================== GIỚI THIỆU ========================
        public ActionResult GioiThieu()
        {
            return View();
        }

        // ======================== DEBUG CATEGORIES ========================
        public ActionResult DebugCategories()
        {
            var danhMucs = _db.DanhMuc.ToList();
            return View(danhMucs);
        }

        public ActionResult KienThucHome()
        {
            var data = _db.KienThuc
                         .Where(x => x.TrangThai == true)
                         .OrderByDescending(x => x.NgayDang)
                         .Take(4)
                         .ToList();

            return PartialView("_KnowledgePartial", data);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
