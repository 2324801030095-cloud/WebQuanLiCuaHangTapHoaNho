using System;
using System.Collections.Generic;
using WebQuanLiCuaHangTapHoa.Models;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Models.ViewModels
{
    public class ChiTietPN_VM
    {
        public int MaCTPN { get; set; }
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public int SoLuong { get; set; }
        public int DonGia { get; set; }
        public int ThanhTien { get; set; }
    }

    public class ChiTietPN_Input
    {
        public int MaSP { get; set; }
        public int SoLuong { get; set; }
        public int DonGia { get; set; }
    }

    public class PhieuNhapCreateVM
    {
        public PhieuNhap Phieu { get; set; }
        public List<ChiTietPN_Input> ChiTiet { get; set; }
    }
}
