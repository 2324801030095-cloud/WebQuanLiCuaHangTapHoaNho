using System;
using System.Collections.Generic;

namespace WebQuanLiCuaHangTapHoa.Helpers
{
    /// <summary>
    /// Helper để quản lý icon cho các danh mục sản phẩm
    /// Mapping: TenDM từ DB (CÓ DẤU) → Icon Bootstrap
    /// </summary>
    public static class CategoryIconHelper
    {
        /// <summary>
        /// Bản đồ danh mục: Key = TenDM từ DB (có dấu), Value = Icon class
        /// Thêm cả phiên bản không dấu để handle trường hợp DB lưu sai format
        /// </summary>
        private static readonly Dictionary<string, string> CategoryIconMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) // Case-insensitive lookup
            {
                // Phiên bản có dấu (chính)
                ["Rau Củ Quả"] = "bi-leaf",
                ["Đồ Uống"] = "bi-cup-straw",
                ["Bánh Kẹo"] = "bi-cake2",
                ["Sữa Các Loại"] = "bi-cup",
                ["Gia Vị Và Dầu Ăn"] = "bi-droplet",
                ["Hóa Phẩm Gia Dụng"] = "bi-bucket",
                ["Gạo Và Ngũ Cốc"] = "bi-grain",
                ["Thịt Và Trứng"] = "bi-egg",
                
                // Phiên bản không dấu (fallback)
                ["Rau Cu Qua"] = "bi-leaf",
                ["Do Uong"] = "bi-cup-straw",
                ["Banh Keo"] = "bi-cake2",
                ["Sua Cac Loai"] = "bi-cup",
                ["Gia Vi Va Dau An"] = "bi-droplet",
                ["Hoa Pham Gia Dung"] = "bi-bucket",
                ["Gao Va Ngu Coc"] = "bi-grain",
                ["Thit Va Trung"] = "bi-egg",
                
                // Phiên bản khác (nếu DB lưu tên khác)
                ["Rau"] = "bi-leaf",
                ["Quả"] = "bi-leaf",
                ["Nước"] = "bi-cup-straw",
                ["Bánh"] = "bi-cake2",
                ["Kẹo"] = "bi-cake2",
                ["Sữa"] = "bi-cup",
                ["Gia Vị"] = "bi-droplet",
                ["Dầu"] = "bi-droplet",
                ["Hóa Chất"] = "bi-bucket",
                ["Gạo"] = "bi-grain",
                ["Ngũ Cốc"] = "bi-grain",
                ["Thịt"] = "bi-egg",
                ["Trứng"] = "bi-egg"
            };

        private const string DefaultIcon = "bi-tag";

        /// <summary>
        /// Lấy icon cho danh mục (với trim để xử lý khoảng trắng từ DB)
        /// </summary>
        /// <param name="categoryName">Tên danh mục từ DB (TenDM có dấu)</param>
        /// <returns>Icon class (VD: "bi-leaf"), fallback "bi-tag" nếu không tìm thấy</returns>
        public static string GetIcon(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return DefaultIcon;

            // Trim để loại bỏ khoảng trắng đầu/cuối từ DB
            var trimmedName = categoryName.Trim();

            // Tìm kiếm case-insensitive (do Dictionary dùng OrdinalIgnoreCase)
            if (CategoryIconMap.ContainsKey(trimmedName))
                return CategoryIconMap[trimmedName];

            // Thử tìm bằng cách loại bỏ dấu nếu vẫn không tìm thấy
            var unaccentedName = RemoveAccents(trimmedName);
            if (unaccentedName != trimmedName && CategoryIconMap.ContainsKey(unaccentedName))
                return CategoryIconMap[unaccentedName];

            // Log hoặc debug: tên danh mục không tìm thấy
            System.Diagnostics.Debug.WriteLine($"[CategoryIconHelper] Icon không tìm thấy cho: '{categoryName}' (trimmed: '{trimmedName}', unaccented: '{unaccentedName}')");
            
            return DefaultIcon;
        }

