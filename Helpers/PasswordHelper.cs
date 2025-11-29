using System.Security.Cryptography;   // SHA256
using System.Text;                    // Encoding

namespace WebQuanLiCuaHangTapHoa.Helpers
{
    public static class PasswordHelper
    {
        // Hàm băm SHA256 cho mật khẩu
        public static string HashSha256(string input)
        {
            // Trả về chuỗi hex sha256 cho input
            using (var sha = SHA256.Create())                                  // Tạo đối tượng SHA256
            {
                var bytes = Encoding.UTF8.GetBytes(input ?? string.Empty);     // Chuyển chuỗi -> bytes
                var hash = sha.ComputeHash(bytes);                             // Băm bytes
                var sb = new StringBuilder(hash.Length * 2);                   // Chuẩn bị builder cho hex
                foreach (var b in hash) sb.Append(b.ToString("x2"));           // Mỗi byte -> 2 ký tự hex
                return sb.ToString();                                          // Trả kết quả
            }
        }
    }
}
