using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Models.ViewModels
{
    public class TopBanChayVM
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public decimal GiaBan { get; set; }
        public string TenDanhMuc { get; set; }
        public bool HoatDong { get; set; }
        public string HinhAnh { get; set; }
        public int SoLuongBan { get; set; }
        public decimal TongDoanhThu { get; set; }
    }
}
