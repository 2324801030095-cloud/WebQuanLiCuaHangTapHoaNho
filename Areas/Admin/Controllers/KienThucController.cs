using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using WebQuanLiCuaHangTapHoa.Models;
using PagedList;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class KienThucController : BaseController
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // ===========================================================
        // INDEX - Danh sách bài viết kiến thức
        // ===========================================================
        public ActionResult Index(int? page, string search, bool? trangThai)
        {
            int pageNumber = page ?? 1;
            int pageSize = 15;

            var query = _db.KienThuc.AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(k => 
                    k.TieuDe.Contains(search) || 
                    k.NoiDung.Contains(search) ||
                    (k.Slug != null && k.Slug.Contains(search))
                );
            }

            // Lọc theo trạng thái
            if (trangThai.HasValue)
            {
                query = query.Where(k => k.TrangThai == trangThai.Value);
            }

            var list = query
                .OrderByDescending(k => k.NgayDang)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.Search = search;
            ViewBag.TrangThai = trangThai;

            return View(list);
        }

        // ===========================================================
        // THÊM BÀI VIẾT
        // ===========================================================
        [HttpGet]
        public ActionResult Them()
        {
            return PartialView("_ThemKienThuc", new KienThuc 
            { 
                NgayDang = DateTime.Now,
                TrangThai = true
            });
        }

        [HttpPost]
        public JsonResult Them(KienThuc model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.TieuDe))
                    return Json(new { success = false, message = "Tiêu đề không được để trống." });

                // Tạo slug từ tiêu đề
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    model.Slug = model.TieuDe.ToLower()
                        .Replace(" ", "-")
                        .Replace("đ", "d")
                        .Replace("Đ", "D");
                }

                model.NgayDang = DateTime.Now;
                model.NgayCapNhat = DateTime.Now;
                model.LuotXem = 0;

                _db.KienThuc.Add(model);
                _db.SaveChanges();

                return Json(new { success = true, message = "Đã thêm bài viết." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ===========================================================
        // SỬA BÀI VIẾT
        // ===========================================================
        [HttpGet]
        public ActionResult Sua(int id)
        {
            var kt = _db.KienThuc.Find(id);
            if (kt == null) return HttpNotFound();

            return PartialView("_SuaKienThuc", kt);
        }

        [HttpPost]
        public JsonResult Sua(KienThuc model)
        {
            try
            {
                var old = _db.KienThuc.Find(model.MaKT);
                if (old == null)
                    return Json(new { success = false, message = "Không tìm thấy bài viết." });

                old.TieuDe = model.TieuDe;
                old.NoiDung = model.NoiDung;
                old.TrangThai = model.TrangThai;
                old.NgayCapNhat = DateTime.Now;

                if (!string.IsNullOrWhiteSpace(model.Slug))
                    old.Slug = model.Slug;

                _db.SaveChanges();

                return Json(new { success = true, message = "Đã cập nhật bài viết." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ===========================================================
        // XÓA BÀI VIẾT
        // ===========================================================
        [HttpPost]
        public JsonResult Xoa(int id)
        {
            try
            {
                var kt = _db.KienThuc.Find(id);
                if (kt == null)
                    return Json(new { success = false, message = "Không tìm thấy bài viết." });

                _db.KienThuc.Remove(kt);
                _db.SaveChanges();

                return Json(new { success = true, message = "Đã xóa bài viết." });
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

