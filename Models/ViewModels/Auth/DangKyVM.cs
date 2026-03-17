using System;
using System.ComponentModel.DataAnnotations;

namespace WebQuanLiCuaHangTapHoa.Models.ViewModels.Auth
{
    public class DangKyVM
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(60, ErrorMessage = "Tên đăng nhập tối đa 60 ký tự")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(150, ErrorMessage = "Email tối đa 150 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(128, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 đến 128 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        [Display(Name = "Nhập lại mật khẩu")]
        public string XacNhanMatKhau { get; set; }

        [Required(ErrorMessage = "Họ tên khách hàng không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ tên khách hàng")]
        public string TenKH { get; set; }

        [Display(Name = "Số điện thoại (tùy chọn)")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
        [RegularExpression(@"^\+?[0-9]{9,15}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        [DataType(DataType.PhoneNumber)]
        public string SoDT { get; set; }

        [Display(Name = "Địa chỉ (tùy chọn)")]
        [StringLength(180, ErrorMessage = "Địa chỉ tối đa 180 ký tự")]
        public string DiaChi { get; set; }
    }
}
