// Nạp thư viện cần thiết
using System.Linq;                                 // Cho LINQ: Where(), OrderBy()
using System.Web.Mvc;                              // Cho Controller, ActionResult
using WebQuanLiCuaHangTapHoa.Models;               // Cho context + model từ EDMX + ViewModel
using WebQuanLiCuaHangTapHoa.Models.ViewModels;    // Cho ViewModel

namespace WebQuanLiCuaHangTapHoa.Controllers
{
    public class DanhMucController : Controller
    {
        // Đối tượng context để làm việc với CSDL
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // ===============================================================
        // 1️⃣ Action: DanhSach — Hiển thị sản phẩm theo mã danh mục
        // URL: /DanhMuc/DanhSach?maDM=1
        // ===============================================================
        public ActionResult DanhSach(int maDM)
        {
            // Lấy tên danh mục cho tiêu đề
            var dm = _db.DanhMuc.FirstOrDefault(x => x.MaDM == maDM);
            ViewBag.TenDM = dm?.TenDM ?? "Không rõ";

            // Lấy danh sách sản phẩm thuộc danh mục này
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
                          MaDM = sp.MaDM
                      }).ToList();

            // Danh mục sidebar
            var cats = _db.DanhMuc
                          .OrderBy(x => x.TenDM)
                          .Select(x => new CatItemVM
                          {
                              MaDM = x.MaDM,
                              TenDM = x.TenDM,
                              Count = _db.SanPham.Count(sp => sp.HoatDong == true && sp.MaDM == x.MaDM)
                          }).ToList();

            ViewBag.Cats = cats;
            ViewBag.Selected = maDM;

            return View(ds);
        }

        // ===============================================================
        // 2️⃣ Action: Menu — danh mục động gọi trong layout
        // ===============================================================
        [ChildActionOnly]
        public ActionResult Menu()
        {
            var ds = _db.DanhMuc.OrderBy(x => x.TenDM).ToList();
            return PartialView("Menu", ds);
        }

        // ===============================================================
        // 3️⃣ Action: TatCa — Hiển thị toàn bộ sản phẩm khi bấm "Mua ngay"
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
                          MaDM = sp.MaDM
                      }).ToList();

            var cats = _db.DanhMuc
                          .OrderBy(x => x.TenDM)
                          .Select(x => new CatItemVM
                          {
                              MaDM = x.MaDM,
                              TenDM = x.TenDM,
                              Count = _db.SanPham.Count(sp => sp.HoatDong == true && sp.MaDM == x.MaDM)
                          }).ToList();

            ViewBag.Cats = cats;
            ViewBag.Selected = 0;
            ViewBag.TenDM = "Tất cả sản phẩm";

            return View("DanhSach", ds);
        }

        // ===============================================================
        // 4️⃣ Giải phóng DbContext
        // ===============================================================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();

            base.Dispose(disposing);
        }
    }
}
