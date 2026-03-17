using System;
using System.Linq;
using System.IO;
using System.Web.Mvc;
using System.Data.Entity;
using WebQuanLiCuaHangTapHoa.Models;
using WebQuanLiCuaHangTapHoa.Areas.Admin.Models.ViewModels;
using PagedList;
using System.Web;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class SanPhamController : BaseController
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // =====================================================
        // 📄 DANH SÁCH SẢN PHẨM + TÌM KIẾM + LỌC
        // =====================================================
        public ActionResult Index(string search, int? danhMuc, string gia, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 8;

            // Lấy sản phẩm + bảng liên quan
            var sanPhams = _db.SanPham
                .Include(s => s.DanhMuc)
                .Include(s => s.DonViTinh)
                .Include(s => s.KhuyenMai)
                .AsQueryable();

            // 🔍 Tìm kiếm
            if (!string.IsNullOrEmpty(search))
                sanPhams = sanPhams.Where(sp => sp.TenSP.Contains(search));

            // 🧩 Lọc theo danh mục
            if (danhMuc.HasValue && danhMuc.Value > 0)
                sanPhams = sanPhams.Where(sp => sp.MaDM == danhMuc.Value);

            // 💰 Lọc theo giá
            switch (gia)
            {
                case "duoi50":
                    sanPhams = sanPhams.Where(sp => sp.GiaBan < 50000);
                    break;
                case "50to100":
                    sanPhams = sanPhams.Where(sp => sp.GiaBan >= 50000 && sp.GiaBan <= 100000);
                    break;
                case "tren100":
                    sanPhams = sanPhams.Where(sp => sp.GiaBan > 100000);
                    break;
            }

            // Gửi danh mục cho dropdown
            ViewBag.MaDM = new SelectList(_db.DanhMuc.ToList(), "MaDM", "TenDM");

            // 🔄 Map sang ViewModel phẳng
            var list = sanPhams
                .OrderBy(sp => sp.MaSP) // ✅ sắp tăng dần theo mã sản phẩm
                .ToList()
                .Select(sp => new ProductVM
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    GiaBan = sp.GiaBan,
                    TenDanhMuc = sp.DanhMuc != null ? sp.DanhMuc.TenDM : "",
                    TenDonViTinh = sp.DonViTinh != null ? sp.DonViTinh.TenDVT : "",
                    TenKhuyenMai = sp.KhuyenMai != null ? sp.KhuyenMai.TenKM : "",
                    HoatDong = sp.HoatDong,
                    HinhAnh = sp.HinhAnh,
                    MoTaNgan = sp.MoTaNgan
                })
                .ToPagedList(pageNumber, pageSize);

            return View(list);
        }

        // =====================================================
        // ➕ THÊM (CÓ THỂ NHẬN DANH MỤC SẴN)
        // =====================================================
        [HttpGet]
        public ActionResult Them(int? maDM)
        {
            // Nếu có tham số maDM, chọn sẵn danh mục đó trong dropdown
            ViewBag.MaDM = new SelectList(_db.DanhMuc, "MaDM", "TenDM", maDM);
            ViewBag.MaDVT = new SelectList(_db.DonViTinh, "MaDVT", "TenDVT");
            ViewBag.MaKM = new SelectList(_db.KhuyenMai, "MaKM", "TenKM");

            // Truyền MaDM xuống view để khi POST vẫn giữ đúng danh mục
            ViewBag.SelectedDM = maDM;

            return PartialView("_ThemSanPham");
        }

        private bool TrySaveImage(HttpPostedFileBase imageFile, string imageName, out string fileName, out string error)
        {
            fileName = null;
            error = null;

            if (imageFile == null || imageFile.ContentLength == 0) return true;

            var originalName = Path.GetFileName(imageFile.FileName);
            var ext = Path.GetExtension(originalName);
            var baseName = string.IsNullOrWhiteSpace(imageName)
                ? Path.GetFileNameWithoutExtension(originalName)
                : Path.GetFileNameWithoutExtension(imageName);

            var invalidChars = Path.GetInvalidFileNameChars();
            baseName = new string(baseName.Where(c => !invalidChars.Contains(c)).ToArray()).Trim();
            if (string.IsNullOrWhiteSpace(baseName))
            {
                baseName = "img_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            }

            fileName = baseName + ext;
            var imagesDir = Server.MapPath("~/Content/Images");
            if (!Directory.Exists(imagesDir)) Directory.CreateDirectory(imagesDir);
            var filePath = Path.Combine(imagesDir, fileName);

            imageFile.SaveAs(filePath);
            return true;
        }

        [HttpPost]
        public JsonResult Them(SanPham sp, HttpPostedFileBase imageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "⚠️ Dữ liệu không hợp lệ!" });

                string fileName;
                string error;
                if (!TrySaveImage(imageFile, sp.HinhAnh, out fileName, out error))
                    return Json(new { success = false, message = error ?? "❌ Không thể lưu ảnh!" });

                if (!string.IsNullOrWhiteSpace(fileName))
                    sp.HinhAnh = fileName;

                _db.SanPham.Add(sp);
                _db.SaveChanges();
                return Json(new { success = true, message = "✅ Đã thêm sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "❌ Lỗi: " + ex.Message });
            }
        }


        // =====================================================
        // ✏️ SỬA
        // =====================================================
        [HttpGet]
        public ActionResult Sua(int id)
        {
            var sp = _db.SanPham.Find(id);
            if (sp == null) return HttpNotFound();

            ViewBag.MaDM = new SelectList(_db.DanhMuc, "MaDM", "TenDM", sp.MaDM);
            ViewBag.MaDVT = new SelectList(_db.DonViTinh, "MaDVT", "TenDVT", sp.MaDVT);
            ViewBag.MaKM = new SelectList(_db.KhuyenMai, "MaKM", "TenKM", sp.MaKM);

            return PartialView("_SuaSanPham", sp);
        }

        [HttpPost]
        public JsonResult Sua(SanPham sp)
        {
            try
            {
                var old = _db.SanPham.Find(sp.MaSP);
                if (old == null)
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm để cập nhật!" });

                _db.Entry(old).CurrentValues.SetValues(sp);
                _db.SaveChanges();

                return Json(new { success = true, message = "✅ Cập nhật sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "❌ Lỗi: " + ex.Message });
            }
        }

        // =====================================================
        // ❌ XÓA
        // =====================================================
        [HttpPost]
        public JsonResult Xoa(int id)
        {
            try
            {
                var sp = _db.SanPham.Find(id);
                if (sp == null)
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm để xóa!" });

                _db.SanPham.Remove(sp);
                _db.SaveChanges();

                return Json(new { success = true, message = "🗑️ Đã xóa sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "❌ Lỗi: " + ex.Message });
            }
        }

        // =====================================================
        // 👁️ XEM CHI TIẾT
        // =====================================================
        [HttpGet]
        public ActionResult ChiTiet(int id)
        {
            try
            {
                var sp = _db.SanPham
                    .Include(s => s.DanhMuc)
                    .Include(s => s.DonViTinh)
                    .Include(s => s.KhuyenMai)
                    .FirstOrDefault(s => s.MaSP == id);

                if (sp == null)
                    return HttpNotFound();

                // ✅ Map sang ProductVM (ViewModel phẳng)
                var vm = new WebQuanLiCuaHangTapHoa.Areas.Admin.Models.ViewModels.ProductVM
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    GiaBan = sp.GiaBan,
                    TenDanhMuc = sp.DanhMuc != null ? sp.DanhMuc.TenDM : "Không có",
                    TenDonViTinh = sp.DonViTinh != null ? sp.DonViTinh.TenDVT : "Không có",
                    TenKhuyenMai = sp.KhuyenMai != null ? sp.KhuyenMai.TenKM : "Không có",
                    HoatDong = sp.HoatDong,
                    HinhAnh = sp.HinhAnh,
                    MoTaNgan = sp.MoTaNgan
                };

                // Use explicit view path to avoid view resolution issues when called via AJAX
                return PartialView("~/Areas/Admin/Views/SanPham/_ChiTietSanPham.cshtml", vm);
            }
            catch (Exception ex)
            {
                // Return a simple error modal so client doesn't receive a 500 page
                return PartialView("~/Areas/Admin/Views/Shared/_ErrorModal.cshtml", ex.Message);
            }
        }


    }
}
