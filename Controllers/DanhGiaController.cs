using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using WebQuanLiCuaHangTapHoa.Models.ViewModels;

namespace WebQuanLiCuaHangTapHoa.Controllers
{
    public class DanhGiaController : Controller
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        /* ===========================================================
           1) HIỂN THỊ DANH SÁCH ĐÁNH GIÁ (đã duyệt)
           =========================================================== */
        [ChildActionOnly]
        public ActionResult TestimonialList(int take = 10)
        {
            var list = _db.DanhGia
                .Where(d => d.TrangThai == true)     // chỉ lấy đánh giá đã duyệt
                .OrderByDescending(d => d.NgayDanhGia)
                .Take(take)
               .Select(d => new Testimonial
               {
                   MaDG = d.MaDanhGia,
                   MaKH = d.MaKH,
                   TenKH = d.KhachHang.TenKH,
                   MaSP = d.MaSP,
                   Diem = (byte)d.Diem,   // FIX LỖI
                   NoiDung = d.NoiDung,
                   NgayDanhGia = d.NgayDanhGia,
                   TrangThai = d.TrangThai
               })
                .ToList();


            return PartialView("~/Views/Shared/_TestimonialsPartial.cshtml", list);
        }


        /* ===========================================================
           2) FORM GỬI ĐÁNH GIÁ
           =========================================================== */
        [ChildActionOnly]
        public ActionResult TestimonialForm(int? maSP = null)
        {
            ViewBag.CanReview = false;

            // kiểm tra đăng nhập
            var sessionKh = Session["KH"];
            if (sessionKh == null)
            {
                return PartialView("~/Views/Shared/_TestimonialFormPartial.cshtml",
                    new TestimonialCreateModel { MaSP = maSP });
            }

            // lấy mã khách hàng từ Session
            int maKH = 0;
            try
            {
                dynamic kh = sessionKh;
                maKH = (int)kh.MaKH;
            }
            catch
            {
                int.TryParse(Session["KH"].ToString(), out maKH);
            }

            // kiểm tra quyền đánh giá
            bool canReview = false;

            if (maSP == null)
            {
                // khách phải có ít nhất 1 hóa đơn
                canReview = _db.HoaDon.Any(h => h.MaKH == maKH);
            }
            else
            {
                // phải mua sản phẩm này
                canReview =
                    (from hd in _db.HoaDon
                     join ct in _db.ChiTietHoaDon on hd.MaHD equals ct.MaHD
                     where hd.MaKH == maKH && ct.MaSP == maSP
                     select ct).Any();
            }

            ViewBag.CanReview = canReview;

            return PartialView("~/Views/Shared/_TestimonialFormPartial.cshtml",
                new TestimonialCreateModel { MaSP = maSP });
        }


        /* ===========================================================
           3) POST: GỬI ĐÁNH GIÁ
           =========================================================== */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(TestimonialCreateModel model)
        {
            var sessionKh = Session["KH"];
            if (sessionKh == null)
            {
                return Json(new { ok = false, message = "Vui lòng đăng nhập trước khi đánh giá." });
            }

            int maKH = 0;
            try
            {
                dynamic kh = sessionKh;
                maKH = (int)kh.MaKH;
            }
            catch
            {
                int.TryParse(Session["KH"].ToString(), out maKH);
            }

            // ✅ Parse điểm số (hỗ trợ 0.5 star, chấp nhận cả "." và ",")
            var diemRaw = Request["Diem"];
            decimal diem;
            if (!TryParseDiem(diemRaw, out diem))
                return Json(new { ok = false, message = "Điểm phải nằm trong khoảng 0.5 - 5." });

            // ✅ Validate điểm số (hỗ trợ 0.5 star)
            if (diem < 0.5m || diem > 5m)
                return Json(new { ok = false, message = "Điểm phải nằm trong khoảng 0.5 - 5." });

            // ✅ Kiểm tra quyền đánh giá: khách hàng phải đã mua sản phẩm này
            bool canReview = false;

            if (model.MaSP == null || model.MaSP == 0)
            {
                // Đánh giá cửa hàng: phải có ít nhất 1 hóa đơn
                canReview = _db.HoaDon.Any(h => h.MaKH == maKH);
            }
            else
            {
                // ✅ Đánh giá sản phẩm: phải có trong ChiTietHoaDon
                canReview = _db.ChiTietHoaDon
                    .Any(ct => ct.HoaDon.MaKH == maKH && ct.MaSP == model.MaSP);
            }

            if (!canReview)
            {
                return Json(new
                {
                    ok = false,
                    message = "Bạn không thể đánh giá vì chưa mua sản phẩm này (hoặc chưa từng mua hàng)."
                });
            }

            // ✅ Tạo record đánh giá mới
            var dg = new DanhGia
            {
                MaKH = maKH,
                MaSP = model.MaSP > 0 ? (int?)model.MaSP : null,  // Null nếu đánh giá cửa hàng
                Diem = (byte)Math.Round((double)diem, MidpointRounding.AwayFromZero),
                NoiDung = model.NoiDung ?? "",
                NgayDanhGia = DateTime.Now,
                TrangThai = false  // Chờ admin duyệt
            };

            _db.DanhGia.Add(dg);
            _db.SaveChanges();

            return Json(new { ok = true, message = "Đánh giá đã được gửi và chờ xét duyệt." });
        }

