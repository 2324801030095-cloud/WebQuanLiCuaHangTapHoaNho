using System;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Models.ViewModels
{
    public class ChiTietItemVM
    {
        // Mã sản phẩm
        public int MaSP { get; set; }

        // Tên sản phẩm
        public string TenSP { get; set; }

        // Số lượng mua
        public int SoLuong { get; set; }

        // Đơn giá sản phẩm tại thời điểm mua
        public decimal DonGia { get; set; }

        // Thành tiền = SL × Đơn giá
        public decimal ThanhTien => SoLuong * DonGia;
        /// <summary>
        /// Tính thành tiền = SoLuong * DonGia
        /// </summary>
     
    }
}
