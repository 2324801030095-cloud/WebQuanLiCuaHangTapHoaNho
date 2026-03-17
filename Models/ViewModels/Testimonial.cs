using System;

namespace WebQuanLiCuaHangTapHoa.Models.ViewModels
{
    public class Testimonial
    {
        public int MaDG { get; set; }          // MaDanhGia
        public int MaKH { get; set; }          // Người đánh giá
        public string TenKH { get; set; }      // Lấy từ bảng KhachHang

        public int? MaSP { get; set; }         // Null = đánh giá cửa hàng, có SP = đánh giá sản phẩm

        public byte Diem { get; set; }         // 1–5 sao
        public string NoiDung { get; set; }
        public DateTime NgayDanhGia { get; set; }

        public bool TrangThai { get; set; }    // Đã duyệt hay chưa
    }
}