        // ===========================================================
        // 4) TRANG DANH SÁCH TẤT CẢ ĐÁNH GIÁ (đã duyệt)
        // ===========================================================
        public ActionResult DanhSach()
        {
            var list = _db.DanhGia
                .Where(d => d.TrangThai == true)
                .OrderByDescending(d => d.NgayDanhGia)
                .Select(d => new Testimonial
                {
                    MaDG = d.MaDanhGia,
                    MaKH = d.MaKH,
                    TenKH = d.KhachHang.TenKH,
                    MaSP = d.MaSP,
                    Diem = (byte)d.Diem,
                    NoiDung = d.NoiDung,
                    NgayDanhGia = d.NgayDanhGia,
                    TrangThai = d.TrangThai
                })
                .ToList();

            return View("~/Views/DanhGia/DanhSach.cshtml", list);
        }

        // ✅ TH ÊM ACTION CHI TIẾT ĐÁNH GIÁ (trả về PartialView)
        [HttpGet]
        public ActionResult ChiTiet(int id)
        {
            var dg = _db.DanhGia
                .Include("KhachHang")
                .Include("SanPham")
                .FirstOrDefault(d => d.MaDanhGia == id && d.TrangThai == true);

            if (dg == null) return HttpNotFound();

            // ✅ LOGIC: Chỉ lấy 1 hóa đơn chứa sản phẩm được đánh giá
            HoaDon hoaDonFound = null;

            if (dg.MaSP.HasValue)
            {
                // ✅ Đánh giá SẢN PHẨM: lấy hóa đơn mới nhất chứa sản phẩm này
                hoaDonFound = _db.HoaDon
                    .Include("ChiTietHoaDon.SanPham")
                    .Include("KhachHang")
                    .Where(h => h.MaKH == dg.MaKH &&
                                h.ChiTietHoaDon.Any(ct => ct.MaSP == dg.MaSP))
                    .OrderByDescending(h => h.Ngay)
                    .FirstOrDefault();
            }
            else
            {
                // ✅ Đánh giá CỬA HÀNG: lấy hóa đơn mới nhất
                hoaDonFound = _db.HoaDon
                    .Include("ChiTietHoaDon.SanPham")
                    .Include("KhachHang")
                    .Where(h => h.MaKH == dg.MaKH)
                    .OrderByDescending(h => h.Ngay)
                    .FirstOrDefault();
            }

            // ✅ XÂY DỰNG CHI TIẾT HÓA ĐƠN
            HoaDonDetail hoaDonDetail = null;
            
            if (hoaDonFound != null)
            {
                hoaDonDetail = new HoaDonDetail
                {
                    MaHD = hoaDonFound.MaHD,
                    NgayMua = hoaDonFound.Ngay,
                    TenKH = hoaDonFound.KhachHang?.TenKH ?? "N/A",
                    Items = hoaDonFound.ChiTietHoaDon.Select(ct => new ChiTietDonHang
                    {
                        MaSP = ct.MaSP,
                        TenSP = ct.SanPham?.TenSP ?? "N/A",
                        HinhAnh = ct.SanPham?.HinhAnh ?? "",
                        SoLuong = ct.SoLuong,
                        DonGia = (decimal)ct.DonGia,
                        LaSanPhamDanhGia = dg.MaSP.HasValue && ct.MaSP == dg.MaSP
                    }).ToList(),
                    TongTien = hoaDonFound.ChiTietHoaDon.Sum(ct => (decimal)ct.SoLuong * ct.DonGia)
                };
            }

            // Tạo ViewModel kết hợp
            var viewModel = new DanhGiaChiTietViewModel
            {
                DanhGia = dg,
                HoaDons = hoaDonFound != null ? new List<HoaDon> { hoaDonFound } : new List<HoaDon>(),
                HoaDonDetails = hoaDonDetail != null ? new List<HoaDonDetail> { hoaDonDetail } : new List<HoaDonDetail>(),
                TongSoHoaDon = hoaDonDetail != null ? 1 : 0,
                TongTienHD = hoaDonDetail?.TongTien ?? 0m
            };

            return PartialView("_ChiTietDanhGia", viewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }

        private static bool TryParseDiem(string raw, out decimal diem)
        {
            diem = 0m;
            if (string.IsNullOrWhiteSpace(raw)) return false;

            // Normalize to invariant with dot to avoid vi-VN parsing "2.5" as 25
            var match = System.Text.RegularExpressions.Regex.Match(raw, @"\d+([.,]\d+)?");
            var normalized = match.Success ? match.Value : raw;
            normalized = normalized.Replace(',', '.');

            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out diem))
            {
                // If user selected x.5 but culture dropped dot, recover (e.g. "2.5" -> "25")
                if (diem > 5m && (raw.Contains(".") || raw.Contains(",")))
                {
                    var digits = System.Text.RegularExpressions.Regex.Replace(raw, @"[^\d]", "");
                    if (digits.Length >= 2 && decimal.TryParse(digits, NumberStyles.Number, CultureInfo.InvariantCulture, out var asInt))
                    {
                        var asHalf = asInt / 10m;
                        if (asHalf <= 5m)
                        {
                            diem = asHalf;
                            return true;
                        }
                    }
                }
                return true;
            }

            if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.CurrentCulture, out diem))
                return true;

            if (decimal.TryParse(raw, NumberStyles.Number, new CultureInfo("vi-VN"), out diem))
                return true;

            return false;
        }
    }
}
