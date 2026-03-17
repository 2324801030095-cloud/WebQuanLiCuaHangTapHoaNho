using System;
using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using PagedList;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class NhanVienController : BaseController
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db =
            new QuanLyTapHoaThanhNhanEntities1();

        // ===================== INDEX (DANH SÁCH + FILTER) =====================
        public ActionResult Index(
            string keyword,
            string chucvu,
            string gioitinh,
            int? minLuong,
            int? maxLuong,
            int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;

            var list = _db.NhanVien.AsQueryable();

            // --- SEARCH: theo tên hoặc mã NV ---
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                list = list.Where(x =>
                    x.TenNV.Contains(keyword) ||
                    x.MaNV.ToString().Contains(keyword));
            }

            // --- FILTER CHỨC VỤ ---
            if (!string.IsNullOrWhiteSpace(chucvu))
                list = list.Where(x => x.ChucVu == chucvu);

            // --- FILTER GIỚI TÍNH ---
            if (!string.IsNullOrWhiteSpace(gioitinh))
                list = list.Where(x => x.GioiTinh == gioitinh);

            // --- FILTER LƯƠNG ---
            if (minLuong.HasValue)
                list = list.Where(x => x.MucLuong >= minLuong.Value);

            if (maxLuong.HasValue)
                list = list.Where(x => x.MucLuong <= maxLuong.Value);

            list = list.OrderBy(x => x.MaNV);

            // --- ViewBag giữ giá trị filter ---
            ViewBag.Keyword = keyword;
            ViewBag.ChucVu = chucvu;
            ViewBag.GioiTinh = gioitinh;
            ViewBag.MinLuong = minLuong;
            ViewBag.MaxLuong = maxLuong;

            // --- Danh sách chức vụ để load dropdown ---
            ViewBag.DSChucVu = _db.NhanVien
                .Where(x => x.ChucVu != null && x.ChucVu.Trim() != "")
                .Select(x => x.ChucVu)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            // --- Danh sách giới tính ---
            ViewBag.DSGioiTinh = new[] { "Nam", "Nữ", "Khác" }.ToList();

            return View(list.ToPagedList(pageNumber, pageSize));
        }

        // ===================== THÊM =====================
        [HttpGet]
        public ActionResult Them()
        {
            return PartialView("_ThemNhanVien");
        }

        [HttpPost]
        public JsonResult Them(NhanVien model)
        {
            try
            {
                if (model == null)
                    return Json(new { success = false, message = "Không nhận được dữ liệu." });

                if (string.IsNullOrWhiteSpace(model.TenNV))
                    return Json(new { success = false, message = "Tên nhân viên không được trống." });

                var nv = new NhanVien
                {
                    TenNV = model.TenNV,
                    DiaChi = model.DiaChi,
                    SoDT = model.SoDT,
                    Email = model.Email,
                    GioiTinh = model.GioiTinh,
                    NgaySinh = model.NgaySinh,
                    CCCD = model.CCCD,
                    ChucVu = model.ChucVu,
                    NgayVaoLam = model.NgayVaoLam ?? DateTime.Now,
                    MucLuong = model.MucLuong ?? 0,
                    GhiChu = model.GhiChu,
                    HinhAnh = string.IsNullOrEmpty(model.HinhAnh) ? "default.png" : model.HinhAnh
                };

                _db.NhanVien.Add(nv);
                _db.SaveChanges();

                return Json(new { success = true, message = "Thêm nhân viên thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ===================== SỬA =====================
        [HttpGet]
        public ActionResult Sua(int id)
        {
            var nv = _db.NhanVien.Find(id);
            if (nv == null) return HttpNotFound();

            // Populate role list like salary page
            var chucVuList = _db.NhanVien
                .Where(x => x.ChucVu != null && x.ChucVu.Trim() != "")
                .Select(x => x.ChucVu)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            ViewBag.DSChucVu = chucVuList;

            return PartialView("_SuaNhanVien", nv);
        }

        [HttpPost]
        public JsonResult Sua(NhanVien model)
        {
            try
            {
                if (model == null)
                    return Json(new { success = false, message = "Không nhận được dữ liệu." });

                var old = _db.NhanVien.Find(model.MaNV);
                if (old == null)
                    return Json(new { success = false, message = "Không tìm thấy nhân viên!" });

                old.TenNV = model.TenNV;
                old.DiaChi = model.DiaChi;
                old.SoDT = model.SoDT;
                old.Email = model.Email;
                old.GioiTinh = model.GioiTinh;
                old.NgaySinh = model.NgaySinh;
                old.CCCD = model.CCCD;
                old.ChucVu = model.ChucVu;
                old.NgayVaoLam = model.NgayVaoLam;
                old.MucLuong = model.MucLuong;
                old.GhiChu = model.GhiChu;
                old.HinhAnh = string.IsNullOrEmpty(model.HinhAnh) ? old.HinhAnh : model.HinhAnh;

                _db.SaveChanges();

                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ===================== XÓA =====================
        [HttpGet]
        public ActionResult Xoa(int id)
        {
            var nv = _db.NhanVien.Find(id);
            if (nv == null) return HttpNotFound();

            return PartialView("_XoaNhanVien", nv);
        }

        [HttpPost]
        public JsonResult XoaConfirmed(int id)
        {
            try
            {
                var nv = _db.NhanVien.Find(id);
                if (nv == null)
                    return Json(new { success = false, message = "Không tìm thấy nhân viên!" });

                _db.NhanVien.Remove(nv);
                _db.SaveChanges();

                return Json(new { success = true, message = "Đã xóa nhân viên!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
