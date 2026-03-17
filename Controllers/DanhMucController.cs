using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using WebQuanLiCuaHangTapHoa.Models.ViewModels;

namespace WebQuanLiCuaHangTapHoa.Controllers
{
    public class DanhMucController : Controller
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // ===============================================================
        // 1️⃣ DANH SÁCH SẢN PHẨM THEO DANH MỤC (CHÍNH XÁC THEO DB)
        // ===============================================================
        public ActionResult DanhSach(int? maDM)
        {
            if (maDM == null)
                return RedirectToAction("TatCa");

            // Lấy tên danh mục
            var dm = _db.DanhMuc.FirstOrDefault(x => x.MaDM == maDM);
            ViewBag.TenDM = dm?.TenDM ?? "Không rõ";

            // Lấy sản phẩm + tồn kho
            var ds = (from sp in _db.SanPham
                      join k in _db.Kho on sp.MaSP equals k.MaSP
                      where sp.MaDM == maDM && sp.HoatDong == true
                      orderby sp.TenSP
                      select new SanPhamView
                      {
                          MaSP = sp.MaSP,
                          TenSP = sp.TenSP,
                          GiaBan = sp.GiaBan,
                          Ton = k.Ton,
                          MaDM = sp.MaDM,
                          HinhAnh = sp.HinhAnh
                      }).ToList();

            // Chuẩn hóa sidebar danh mục
            ViewBag.Cats = _db.DanhMuc
                .OrderBy(x => x.TenDM)
                .Select(x => new CatItemVM
                {
                    MaDM = x.MaDM,
                    TenDM = x.TenDM,
                    Count = _db.SanPham.Count(sp => sp.HoatDong && sp.MaDM == x.MaDM)
                }).ToList();

            ViewBag.Selected = maDM;

            return View(ds);
        }

        // ===============================================================
        // 2️⃣ MENU DANH MỤC TRONG LAYOUT
        // ===============================================================
        [ChildActionOnly]
        public ActionResult Menu()
        {
            var ds = _db.DanhMuc.OrderBy(x => x.TenDM).ToList();
            return PartialView("Menu", ds);
        }

        // ===============================================================
        // 3️⃣ TẤT CẢ SẢN PHẨM — TẢI LÊN LẠI VIEW DanhSach
        // ===============================================================
        public ActionResult TatCa()
        {
            var ds = (from sp in _db.SanPham
                      join k in _db.Kho on sp.MaSP equals k.MaSP
                      where sp.HoatDong == true
                      orderby sp.TenSP
                      select new SanPhamView
                      {
                          MaSP = sp.MaSP,
                          TenSP = sp.TenSP,
                          GiaBan = sp.GiaBan,
                          Ton = k.Ton,
                          MaDM = sp.MaDM,
                          HinhAnh = sp.HinhAnh
                      }).ToList();

            ViewBag.Cats = _db.DanhMuc
                .OrderBy(x => x.TenDM)
                .Select(x => new CatItemVM
                {
                    MaDM = x.MaDM,
                    TenDM = x.TenDM,
                    Count = _db.SanPham.Count(sp => sp.HoatDong && sp.MaDM == x.MaDM)
                }).ToList();

            ViewBag.Selected = 0;
            ViewBag.TenDM = "Tất cả sản phẩm";

            return View("DanhSach", ds);
        }

        // ===============================================================
        // 4️⃣ GIẢI PHÓNG DB
        // ===============================================================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();

            base.Dispose(disposing);
        }
    }
}
