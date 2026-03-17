using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.Collections.Generic;
using WebQuanLiCuaHangTapHoa.Models;
using WebQuanLiCuaHangTapHoa.Models.ViewModels;
using PagedList;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class BinhLuanController : BaseController
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // ===========================================================
        // INDEX - Danh sách bình luận/đánh giá
        // ===========================================================
        public ActionResult Index(int? page, string search, bool? trangThai)
        {
            int pageNumber = page ?? 1;
            int pageSize = 15;

            var query = _db.DanhGia
                .Include("KhachHang")
                .Include("SanPham")
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => 
                    d.KhachHang.TenKH.Contains(search) || 
                    (d.SanPham != null && d.SanPham.TenSP.Contains(search)) ||
                    d.NoiDung.Contains(search)
                );
            }

            // Lọc theo trạng thái
            if (trangThai.HasValue)
            {
                query = query.Where(d => d.TrangThai == trangThai.Value);
            }

            var list = query
                .OrderByDescending(d => d.NgayDanhGia)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.Search = search;
            ViewBag.TrangThai = trangThai;

            return View(list);
        }

        // ===========================================================
        // CHI TIẾT BÌNH LUẬN + LỊCH SỬ HÓA ĐƠN
        // ===========================================================
        [HttpGet]
        public ActionResult ChiTiet(int id)
        {
            var dg = _db.DanhGia
                .Include("KhachHang")
                .Include("SanPham")
                .FirstOrDefault(d => d.MaDanhGia == id);

            if (dg == null) return HttpNotFound();

            HoaDon hoaDonFound = null;

            if (dg.MaSP.HasValue)
            {
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
                hoaDonFound = _db.HoaDon
                    .Include("ChiTietHoaDon.SanPham")
                    .Include("KhachHang")
                    .Where(h => h.MaKH == dg.MaKH)
                    .OrderByDescending(h => h.Ngay)
                    .FirstOrDefault();
            }

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

            var viewModel = new DanhGiaChiTietViewModel
            {
                DanhGia = dg,
                HoaDons = hoaDonFound != null ? new List<HoaDon> { hoaDonFound } : new List<HoaDon>(),
                HoaDonDetails = hoaDonDetail != null ? new List<HoaDonDetail> { hoaDonDetail } : new List<HoaDonDetail>(),
                TongSoHoaDon = hoaDonDetail != null ? 1 : 0,
                TongTienHD = hoaDonDetail?.TongTien ?? 0m
            };

            return PartialView("_ChiTietBinhLuan", viewModel);
        }

        // ===========================================================
        // DUYỆT BÌNH LUẬN
        // ===========================================================
        [HttpPost]
        public JsonResult Duyet(int id)
        {
            try
            {
                var dg = _db.DanhGia.Find(id);
                if (dg == null)
                    return Json(new { success = false, message = "Không tìm thấy bình luận." });

                dg.TrangThai = true;
                _db.SaveChanges();

                return Json(new { success = true, message = "Đã duyệt bình luận." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ===========================================================
        // XÓA BÌNH LUẬN
        // ===========================================================
        [HttpPost]
        public JsonResult Xoa(int id)
        {
            try
            {
                var dg = _db.DanhGia.Find(id);
                if (dg == null)
                    return Json(new { success = false, message = "Không tìm thấy bình luận." });

                _db.DanhGia.Remove(dg);
                _db.SaveChanges();

                return Json(new { success = true, message = "Đã xóa bình luận." });
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

