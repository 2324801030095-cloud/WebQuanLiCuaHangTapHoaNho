using System;

namespace WebQuanLiCuaHangTapHoa.Models.ViewModels
{
    /// <summary>
    /// ViewModel for displaying products in order review after payment
    /// </summary>
    public class OrderProductVM
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string HinhAnh { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
    }
}
