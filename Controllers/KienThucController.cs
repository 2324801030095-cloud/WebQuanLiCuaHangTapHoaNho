using System;
using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;

namespace WebQuanLiCuaHangTapHoa.Controllers
{
    public class KienThucController : Controller
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db
            = new QuanLyTapHoaThanhNhanEntities1();

        // =========================
        // 1️⃣ TRANG DANH SÁCH KIẾN THỨC
        // =========================
        public ActionResult DanhSach()
        {
            var list = _db.KienThuc
                          .Where(x => x.TrangThai == true)
                          .OrderByDescending(x => x.NgayDang)
                          .ToList();

            return View(list);
        }

        // =========================
        // 2️⃣ TRANG CHI TIẾT KIẾN THỨC (SEO URL)
        // =========================
        public ActionResult ChiTiet(string slug)
        {
            if (slug == null) return HttpNotFound();

            var kt = _db.KienThuc
                        .FirstOrDefault(x => x.Slug == slug && x.TrangThai == true);

            if (kt == null) return HttpNotFound();

            // Tăng lượt xem
            kt.LuotXem += 1;
            kt.NgayCapNhat = DateTime.Now;
            _db.SaveChanges();

            return View(kt); // model = 1 bài viết
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
