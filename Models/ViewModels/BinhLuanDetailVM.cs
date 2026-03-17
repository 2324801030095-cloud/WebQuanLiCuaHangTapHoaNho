using System;
using System.Collections.Generic;

namespace WebQuanLiCuaHangTapHoa.Models.ViewModels
{
    public class BinhLuanDetailVM
    {
        // ===== THÔNG TIN BÌNH LUẬN =====
        public int MaDanhGia { get; set; }
        public string NoiDung { get; set; }
        public int Diem { get; set; }
        public DateTime NgayDanhGia { get; set; }
        public bool TrangThai { get; set; }

        // ===== THÔNG TIN KHÁCH HÀNG =====
        public int MaKH { get; set; }
        public string TenKH { get; set; }
        public string SoDT { get; set; }
        public string Email { get; set; }

        // ===== THÔNG TIN SẢN PHẨM (Nếu là đánh giá sản phẩm) =====
        public int? MaSP { get; set; }
        public string TenSP { get; set; }

        // ===== LỊCH SỬ HÓA ĐƠN =====
        public List<HoaDonTomTatVM> LichSuHoaDon { get; set; } = new List<HoaDonTomTatVM>();
        public int TongSoHoaDon { get; set; }
        public decimal TongTienHD { get; set; }
    }

    public class HoaDonTomTatVM
    {
        public int MaHD { get; set; }
        public DateTime NgayTao { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }
        public int SoLuongSP { get; set; }
    }
}