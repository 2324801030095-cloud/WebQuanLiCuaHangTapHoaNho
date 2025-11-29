// File: Areas/Admin/Models/ViewModels/KhachHangVM.cs
using System;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Models.ViewModels
{
    // ViewModel cho Khách hàng: chứa thông tin hiển thị trên bảng (bao gồm tổng đơn/tổng tiền)
    public class KhachHangVM
    {
        public int MaKH { get; set; }                   // Mã khách hàng
        public string TenKH { get; set; }               // Tên khách hàng
        public string SoDT { get; set; }                // Số điện thoại
        public string DiaChi { get; set; }              // Địa chỉ

        public int TongDon { get; set; }                // Tổng số đơn (số hoá đơn có MaKH)
        public decimal TongTien { get; set; }          // Tổng tiền đã chi (tổng DonGia * SoLuong của tất cả hoá đơn)
    }
}
