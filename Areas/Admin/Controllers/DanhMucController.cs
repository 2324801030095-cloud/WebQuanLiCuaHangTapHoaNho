using System;
using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using WebQuanLiCuaHangTapHoa.Areas.Admin.Models.ViewModels;
using PagedList;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class DanhMucController : Controller
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // =========================
        // 📄 Danh sách danh mục
        // =========================
        public ActionResult Index(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;

            var danhMucs = _db.DanhMuc
                .Select(dm => new DanhMucVM
                {
                    MaDM = dm.MaDM,
                    TenDM = dm.TenDM,
                    MoTa = dm.MoTa,
                    SoSP = _db.SanPham.Count(sp => sp.MaDM == dm.MaDM)
                })
                .OrderBy(dm => dm.MaDM)
                .ToPagedList(pageNumber, pageSize);

            return View(danhMucs);
        }

        // =========================
        // 👁 Xem chi tiết danh mục
        // =========================
        [HttpGet]
        public ActionResult ChiTietDanhMuc(int id)
        {
            var dm = _db.DanhMuc.Find(id);
            if (dm == null) return HttpNotFound();

            var sanPhams = _db.SanPham
                .Where(sp => sp.MaDM == id)
                .Select(sp => new ProductInCategoryVM
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    GiaBan = sp.GiaBan,
                    HinhAnh = sp.HinhAnh,
                    HoatDong = sp.HoatDong
                })
                .OrderBy(sp => sp.MaSP)
                .ToList();

            ViewBag.TenDM = dm.TenDM;
            ViewBag.MaDM = dm.MaDM;

            return PartialView("_ChiTietDanhMuc", sanPhams);
        }

        // =========================
        // ➕ Thêm danh mục
        // =========================
        [HttpGet]
        public ActionResult Them()
        {
            var model = new DanhMuc();
            return PartialView("_ThemDanhMuc", model);
        }

        [HttpPost]
        public JsonResult Them(DanhMuc dm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dm.TenDM))
                    return Json(new { success = false, message = "⚠️ Tên danh mục không được để trống!" });

                _db.DanhMuc.Add(dm);
                _db.SaveChanges();

                return Json(new { success = true, message = "✅ Thêm danh mục thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "❌ Lỗi: " + ex.Message });
            }
        }

        // =========================
        // ✏️ Sửa danh mục
        // =========================
        [HttpGet]
        public ActionResult Sua(int id)
        {
            var dm = _db.DanhMuc.Find(id);
            if (dm == null) return HttpNotFound();

            return PartialView("_SuaDanhMuc", dm);
        }

        [HttpPost]
        public JsonResult Sua(DanhMuc dm)
        {
            try
            {
                var old = _db.DanhMuc.Find(dm.MaDM);
                if (old == null)
                    return Json(new { success = false, message = "Không tìm thấy danh mục để cập nhật!" });

                old.TenDM = dm.TenDM;
                old.MoTa = dm.MoTa;
                _db.SaveChanges();

                return Json(new { success = true, message = "✅ Cập nhật danh mục thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "❌ Lỗi: " + ex.Message });
            }
        }

        // =========================
        // ❌ Xóa danh mục
        // =========================
        [HttpPost]
        public JsonResult Xoa(int id)
        {
            try
            {
                var dm = _db.DanhMuc.Find(id);
                if (dm == null)
                    return Json(new { success = false, message = "Không tìm thấy danh mục để xóa!" });

                _db.DanhMuc.Remove(dm);
                _db.SaveChanges();

                return Json(new { success = true, message = "🗑️ Đã xóa danh mục thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "❌ Lỗi: " + ex.Message });
            }
        }
    }

    // ViewModel cho bảng danh mục
    public class DanhMucVM
    {
        public int MaDM { get; set; }
        public string TenDM { get; set; }
        public string MoTa { get; set; }
        public int SoSP { get; set; }
    }
}
