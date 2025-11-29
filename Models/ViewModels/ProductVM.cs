using System;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Models.ViewModels
{
    // ViewModel phẳng, chỉ dùng trong khu vực Admin
    public class ProductVM
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public int GiaBan { get; set; }

        public string TenDanhMuc { get; set; }
        public string TenDonViTinh { get; set; }
        public string TenKhuyenMai { get; set; }

        public bool HoatDong { get; set; }
        public string HinhAnh { get; set; }
        public string MoTaNgan { get; set; }
    }
}
