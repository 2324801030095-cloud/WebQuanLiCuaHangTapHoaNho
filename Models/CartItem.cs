using System;
using System.Collections.Generic;

namespace WebQuanLiCuaHangTapHoa.Models
{
    // Đại diện cho 1 sản phẩm trong giỏ hàng
    public class CartItem
    {
        public int MaSP { get; set; }           // Mã sản phẩm
        public string TenSP { get; set; }       // Tên sản phẩm
        public int DonGia { get; set; }         // Giá bán hiện tại
        public int SoLuong { get; set; }        // Số lượng mua
        public string HinhAnh { get; set; }     // Ảnh sản phẩm

        // Thuộc tính tính tổng tiền
        public int ThanhTien => SoLuong * DonGia;
    }
}
