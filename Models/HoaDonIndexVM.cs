using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebQuanLiCuaHangTapHoa.Models
{
    public class HoaDonIndexVM
    {
        // Mã hóa đơn
        public int MaHD { get; set; }

        // Ngày lập
        public DateTime Ngay { get; set; }

        // Mã khách hàng
        public int? MaKH { get; set; }

        // Tên nhân viên lập
        public string TenNV { get; set; }

        // Tổng tiền hóa đơn
        public decimal TongTien { get; set; }
    }

}