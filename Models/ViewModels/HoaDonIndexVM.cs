using System;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Models.ViewModels
{
    public class HoaDonIndexVM
    {
        public int MaHD { get; set; }
        public DateTime Ngay { get; set; }

        public int? MaKH { get; set; }

        // THÊM MỚI — Khách hàng
        public string TenKH { get; set; }

        // Nhân viên lập
        public string TenNV { get; set; }

        // Tổng tiền hóa đơn
        public decimal TongTien { get; set; }
    }
}
