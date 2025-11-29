using System;

namespace WebQuanLiCuaHangTapHoa.Models.ViewModels
{
    public class LichSuThongBaoVM
    {
        public int Id { get; set; }
        public int BaoNoId { get; set; }
        public string Loai { get; set; }
        public DateTime NgayGui { get; set; }

        public virtual BaoNo BaoNo { get; set; }
    }
}
