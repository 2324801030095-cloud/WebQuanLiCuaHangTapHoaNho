// using: nap MVC, LINQ, va EDMX Models
using System.Linq;                                   // Dung cho Count(), Take()
using System.Web.Mvc;                                // Dung cho Controller, ActionResult
using WebQuanLiCuaHangTapHoa.Models;                 // Dung context + entity sinh tu EDMX

namespace WebQuanLiCuaHangTapHoa.Controllers         // Namespace phai dung voi project
{
    // Controller kiem tra ket noi CSDL bang EDMX
    public class TestKetNoiController : Controller
    {
        // Tao context de lam viec voi DB
        private readonly QuanLyTapHoaThanhNhanEntities1 _db
            = new QuanLyTapHoaThanhNhanEntities1();   // Doi tuong DbContext dung connectionString trong Web.config

        // Action ping: tra ve so dong bang co trong DB de test nhanh
        public ActionResult Ping()
        {
            // Dem so ban ghi o mot vai bang de xac nhan co du lieu
            var soDanhMuc = _db.DanhMuc.Count();      // Dem so danh muc
            var soSanPham = _db.SanPham.Count();      // Dem so san pham
            var soKhach = _db.KhachHang.Count();    // Dem so khach hang (neu co)

            // Dua len ViewBag de hien thi don gian
            ViewBag.SoDanhMuc = soDanhMuc;
            ViewBag.SoSanPham = soSanPham;
            ViewBag.SoKhach = soKhach;

            // Lay 5 danh muc dau tien de hien thi
            var topDanhMuc = _db.DanhMuc
                                .OrderBy(dm => dm.TenDM)
                                .Take(5)
                                .ToList();

            return View(topDanhMuc);                  // Tra ve View manh kieu IEnumerable<DanhMuc>
        }

        // Giai phong DbContext dung cach
        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();             // Dong DbContext (co san vi ke thua DbContext)
            base.Dispose(disposing);                   // Goi Dispose cua lop cha
        }
    }
}
