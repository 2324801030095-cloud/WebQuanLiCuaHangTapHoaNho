using System;
using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;

namespace WebQuanLiCuaHangTapHoa.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // ================== CHI TIẾT SẢN PHẨM ==================
        public ActionResult ChiTiet(int id)
        {
            // 1️⃣ Lấy sản phẩm theo ID
            var sp = _db.SanPham.FirstOrDefault(x => x.MaSP == id && x.HoatDong == true);
            if (sp == null) return HttpNotFound();

            // 2️⃣ Lấy tồn kho (nếu không có thì = 0)
            var ton = _db.Kho.Where(k => k.MaSP == id)
                             .Select(k => (int?)k.Ton)
                             .FirstOrDefault() ?? 0;

            // 3️⃣ Lấy danh mục + đơn vị tính
            var dm = _db.DanhMuc.FirstOrDefault(d => d.MaDM == sp.MaDM);
            var dvt = _db.DonViTinh.FirstOrDefault(d => d.MaDVT == sp.MaDVT);

            // 4️⃣ TÍNH GIẢM GIÁ
            // Trong CSDL, cột "KhuyenMai" của bảng SanPham chứa phần trăm giảm
            int? giam = null;

            try
            {
                // Nếu EDMX map KhuyenMai là kiểu số (int/decimal)
                giam = Convert.ToInt32(sp.KhuyenMai);
            }
            catch
            {
                // Nếu null hoặc kiểu string thì parse thử
                int temp;
                if (sp.KhuyenMai != null && int.TryParse(sp.KhuyenMai.ToString(), out temp))
                    giam = temp;
            }

            // Giá sau KM = Giá gốc mặc định
            var giaSauKM = sp.GiaBan;

            if (giam.HasValue && giam.Value > 0)
            {
                // Tính phần trăm giảm
                var soTienGiam = (int)Math.Round(sp.GiaBan * giam.Value / 100.0);
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

                Giam = giam,          // phần trăm giảm từ cột SanPham.KhuyenMai
                TenKM = "Khuyến mãi nội bộ", // text giả lập (nếu bạn muốn)
                TuNgay = null,
                DenNgay = null
            };

            // 6️⃣ Trả về view kèm model
            ViewBag.Title = sp.TenSP;
            return View(vm);
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
                          MaDM = s.MaDM
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
