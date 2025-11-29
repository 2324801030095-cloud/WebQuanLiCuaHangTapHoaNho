using System.ComponentModel.DataAnnotations; // Dùng cho [Required], [StringLength]

namespace WebQuanLiCuaHangTapHoa.Models.ViewModels.Auth
{
    // ViewModel cho form Đăng nhập
    public class DangNhapVM
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]   // Bắt buộc
        [StringLength(60)]                                         // Giới hạn độ dài
        public string TenDangNhap { get; set; }                    // Tên đăng nhập

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]        // Bắt buộc
        [DataType(DataType.Password)]                              // Hiển thị ********
        [StringLength(64, MinimumLength = 4)]                      // Độ dài hợp lệ
        public string MatKhau { get; set; }                        // Mật khẩu (plain trong form)

        public string ReturnUrl { get; set; }                      // Nơi quay lại sau đăng nhập
    }
}