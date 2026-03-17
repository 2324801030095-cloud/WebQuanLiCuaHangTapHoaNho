using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using WebQuanLiCuaHangTapHoa.Models;
using WebQuanLiCuaHangTapHoa.Models.ViewModels; // <-- add

namespace WebQuanLiCuaHangTapHoa.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // ================== CHI TIẾT SẢN PHẨM ==================
        public ActionResult ChiTiet(int id)
        {
            try
            {
                // 1️⃣ Lấy sản phẩm theo ID (include liên quan để tránh NullReference / lazy load issues)
                var sp = _db.SanPham
                            .Include(s => s.DanhMuc)
                            .Include(s => s.DonViTinh)
                            .Include(s => s.KhuyenMai)
                            .FirstOrDefault(x => x.MaSP == id && x.HoatDong == true);

                if (sp == null) return HttpNotFound();

                // 2️⃣ Lấy tồn kho (nếu không có thì = 0)
                var ton = _db.Kho.Where(k => k.MaSP == id)
                                 .Select(k => (int?)k.Ton)
                                 .FirstOrDefault() ?? 0;

                // 3️⃣ Lấy danh mục + đơn vị tính
                var dm = sp.DanhMuc; // already included
                var dvt = sp.DonViTinh; // already included

                // 4️⃣ TÍNH GIẢM GIÁ
                decimal? giam = null;

                // Nếu trong bảng SanPham có cột KhuyenMai1 (số nguyên phần trăm)
                if (sp.KhuyenMai1.HasValue)
                {
                    giam = sp.KhuyenMai1.Value;
                }
                else if (sp.KhuyenMai != null)
                {
                    // Nếu có navigation KhuyenMai, lấy trường Giam (decimal)
                    giam = sp.KhuyenMai.Giam;
                }

                // Giá sau KM = Giá gốc mặc định
                var giaSauKM = sp.GiaBan;

                if (giam.HasValue && giam.Value > 0)
                {
                    var soTienGiam = (int)Math.Round(sp.GiaBan * (double)giam.Value / 100.0);
                    giaSauKM = Math.Max(0, sp.GiaBan - soTienGiam);
                }

                // 5️⃣ Tạo ViewModel gửi sang View
                var vm = new SanPhamDetailVM
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    GiaBan = sp.GiaBan,
                    GiaSauKM = giaSauKM,
                    Ton = ton,

                    MaDM = sp.MaDM,
                    TenDM = dm?.TenDM,
                    DonViTinh = dvt?.TenDVT,

                    HinhAnh = string.IsNullOrWhiteSpace(sp.HinhAnh)
                              ? $"https://picsum.photos/800/600?random={sp.MaSP}"
                              : sp.HinhAnh,

                    MoTaNgan = sp.MoTaNgan,
                    MoTaChiTiet = sp.MoTaChiTiet,

                    Giam = giam,          // phần trăm giảm
                    TenKM = sp.KhuyenMai != null ? sp.KhuyenMai.TenKM : null,
                    TuNgay = sp.KhuyenMai != null ? (DateTime?)sp.KhuyenMai.TuNgay : null,
                    DenNgay = sp.KhuyenMai != null ? (DateTime?)sp.KhuyenMai.DenNgay : null
                };

                // 6️⃣ Trả về view kèm model
                ViewBag.Title = sp.TenSP;
                return View(vm);
            }
            catch (Exception ex)
            {
                // Ghi log đơn giản
                try { System.Diagnostics.Trace.TraceError("ChiTiet error: " + ex.ToString()); } catch { }

                // Trả về view lỗi thân thiện (không trả 500 raw)
                ViewBag.ErrorMessage = "Không thể tải chi tiết sản phẩm. Vui lòng thử lại sau.";
                ViewBag.ErrorDetails = ex.Message; // ít thông tin - không lộ stack
                return View("ChiTietError");
            }
        }

        // ================== SẢN PHẨM LIÊN QUAN ==================
        [ChildActionOnly]
        public ActionResult LienQuan(int id, int take = 6)
        {
            var maDM = _db.SanPham.Where(s => s.MaSP == id)
                                   .Select(s => s.MaDM)
                                   .FirstOrDefault();

            var ds = (from s in _db.SanPham
                      where s.HoatDong == true
                         && s.MaDM == maDM
                         && s.MaSP != id
                      orderby s.MaSP descending
                      select new SanPhamView
                      {
                          MaSP = s.MaSP,
                          TenSP = s.TenSP,
                          GiaBan = s.GiaBan,
                          Ton = _db.Kho.Where(k => k.MaSP == s.MaSP)
                                       .Select(k => (int?)k.Ton)
                                       .FirstOrDefault() ?? 0,
                          MaDM = s.MaDM,
                          HinhAnh = s.HinhAnh   // ⭐ THÊM DÒNG NÀY – BẮT BUỘC
                      })
                      .Take(take)
                      .ToList();

            return PartialView("~/Views/Shared/_ProductGridPartial.cshtml", ds);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
