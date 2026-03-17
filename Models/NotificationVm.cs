using System;

namespace WebQuanLiCuaHangTapHoa.Models
{
    public class NotificationVm
    {
        public int MaThongBao { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public string Icon { get; set; }
        public DateTime? NgayGui { get; set; }
        public string Link { get; set; }
        public bool IsRead { get; set; }
    }
}