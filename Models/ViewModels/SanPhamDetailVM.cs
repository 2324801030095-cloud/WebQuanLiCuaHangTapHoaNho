// Dòng này cho phép dùng các kiểu dữ liệu cơ bản (int, string, DateTime, …)
using System;

// (Tùy chọn) Nếu sau này bạn muốn hiển thị/validate dữ liệu đẹp hơn có thể dùng DataAnnotations
using System.ComponentModel.DataAnnotations;

namespace WebQuanLiCuaHangTapHoa.Models.ViewModels
{
    // Lớp ViewModel này gom dữ liệu từ nhiều bảng (SanPham, DanhMuc, DonViTinh, KhuyenMai, Kho)
    // Mục tiêu: cung cấp đủ thông tin cho View Chi tiết sản phẩm (Views/SanPham/ChiTiet.cshtml)
    public class SanPhamDetailVM
    {
        // ====== Khối thông tin “cốt lõi” của sản phẩm ======

        [Display(Name = "Mã SP")]               // (Tùy chọn) Nhãn hiển thị đẹp
        public int MaSP { get; set; }           // Mã sản phẩm (khóa chính)

        [Display(Name = "Tên sản phẩm")]
        public string TenSP { get; set; }       // Tên sản phẩm

        [Display(Name = "Giá bán")]
        public int GiaBan { get; set; }         // Giá gốc niêm yết (đơn vị: VND)

        [Display(Name = "Tồn kho")]
        public int Ton { get; set; }            // Số lượng tồn hiện tại (đọc từ bảng Kho)

        // ====== Danh mục / Đơn vị tính ======

        public int MaDM { get; set; }           // Mã danh mục (để tạo link quay lại danh mục)
        public string TenDM { get; set; }       // Tên danh mục (hiển thị breadcrumb)
        public string DonViTinh { get; set; }   // Tên đơn vị tính (Chai/Lon/Kg/...)

        // ====== Hình ảnh + mô tả ======

        public string HinhAnh { get; set; }     // URL ảnh chính của sản phẩm (nếu null View sẽ dùng ảnh fallback)
        public string MoTaNgan { get; set; }    // Mô tả ngắn (hiển thị gần tiêu đề/giá)
        public string MoTaChiTiet { get; set; } // Mô tả dài (khu nội dung chi tiết)

        // ====== Thông tin khuyến mãi (nếu có) ======

        public int? MaKM { get; set; }          // Mã khuyến mãi (nullable: có thể không có KM)
        public string TenKM { get; set; }       // Tên chương trình khuyến mãi (ví dụ: “Giảm 10% đồ uống”)
        public decimal? Giam { get; set; }      // Phần trăm giảm (0–100), nullable
        public DateTime? TuNgay { get; set; }   // Ngày bắt đầu áp dụng KM
        public DateTime? DenNgay { get; set; }  // Ngày kết thúc KM

        // ====== Trường tính toán sẵn cho View ======

        [Display(Name = "Giá sau KM")]
        public int GiaSauKM { get; set; }       // Giá sau khi áp dụng KM hợp lệ; nếu không có KM => = GiaBan

        // ====== Thuận tiện cho View: chuỗi ngày / thông tin KM đã format sẵn ======
        // Tránh viết nhiều @Model.TuNgay?.ToString(...) trực tiếp trong Razor (dễ gây lỗi khi mix @)
        public string TuNgayStr => TuNgay.HasValue ? TuNgay.Value.ToString("dd/MM/yyyy") : string.Empty;
        public string DenNgayStr => DenNgay.HasValue ? DenNgay.Value.ToString("dd/MM/yyyy") : string.Empty;

        // KmInfo trả về "TenKM (dd/MM/yyyy - dd/MM/yyyy)" hoặc chỉ dates hoặc empty
        public string KmInfo
        {
            get
            {
                var dates = string.Empty;
                if (TuNgay.HasValue || DenNgay.HasValue)
                {
                    dates = $"({TuNgayStr} - {DenNgayStr})".Trim();
                }

                if (string.IsNullOrWhiteSpace(TenKM))
                    return dates.Trim();

                return $"{TenKM} {dates}".Trim();
            }
        }

        // (Tùy chọn) Bạn có thể thêm các flag tiện lợi cho View:
        public bool CoKhuyenMai => Giam.HasValue && Giam.Value > 0 && GiaSauKM < GiaBan;
    }
}
