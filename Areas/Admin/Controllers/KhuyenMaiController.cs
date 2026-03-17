using System;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using WebQuanLiCuaHangTapHoa.Models;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class KhuyenMaiController : BaseController
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // INDEX (list + paging)
        public ActionResult Index(int page = 1)
        {
            int pageSize = 15;
            var list = _db.KhuyenMai
                .OrderByDescending(x => x.TuNgay)
                .ToList();
            return View(list.ToPagedList(page, pageSize));
        }

        // GET: Partial - create form
        [HttpGet]
        public ActionResult Them()
        {
            return PartialView("_ThemKhuyenMai");
        }

        // POST: create
        [HttpPost]
        public JsonResult Them(KhuyenMai model)
        {
            try
            {
                if (model == null) return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                model.TuNgay = model.TuNgay.ToLocalTime();
                model.DenNgay = model.DenNgay.ToLocalTime();

                _db.KhuyenMai.Add(model);
                _db.SaveChanges();

                return Json(new { success = true, message = "Đã thêm khuyến mãi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Partial - edit form
        [HttpGet]
        public ActionResult Sua(int id)
        {
            var km = _db.KhuyenMai.Find(id);
            if (km == null) return HttpNotFound();
            return PartialView("_SuaKhuyenMai", km);
        }

        // POST: edit
        [HttpPost]
        public JsonResult Sua(KhuyenMai model)
        {
            try
            {
                var old = _db.KhuyenMai.Find(model.MaKM);
                if (old == null) return Json(new { success = false, message = "Không tìm thấy khuyến mãi." });

                old.TenKM = model.TenKM;
                old.Giam = model.Giam;
                old.TuNgay = model.TuNgay;
                old.DenNgay = model.DenNgay;

                _db.SaveChanges();
                return Json(new { success = true, message = "Đã cập nhật khuyến mãi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: delete
        [HttpPost]
        public JsonResult Xoa(int id)
        {
            try
            {
                var item = _db.KhuyenMai.Find(id);
                if (item == null) return Json(new { success = false, message = "Không tìm thấy." });

                _db.KhuyenMai.Remove(item);
                _db.SaveChanges();

                return Json(new { success = true, message = "Đã xóa." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}