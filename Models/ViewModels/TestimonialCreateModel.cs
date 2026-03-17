using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;

namespace WebQuanLiCuaHangTapHoa.Models.ViewModels
{
    public class TestimonialCreateModel
    {
        public int? MaSP { get; set; }  // Null => đánh giá cửa hàng

        [Range(0.5, 5, ErrorMessage = "Điểm đánh giá phải từ 0.5 đến 5.")]
        public decimal Diem { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá.")]
        [StringLength(500, ErrorMessage = "Nội dung tối đa 500 ký tự.")]
        public string NoiDung { get; set; }
    }
}
