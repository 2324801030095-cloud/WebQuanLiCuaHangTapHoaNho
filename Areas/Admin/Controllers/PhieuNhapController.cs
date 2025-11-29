using System;
using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using PagedList;
using System.Data.Entity;
using Newtonsoft.Json;
using WebQuanLiCuaHangTapHoa.Areas.Admin.Models.ViewModels;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class PhieuNhapController : Controller
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // ============================================================
        // INDEX
        // ============================================================
        public ActionResult Index(int? page, string search, string from, string to)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;

            var q = _db.PhieuNhap
                .Include(p => p.NhaCungCap)
                .Include(p => p.NhanVien)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string kw = search.Trim();
                if (int.TryParse(kw, out int id))
                    q = q.Where(p => p.MaPN == id || p.MaNCC == id);
                else
                    q = q.Where(p => p.NhaCungCap.TenNCC.Contains(kw) ||
                                     p.NhanVien.TenNV.Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(from) && DateTime.TryParse(from, out DateTime tu))
                q = q.Where(p => DbFunctions.TruncateTime(p.Ngay) >= tu.Date);

            if (!string.IsNullOrWhiteSpace(to) && DateTime.TryParse(to, out DateTime den))
                q = q.Where(p => p.Ngay <= den.AddDays(1));

            return View(q.OrderBy(p => p.Ngay).ToPagedList(pageNumber, pageSize));
        }

        // ============================================================
        // CHI TIẾT
        // ============================================================
        [HttpGet]
        public ActionResult ChiTiet(int id)
        {
            var pn = _db.PhieuNhap
                .Include(p => p.NhaCungCap)
                .Include(p => p.NhanVien)
                .FirstOrDefault(p => p.MaPN == id);

            if (pn == null) return HttpNotFound();

            var chiTiet = _db.ChiTietPhieuNhap
                .Where(ct => ct.MaPN == id)
                .Select(ct => new ChiTietPN_VM
                {
                    MaCTPN = ct.MaCTPN,
                    MaSP = ct.MaSP,
                    TenSP = ct.SanPham.TenSP,
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGiaNhap,
                    ThanhTien = ct.SoLuong * ct.DonGiaNhap
                }).ToList();

            ViewBag.Phieu = pn;
            ViewBag.TongTien = chiTiet.Sum(x => x.ThanhTien);

            return PartialView("_ChiTietPhieuNhap", chiTiet);
        }

        // ============================================================
        // GET THÊM (KHÔNG LỖI)
        // ============================================================
        [HttpGet]
        public ActionResult Them()
        {
            ViewBag.MaNCC = new SelectList(_db.NhaCungCap, "MaNCC", "TenNCC");
            ViewBag.MaNV = new SelectList(_db.NhanVien, "MaNV", "TenNV");
            ViewBag.SanPhamList = new SelectList(_db.SanPham, "MaSP", "TenSP");

            return PartialView("_ThemPhieuNhap");
        }

        // ============================================================
        // POST THÊM – TÊN KHÁC → KHÔNG TRÙNG
        // ============================================================
        [HttpPost]
        public JsonResult Them_Post()
        {
            try
            {
                Request.InputStream.Position = 0;
                string json = new System.IO.StreamReader(Request.InputStream).ReadToEnd();

                var data = JsonConvert.DeserializeObject<PhieuNhapCreateVM>(json);
                if (data == null)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });

                var header = data.Phieu;
                header.Ngay = header.Ngay == default ? DateTime.Now : header.Ngay;

                _db.PhieuNhap.Add(header);
                _db.SaveChanges();

                foreach (var i in data.ChiTiet)
                {
                    var ct = new ChiTietPhieuNhap
                    {
                        MaPN = header.MaPN,
                        MaSP = i.MaSP,
                        SoLuong = i.SoLuong,
                        DonGiaNhap = i.DonGia
                    };

                    _db.ChiTietPhieuNhap.Add(ct);

                    var kho = _db.Kho.Find(i.MaSP);
                    if (kho == null)
                        _db.Kho.Add(new Kho { MaSP = i.MaSP, Ton = i.SoLuong });
                    else
                        kho.Ton += i.SoLuong;
                }

                _db.SaveChanges();
                return Json(new { success = true, message = "Đã thêm phiếu nhập!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ============================================================
        // GET SỬA
        // ============================================================
        [HttpGet]
        public ActionResult Sua(int id)
        {
            var pn = _db.PhieuNhap.Find(id);
            if (pn == null) return HttpNotFound();

            ViewBag.MaNCC = new SelectList(_db.NhaCungCap, "MaNCC", "TenNCC", pn.MaNCC);
            ViewBag.MaNV = new SelectList(_db.NhanVien, "MaNV", "TenNV", pn.MaNV);

            return PartialView("_SuaPhieuNhap", pn);
        }

        // ============================================================
        // POST SỬA – KHÔNG TRÙNG TÊN
        // ============================================================
        [HttpPost]
        public JsonResult Sua_Post()
        {
            try
            {
                Request.InputStream.Position = 0;
                string json = new System.IO.StreamReader(Request.InputStream).ReadToEnd();

                var model = JsonConvert.DeserializeObject<PhieuNhap>(json);
                if (model == null)
                    return Json(new { success = false, message = "Không nhận dữ liệu!" });

                var old = _db.PhieuNhap.Find(model.MaPN);
                if (old == null)
                    return Json(new { success = false, message = "Không tìm thấy phiếu nhập!" });

                old.Ngay = model.Ngay;
                old.MaNCC = model.MaNCC;
                old.MaNV = model.MaNV;
                old.GhiChu = model.GhiChu;

                _db.SaveChanges();
                return Json(new { success = true, message = "Đã cập nhật phiếu nhập!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ============================================================
        // XÓA
        // ============================================================
        [HttpPost]
        public JsonResult Xoa(int id)
        {
            try
            {
                var pn = _db.PhieuNhap.Find(id);
                if (pn == null)
                    return Json(new { success = false, message = "Không tìm thấy phiếu nhập!" });

                var chiTiet = _db.ChiTietPhieuNhap.Where(ct => ct.MaPN == id).ToList();

                foreach (var ct in chiTiet)
                {
                    var kho = _db.Kho.Find(ct.MaSP);
                    if (kho != null)
                        kho.Ton = Math.Max(0, kho.Ton - ct.SoLuong);
                }

                _db.PhieuNhap.Remove(pn);
                _db.SaveChanges();

                return Json(new { success = true, message = "Đã xóa phiếu nhập!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
