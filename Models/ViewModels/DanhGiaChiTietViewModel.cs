using System;
using System.Collections.Generic;

namespace WebQuanLiCuaHangTapHoa.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho view chi tiết đánh giá
    /// </summary>
    public class DanhGiaChiTietViewModel
    {
        public DanhGia DanhGia { get; set; }
        public List<HoaDon> HoaDons { get; set; }
        public int TongSoHoaDon { get; set; }
        public decimal TongTienHD { get; set; }
        
        // ✅ THÊM: Danh sách chi tiết hóa đơn (lọc theo sản phẩm nếu có)
        public List<HoaDonDetail> HoaDonDetails { get; set; }

        public DanhGiaChiTietViewModel()
        {
            HoaDons = new List<HoaDon>();
            HoaDonDetails = new List<HoaDonDetail>();
            TongSoHoaDon = 0;
            TongTienHD = 0m;
        }
    }

    /// <summary>
    /// Chi tiết hóa đơn cho hiển thị
    /// </summary>
    public class HoaDonDetail
    {
        public int MaHD { get; set; }
        public DateTime NgayMua { get; set; }
        public string TenKH { get; set; }
        public List<ChiTietDonHang> Items { get; set; }
        public decimal TongTien { get; set; }
    }

    /// <summary>
    /// Chi tiết từng sản phẩm trong đơn hàng
    /// </summary>
    public class ChiTietDonHang
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string HinhAnh { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
        public bool LaSanPhamDanhGia { get; set; } // Đánh dấu sản phẩm được đánh giá
    }
}
