using System;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Models
{
    public class SalaryLogEntry
    {
        public DateTime When { get; set; }
        public int MaNV { get; set; }
        public string TenNV { get; set; }
        public int OldSalary { get; set; }
        public int NewSalary { get; set; }
        public string ChangedBy { get; set; }
    }
}