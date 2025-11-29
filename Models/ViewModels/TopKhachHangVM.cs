using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Models.ViewModels
{
    public class TopKhachHangVM
    {
        public int MaKH { get; set; }
        public string TenKH { get; set; }
        public string SoDT { get; set; }

        public int TongSoLuong { get; set; }     // metric B
        public int TongHoaDon { get; set; }      // metric C
        public decimal TongTien { get; set; }    // metric A
    }
}