        /// <summary>
        /// Lấy tên hiển thị (trim để bỏ khoảng trắng)
        /// </summary>
        /// <param name="categoryName">Tên danh mục từ DB (TenDM)</param>
        /// <returns>Tên để hiển thị</returns>
        public static string GetDisplayName(string categoryName)
        {
            return string.IsNullOrWhiteSpace(categoryName) ? categoryName : categoryName.Trim();
        }

        /// <summary>
        /// Lấy cặp (tên hiển thị, icon) cho danh mục
        /// </summary>
        /// <param name="categoryName">Tên danh mục từ DB (TenDM)</param>
        /// <returns>Tuple (tên hiển thị, icon)</returns>
        public static (string display, string icon) GetCategoryInfo(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return (categoryName, DefaultIcon);

            var trimmedName = categoryName.Trim();
            var icon = GetIcon(categoryName); // Dùng GetIcon() để tận dụng logic tìm kiếm

            return (trimmedName, icon);
        }

        /// <summary>
        /// Lấy danh sách tất cả các icon hỗ trợ (cho debug/test)
        /// </summary>
        /// <returns>Dictionary các danh mục và icon tương ứng</returns>
        public static IReadOnlyDictionary<string, string> GetAllMappings()
        {
            return CategoryIconMap;
        }

        /// <summary>
        /// Loại bỏ dấu từ chuỗi tiếng Việt (tạm thời cho tìm kiếm backup)
        /// </summary>
        private static string RemoveAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            string[] vietnameseChars = new[] { "ả", "ã", "á", "à", "ạ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
                "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "đ", "è", "é", "ẻ", "ẽ", "ẹ", "ê", "ế", "ề", "ể", "ễ", "ệ",
                "ì", "í", "ỉ", "ĩ", "ị", "ò", "ó", "ỏ", "õ", "ọ", "ô", "ố", "ồ", "ổ", "ỗ", "ộ", "ơ", "ớ",
                "ờ", "ở", "ỡ", "ợ", "ù", "ú", "ủ", "ũ", "ụ", "ư", "ứ", "ừ", "ử", "ữ", "ự", "ỳ", "ý", "ỷ",
                "ỹ", "ỵ", "Ả", "Ã", "Á", "À", "Ạ", "Ă", "Ắ", "Ằ", "Ẳ", "Ẵ", "Ặ", "Â", "Ấ", "Ầ", "Ẩ",
                "Ẫ", "Ậ", "Đ", "È", "É", "Ẻ", "Ẽ", "Ẹ", "Ê", "Ế", "Ề", "Ể", "Ễ", "Ệ", "Ì", "Í", "Ỉ", "Ĩ",
                "Ị", "Ò", "Ó", "Ỏ", "Õ", "Ọ", "Ô", "Ố", "Ồ", "Ổ", "Ỗ", "Ộ", "Ơ", "Ớ", "Ờ", "Ở", "Ỡ", "Ợ",
                "Ù", "Ú", "Ủ", "Ũ", "Ụ", "Ư", "Ứ", "Ừ", "Ử", "Ữ", "Ự", "Ỳ", "Ý", "Ỷ", "Ỹ", "Ỵ" };
            string[] replacements = new[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
                "a", "a", "a", "a", "a", "a", "d", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e",
                "i", "i", "i", "i", "i", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o",
                "o", "o", "o", "o", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "y", "y", "y",
                "y", "y", "A", "A", "A", "A", "A", "A", "A", "A", "A", "A", "A", "A", "A", "A", "A", "A",
                "A", "A", "D", "E", "E", "E", "E", "E", "E", "E", "E", "E", "E", "E", "I", "I", "I", "I",
                "I", "O", "O", "O", "O", "O", "O", "O", "O", "O", "O", "O", "O", "O", "O", "O", "O", "O",
                "U", "U", "U", "U", "U", "U", "U", "U", "U", "U", "U", "Y", "Y", "Y", "Y", "Y" };

            for (int i = 0; i < vietnameseChars.Length; i++)
            {
                text = text.Replace(vietnameseChars[i], replacements[i]);
            }

            return text;
        }
    }
}
