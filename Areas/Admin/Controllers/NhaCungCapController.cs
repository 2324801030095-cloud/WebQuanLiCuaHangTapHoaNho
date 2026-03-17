using System;
using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using PagedList;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class NhaCungCapController : BaseController
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // ---------------------- INDEX ----------------------
        public ActionResult Index(string search, string diachi, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;

            var list = _db.NhaCungCap.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                list = list.Where(x => x.TenNCC.Contains(search));

            if (!string.IsNullOrWhiteSpace(diachi))
                list = list.Where(x => x.DiaChi.Contains(diachi));

            list = list.OrderBy(x => x.MaNCC);

            ViewBag.Search = search;
            ViewBag.DiaChi = diachi;

            return View(list.ToPagedList(pageNumber, pageSize));
        }

        // ---------------------- THÊM ----------------------
        [HttpGet]
        public ActionResult Them()
        {
            return PartialView("_ThemNhaCungCap");
        }

        [HttpPost]
        public JsonResult Them(NhaCungCap model)
        {
            try
            {
                if (model == null)
                    return Json(new { success = false, message = "Không nhận được dữ liệu" });

                if (string.IsNullOrWhiteSpace(model.TenNCC))
                    return Json(new { success = false, message = "Tên NCC không được để trống" });

                var ncc = new NhaCungCap
                {
                    TenNCC = model.TenNCC,
                    SoDT = model.SoDT,
                    DiaChi = model.DiaChi
                };

                _db.NhaCungCap.Add(ncc);
                _db.SaveChanges();

                return Json(new { success = true, message = "Thêm thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ---------------------- SỬA ----------------------
        [HttpGet]
        public ActionResult Sua(int id)
        {
            var ncc = _db.NhaCungCap.Find(id);
            if (ncc == null) return HttpNotFound();

            return PartialView("_SuaNhaCungCap", ncc);
        }

        [HttpPost]
        public JsonResult Sua(NhaCungCap model)
        {
            try
            {
                if (model == null)
                    return Json(new { success = false, message = "Không nhận được dữ liệu" });

                var old = _db.NhaCungCap.Find(model.MaNCC);
                if (old == null)
                    return Json(new { success = false, message = "Không tìm thấy nhà cung cấp" });

                old.TenNCC = model.TenNCC;
                old.SoDT = model.SoDT;
                old.DiaChi = model.DiaChi;

                _db.SaveChanges();

                return Json(new { success = true, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ---------------------- XOÁ ----------------------
        [HttpGet]
        public ActionResult Xoa(int id)
        {
            var ncc = _db.NhaCungCap.Find(id);
            if (ncc == null) return HttpNotFound();

            return PartialView("_XoaNhaCungCap", ncc);
        }

        [HttpPost]
        public JsonResult XoaConfirmed(int id)
        {
            try
            {
                var ncc = _db.NhaCungCap.Find(id);

                if (ncc == null)
                    return Json(new { success = false, message = "Không tìm thấy nhà cung cấp" });

                _db.NhaCungCap.Remove(ncc);
                _db.SaveChanges();

                return Json(new { success = true, message = "Đã xóa nhà cung cấp" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public ActionResult ChiTiet(int id)
        {
            var ncc = _db.NhaCungCap.Find(id);
            if (ncc == null)
                return HttpNotFound();

            return PartialView("_XemChiTietNhaCungCap", ncc);
        }


    }
}
