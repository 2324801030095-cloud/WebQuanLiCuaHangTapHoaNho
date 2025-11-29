using System;
using System.ComponentModel.DataAnnotations;

namespace WebQuanLiCuaHangTapHoa.Models.ViewModels.Auth
{
    public class DangKyVM
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        [Display(Name = "Nhập lại mật khẩu")]
        public string XacNhanMatKhau { get; set; }

        [Display(Name = "Họ tên khách hàng (tùy chọn)")]
        public string TenKH { get; set; }

        // ✅ Thêm 2 thuộc tính bị thiếu
        [Display(Name = "Số điện thoại (tùy chọn)")]
        [StringLength(15, ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDT { get; set; }

        [Display(Name = "Địa chỉ (tùy chọn)")]
        [StringLength(180)]
        public string DiaChi { get; set; }
    }
}
