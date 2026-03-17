using System;
using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using PagedList;
using System.Collections.Generic;
using System.Data.Entity;
using WebQuanLiCuaHangTapHoa.Areas.Admin.Models.ViewModels;
using Newtonsoft.Json;
using System.IO;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class HoaDonController : BaseController
    {
        // CHỈ KHAI BÁO 1 LẦN DUY NHẤT
        private QuanLyTapHoaThanhNhanEntities1 db = new QuanLyTapHoaThanhNhanEntities1();

        // =====================================================
        // 1) TRANG DANH SÁCH HÓA ĐƠN (INDEX)
        // =====================================================
        public ActionResult Index(int? page, string search, string from, string to, string sort)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;

            var q = db.HoaDon
                       .Include(h => h.NhanVien)
                       .Include(h => h.KhachHang)
                       .Include(h => h.ChiTietHoaDon)
                       .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                string s = search.Trim();
                if (int.TryParse(s, out int id))
                {
                    q = q.Where(h => h.MaHD == id || h.MaKH == id);
                }
                else
                {
                    q = q.Where(h =>
                        (h.KhachHang != null && h.KhachHang.TenKH.Contains(s)) ||
                        (h.NhanVien != null && h.NhanVien.TenNV.Contains(s))
                    );
                }
            }

            // Lọc ngày
            if (DateTime.TryParse(from, out DateTime tuNgay))
            {
                q = q.Where(h => h.Ngay >= tuNgay.Date);
            }
            if (DateTime.TryParse(to, out DateTime denNgay))
            {
                q = q.Where(h => h.Ngay <= denNgay.Date.AddDays(1).AddTicks(-1));
            }

            // Sắp xếp
            switch (sort)
            {
                case "ngay_asc": q = q.OrderBy(h => h.Ngay); break;
                case "ngay_desc": q = q.OrderByDescending(h => h.Ngay); break;
                case "mahd_desc": q = q.OrderByDescending(h => h.MaHD); break;
                default: q = q.OrderBy(h => h.MaHD); break;
            }

            var list = q.ToList();
            return View(list.ToPagedList(pageNumber, pageSize));
        }

        // =====================================================
        // 2) GET FORM THÊM HÓA ĐƠN
        // =====================================================
        [HttpGet]
        public ActionResult Them()
        {
            ViewBag.KhachHang = new SelectList(db.KhachHang.OrderBy(k => k.TenKH), "MaKH", "TenKH");
            ViewBag.NhanVien = new SelectList(db.NhanVien.OrderBy(n => n.TenNV), "MaNV", "TenNV");
            ViewBag.SanPham = db.SanPham.OrderBy(sp => sp.TenSP).ToList();

            return PartialView("_ThemHoaDon");
        }

        // =====================================================
        // 3) POST THÊM HÓA ĐƠN + CHI TIẾT (JSON)
        // =====================================================
        [HttpPost]
        public JsonResult ThemHoaDon()
        {
            try
            {
                string contentType = Request.ContentType ?? "";

                // Nếu client gửi JSON
                if (contentType.Contains("application/json"))
                {
                    Request.InputStream.Position = 0;
                    string json;
                    using (var reader = new StreamReader(Request.InputStream))
                        json = reader.ReadToEnd();

                    var vm = JsonConvert.DeserializeObject<HoaDonCreateVM>(json);

                    // VALIDATE
                    if (vm == null)
                        return Json(new { success = false, message = "Dữ liệu JSON không hợp lệ." }, JsonRequestBehavior.AllowGet);

                    if (vm.MaKH == 0)
                        return Json(new { success = false, message = "Chưa chọn khách hàng." }, JsonRequestBehavior.AllowGet);

                    if (vm.MaNV == 0)
                        return Json(new { success = false, message = "Chưa chọn nhân viên." }, JsonRequestBehavior.AllowGet);

                    if (vm.SanPhams == null || !vm.SanPhams.Any())
                        return Json(new { success = false, message = "Chưa có sản phẩm trong hóa đơn." }, JsonRequestBehavior.AllowGet);

                    // Kiểm tra số lượng hợp lệ
                    if (vm.SanPhams.Any(s => s.SoLuong <= 0))
                        return Json(new { success = false, message = "Số lượng sản phẩm phải lớn hơn 0." }, JsonRequestBehavior.AllowGet);

                    // Transaction đảm bảo an toàn
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            // Tạo hóa đơn
                            var hd = new HoaDon
                            {
                                Ngay = vm.Ngay == default(DateTime) ? DateTime.Now : vm.Ngay,
                                MaKH = vm.MaKH,
                                MaNV = vm.MaNV
                            };

                            db.HoaDon.Add(hd);
                            db.SaveChanges(); // tạo MaHD

                            // Thêm chi tiết
                            foreach (var item in vm.SanPhams)
                            {
                                // Validate sản phẩm tồn tại
                                var sp = db.SanPham.Find(item.MaSP);
                                if (sp == null)
                                {
                                    trans.Rollback();
                                    return Json(new { success = false, message = "Không tìm thấy sản phẩm MaSP=" + item.MaSP }, JsonRequestBehavior.AllowGet);
                                }

                                var ct = new ChiTietHoaDon
                                {
                                    MaHD = hd.MaHD,
                                    MaSP = item.MaSP,
                                    SoLuong = item.SoLuong,
                                    DonGia = (int)item.DonGia
                                };

                                db.ChiTietHoaDon.Add(ct);
                            }

                            db.SaveChanges();
                            trans.Commit();

                            return Json(new { success = true, message = "Tạo hóa đơn thành công.", maHD = hd.MaHD }, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            return Json(new { success = false, message = "Lỗi khi lưu hóa đơn: " + ex.Message }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                return Json(new { success = false, message = "Định dạng request không hợp lệ." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi không xác định: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // =====================================================
        // 4) GET FORM SỬA HÓA ĐƠN
        // =====================================================
        [HttpGet]
        public ActionResult Sua(int id)
        {
            var hd = db.HoaDon.Find(id);
            if (hd == null) return HttpNotFound();

            ViewBag.KhachHang = new SelectList(db.KhachHang.OrderBy(k => k.TenKH), "MaKH", "TenKH", hd.MaKH);
            ViewBag.NhanVien = new SelectList(db.NhanVien.OrderBy(n => n.TenNV), "MaNV", "TenNV", hd.MaNV);

            return PartialView("_SuaHoaDon", hd);
        }

        // =====================================================
        // 5) POST SỬA HÓA ĐƠN (FormData)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Sua(HoaDon model)
        {
            try
            {
                if (model.MaHD <= 0)
                    return Json(new { success = false, message = "Mã hóa đơn không hợp lệ." }, JsonRequestBehavior.AllowGet);

                var old = db.HoaDon.Find(model.MaHD);
                if (old == null)
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn." }, JsonRequestBehavior.AllowGet);

                // Validate
                if (model.MaKH == 0)
                    return Json(new { success = false, message = "Chưa chọn khách hàng." }, JsonRequestBehavior.AllowGet);

                if (model.MaNV == 0)
                    return Json(new { success = false, message = "Chưa chọn nhân viên." }, JsonRequestBehavior.AllowGet);

                // Cập nhật
                old.Ngay = model.Ngay == default(DateTime) ? old.Ngay : model.Ngay;
                old.MaKH = model.MaKH;
                old.MaNV = model.MaNV;

                db.SaveChanges();

                return Json(new { success = true, message = "Cập nhật hóa đơn thành công." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi cập nhật: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // =====================================================
        // 6) CHI TIẾT HÓA ĐƠN (MODAL)
        // =====================================================
        [HttpGet]
        public ActionResult ChiTiet(int id)
        {
            var hd = db.HoaDon
                        .Include(h => h.KhachHang)
                        .Include(h => h.NhanVien)
                        .FirstOrDefault(h => h.MaHD == id);

            if (hd == null) return HttpNotFound();

            var ct = db.ChiTietHoaDon
                        .Where(x => x.MaHD == id)
                        .Select(x => new ChiTietItemVM
                        {
                            MaSP = x.MaSP,
                            TenSP = x.SanPham.TenSP,
                            SoLuong = x.SoLuong,
                            DonGia = x.DonGia
                        })
                        .ToList();

            ViewBag.TongTien = ct.Sum(x => x.ThanhTien);
            ViewBag.HoaDon = hd;

            return PartialView("_ChiTietHoaDon", ct);
        }

        // =====================================================
        // 7) XÓA HÓA ĐƠN
        // =====================================================
        [HttpPost]
        public JsonResult Xoa(int id)
        {
            try
            {
                if (id <= 0)
                    return Json(new { success = false, message = "Mã hóa đơn không hợp lệ." }, JsonRequestBehavior.AllowGet);

                var hd = db.HoaDon.Find(id);
                if (hd == null)
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn." }, JsonRequestBehavior.AllowGet);

                // Xóa chi tiết trước
                var chiTiet = db.ChiTietHoaDon.Where(ct => ct.MaHD == id).ToList();
                foreach (var ct in chiTiet)
                {
                    db.ChiTietHoaDon.Remove(ct);
                }

                // Xóa hóa đơn
                db.HoaDon.Remove(hd);
                db.SaveChanges();

                return Json(new { success = true, message = "Đã xóa hóa đơn." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi xóa: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // =====================================================
        // 8) LỌC THEO NGÀY
        // =====================================================
        [HttpPost]
        public ActionResult LocTheoNgay(DateTime tuNgay, DateTime denNgay)
        {
            var list = db.HoaDon
                          .Where(h => h.Ngay >= tuNgay && h.Ngay <= denNgay)
                          .OrderByDescending(h => h.Ngay)
                          .ToList();

            return PartialView("_DanhSachHoaDon", list);
        }

        // =====================================================
        // 9) XUẤT PDF
        // =====================================================
        public ActionResult ExportPDF(int id)
        {
            var hd = db.HoaDon.Find(id);
            if (hd == null) return HttpNotFound();

            return new Rotativa.ActionAsPdf("ChiTietPDF", new { id = id })
            {
                FileName = "HoaDon_" + id + ".pdf"
            };
        }

        // =====================================================
        // 10) DISPOSE
        // =====================================================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}