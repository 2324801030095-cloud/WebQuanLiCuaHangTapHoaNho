using System;
using System.Collections.Generic;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Models.ViewModels
{
    public class HoaDonChiTietVM
    {
        public int MaHD { get; set; }
        public DateTime Ngay { get; set; }

        public int MaNV { get; set; }
        public string TenNV { get; set; }

        public int? MaKH { get; set; }
        public string TenKH { get; set; }

        public List<ChiTietItemVM> ChiTiet { get; set; }

        public decimal TongTien { get; set; }
    }
}
