using System;
using System.Collections.Generic;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Models.ViewModels
{
    public class ChiTietCreateVM
    {
        public int MaSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }

    public class HoaDonCreateVM
    {
        public DateTime Ngay { get; set; }
        public int MaKH { get; set; }
        public int MaNV { get; set; }

        public List<ChiTietCreateVM> SanPhams { get; set; } = new List<ChiTietCreateVM>();
    }
}
