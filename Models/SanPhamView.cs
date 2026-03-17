namespace WebQuanLiCuaHangTapHoa.Models
{
    // ViewModel dùng chung cho Trang chủ, Danh mục, FlashSale, Chi tiết
    public class SanPhamView
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public int GiaBan { get; set; }
        public int Ton { get; set; }
        public int MaDM { get; set; }
        public string HinhAnh { get; set; }

        public string MoTaNgan { get; set; }     // Mô tả tóm tắt
        public string MoTaChiTiet { get; set; }  // ⭐ Bổ sung để trang Chi tiết hoạt động

        // ============================
        // ⭐ GIẢM GIÁ – hỗ trợ filter
        // ============================

        public decimal? Giam { get; set; }       // % giảm (nếu có)
        public bool IsDiscount                  // Tự tính giảm giá
        {
            get { return Giam.HasValue && Giam.Value > 0; }
        }
    }
}
