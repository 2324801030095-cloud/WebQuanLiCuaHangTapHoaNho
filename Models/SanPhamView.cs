namespace WebQuanLiCuaHangTapHoa.Models
{
    // ViewModel gọn cho hiển thị sản phẩm ở cả Trang chủ và Danh mục
    public class SanPhamView
    {
        public int MaSP { get; set; }         // Mã sản phẩm
        public string TenSP { get; set; }     // Tên sản phẩm
        public int GiaBan { get; set; }       // Giá bán
        public int Ton { get; set; }          // Số lượng tồn kho
        public int MaDM { get; set; }
        public string HinhAnh { get; set; }  // ✅ thêm
                                             // Mã danh mục (thêm mới để lọc)
    }
}
