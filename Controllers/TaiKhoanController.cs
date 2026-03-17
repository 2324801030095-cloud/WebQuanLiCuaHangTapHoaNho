using System;
using System.Linq;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using WebQuanLiCuaHangTapHoa.Models.ViewModels.Auth;
using WebQuanLiCuaHangTapHoa.Helpers;

namespace WebQuanLiCuaHangTapHoa.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db =
            new QuanLyTapHoaThanhNhanEntities1();

        // ==========================================================
        // ĐĂNG NHẬP (GET)
        // ==========================================================
        [HttpGet]
        public ActionResult DangNhap(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.LoginError = TempData["LoginError"];
            return View();
        }

        // ==========================================================
        // ĐĂNG NHẬP (POST) — CHUNG ADMIN + KHÁCH
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhap(string tenDangNhap, string matKhau, string returnUrl)
        {
            tenDangNhap = (tenDangNhap ?? "").Trim();
            matKhau = (matKhau ?? "").Trim();

            // ======================================================
            // VALIDATION
            // ======================================================
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
            {
                TempData["LoginError"] = "Vui lòng nhập đầy đủ thông tin.";

                // GIỮ returnUrl
                return RedirectToAction("DangNhap", new { returnUrl });
            }

            // Compute hash of provided password to compare against stored hashes
            string hash = PasswordHelper.HashSha256(matKhau);

            // ======================================================
            // 1) KIỂM TRA ADMIN
            // ======================================================
            var admin = _db.TaiKhoan.FirstOrDefault(x =>
                x.TenDangNhap == tenDangNhap &&
                (x.MatKhau == matKhau || x.MatKhau == hash));

            if (admin != null)
            {
                Session["Admin"] = new
                {
                    admin.TenDangNhap,
                    admin.Quyen
                };

                return RedirectToAction("Index", "Admin", new { area = "Admin" });
            }

            // ======================================================
            // 2) KIỂM TRA TÀI KHOẢN KHÁCH HÀNG
            // ======================================================
            // Allow matching either the raw password (legacy plain text) or the hashed password
            var tk = _db.TaiKhoanKH.FirstOrDefault(x =>
                x.TenDangNhap == tenDangNhap &&
                (x.MatKhau == matKhau || x.MatKhau == hash) &&
                x.HoatDong == true);

            if (tk == null)
            {
                TempData["LoginError"] = "Sai tên đăng nhập hoặc mật khẩu.";

                // GIỮ returnUrl
                return RedirectToAction("DangNhap", new { returnUrl });
            }

            // If account matched using plaintext password, upgrade stored password to hashed value
            if (tk.MatKhau != hash)
            {
                try
                {
                    tk.MatKhau = hash;
                    _db.SaveChanges();
                }
                catch
                {
                    // swallow errors here to avoid breaking login flow; log if you have logging
                }
            }

            // Lấy thông tin Khách hàng
            var kh = _db.KhachHang.FirstOrDefault(k => k.MaKH == tk.MaKH);

            // ======================================================
            // SESSION KH — CHUẨN HÓA
            // ======================================================
            Session["KH"] = new
            {
                MaKH = kh != null ? kh.MaKH : tk.MaKH,
                TenKH = kh != null ? kh.TenKH : tk.TenDangNhap,
                HoTen = kh != null ? kh.TenKH : tk.TenDangNhap,
                SoDT = kh?.SoDT,
                DiaChi = kh?.DiaChi,
                Email = tk.Email ?? kh?.Email,
                TenDangNhap = tk.TenDangNhap
            };

            // ======================================================
            // TRẢ VỀ TRANG CŨ (NẾU CÓ) — tránh redirect ngược lại trang đăng nhập
            // ======================================================
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                var lower = returnUrl.ToLower();
                if (!lower.Contains("/taikhoan/dangnhap") && !lower.Contains("/taikhoan/dangky"))
                    return Redirect(returnUrl);
            }

            // MẶC ĐỊNH
            return RedirectToAction("Index", "Home");
        }


        // ==========================================================
        // ĐĂNG KÝ (GET)
        // ==========================================================
        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }

        // ==========================================================
        // ĐĂNG KÝ (POST)
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(DangKyVM model)
        {
            if (model == null)
            {
                TempData["RegError"] = "Dữ liệu nhập không hợp lệ. Vui lòng kiểm tra lại.";
                return View(new DangKyVM());
            }

            // Normalize input to avoid whitespace issues and keep login consistent
            model.TenDangNhap = (model.TenDangNhap ?? "").Trim();
            model.Email = (model.Email ?? "").Trim();
            model.MatKhau = (model.MatKhau ?? "").Trim();
            model.XacNhanMatKhau = (model.XacNhanMatKhau ?? "").Trim();
            model.TenKH = (model.TenKH ?? "").Trim();
            model.SoDT = string.IsNullOrWhiteSpace(model.SoDT) ? null : model.SoDT.Trim();
            model.SoDT = NormalizeVietnamPhone(model.SoDT);
            model.DiaChi = string.IsNullOrWhiteSpace(model.DiaChi) ? null : model.DiaChi.Trim();

            ModelState.Clear();
            TryValidateModel(model);

            // Server-side model validation
            if (!ModelState.IsValid)
            {
                TempData["RegError"] = "Dữ liệu nhập không hợp lệ. Vui lòng kiểm tra lại.";
                return View(model);
            }

            // Check duplicate username (customer/admin)
            if (_db.TaiKhoanKH.Any(x => x.TenDangNhap == model.TenDangNhap) ||
                _db.TaiKhoan.Any(x => x.TenDangNhap == model.TenDangNhap))
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
                return View(model);
            }

            // Check duplicate email
            if (!string.IsNullOrWhiteSpace(model.Email) &&
                _db.TaiKhoanKH.Any(x => x.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View(model);
            }

            try
            {
                using (var tran = _db.Database.BeginTransaction())
                {
                    // 1) Tạo khách hàng
                    var kh = new KhachHang
                    {
                        TenKH = model.TenKH,
                        SoDT = model.SoDT,
                        DiaChi = model.DiaChi,
                        Email = model.Email
                    };
                    _db.KhachHang.Add(kh);
                    _db.SaveChanges();

                    // 2) Tạo tài khoản KH — lưu mật khẩu đã băm để nhất quán với hệ thống
                    var tk = new TaiKhoanKH
                    {
                        TenDangNhap = model.TenDangNhap,
                        MatKhau = PasswordHelper.HashSha256(model.MatKhau), // store hash
                        Email = model.Email,
                        MaKH = kh.MaKH,
                        NgayDangKy = DateTime.Now,
                        HoatDong = true
                    };
                    _db.TaiKhoanKH.Add(tk);
                    _db.SaveChanges();

                    tran.Commit();

                    // 3) Auto login (session)
                    Session["KH"] = new
                    {
                        MaKH = kh.MaKH,
                        TenKH = kh.TenKH,
                        HoTen = kh.TenKH,
                        SoDT = kh.SoDT,
                        DiaChi = kh.DiaChi,
                        Email = model.Email,
                        TenDangNhap = model.TenDangNhap
                    };

                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                // Log ex if you have logging; for now show friendly message
                TempData["RegError"] = "Lỗi khi đăng ký: " + ex.Message;
                ModelState.AddModelError("", "Lỗi khi đăng ký: " + ex.Message);
                
                // Rollback transaction if still active
                if (_db.Database.CurrentTransaction != null)
                {
                    try
                    {
                        _db.Database.CurrentTransaction.Rollback();
                    }
                    catch { }
                }
                
                return View(model);
            }
        }

        // ==========================================================
        // THÔNG TIN TÀI KHOẢN
        // ==========================================================
        public ActionResult ThongTin()
        {
            if (Session["KH"] == null)
                return RedirectToAction("DangNhap");

            var user = (dynamic)Session["KH"];

            int maKH = (int)user.MaKH;

            var kh = _db.KhachHang.Find(maKH);
            var tk = _db.TaiKhoanKH.FirstOrDefault(x => x.MaKH == maKH);

            ViewBag.Email = tk?.Email;

            return View(kh);
        }

        // ==========================================================
        // ĐĂNG XUẤT
        // ==========================================================
        public ActionResult DangXuat()
        {
            Session.Remove("KH");
            Session.Remove("Admin");
            return RedirectToAction("Index", "Home");
        }

        // ==========================================================
        // QUÊN MẬT KHẨU (GET)
        // ==========================================================
        [HttpGet]
        public ActionResult QuenMatKhau()
        {
            EnsurePasswordResetTable();
            ViewBag.ForgotError = TempData["ForgotError"];
            ViewBag.ForgotSuccess = TempData["ForgotSuccess"];
            ViewBag.Step = "request";
            ViewBag.CaptchaCode = GenerateCaptchaCode();
            return View();
        }

        // ==========================================================
        // QUÊN MẬT KHẨU (POST) - GỬI MÃ
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuenMatKhau(string emailOrUser, string captcha)
        {
            EnsurePasswordResetTable();
            emailOrUser = (emailOrUser ?? "").Trim();
            captcha = (captcha ?? "").Trim();

            if (string.IsNullOrWhiteSpace(emailOrUser))
            {
                ViewBag.ForgotError = "Vui lòng nhập email hoặc tên đăng nhập.";
                ViewBag.Step = "request";
                ViewBag.CaptchaCode = GenerateCaptchaCode();
                return View();
            }

            if (!IsCaptchaValid(captcha))
            {
                ViewBag.ForgotError = "Mã xác minh chưa đúng. Vui lòng thử lại.";
                ViewBag.Step = "request";
                ViewBag.CaptchaCode = GenerateCaptchaCode();
                return View();
            }

            var key = emailOrUser.ToLower();
            var khAccount = _db.TaiKhoanKH.FirstOrDefault(x =>
                (x.Email != null && x.Email.ToLower() == key) ||
                x.TenDangNhap.ToLower() == key
            );

            if (khAccount == null)
            {
                ViewBag.ForgotError = "Email hoặc tên đăng nhập không tồn tại.";
                ViewBag.Step = "request";
                ViewBag.CaptchaCode = GenerateCaptchaCode();
                return View();
            }

            if (string.IsNullOrWhiteSpace(khAccount.Email))
            {
                ViewBag.ForgotError = "Tài khoản chưa có email đăng ký. Vui lòng liên hệ quản trị.";
                ViewBag.Step = "request";
                ViewBag.CaptchaCode = GenerateCaptchaCode();
                return View();
            }

            string resetCode = GenerateDigits(6);
            int resetId = CreatePasswordResetToken(khAccount, resetCode);

            if (!IsSmtpConfigured())
            {
                TryLogResetCode(resetCode, khAccount.Email);
                ViewBag.ForgotError = null;
                ViewBag.ForgotSuccess = "Email chưa được cấu hình. Mã xác minh đã được ghi log nội bộ để tiếp tục kiểm thử.";
                ViewBag.Step = "verify";
                ViewBag.ResetId = resetId;
                ViewBag.MaskedEmail = MaskEmail(khAccount.Email);
                return View();
            }

            string sendError;
            bool sent = WebQuanLiCuaHangTapHoa.Helpers.EmailHelper.TrySendResetCode(khAccount.Email, resetCode, out sendError);
            if (!sent)
            {
                TryLogResetCode(resetCode, khAccount.Email);
                ViewBag.ForgotError = null;
                ViewBag.ForgotSuccess = "Không thể gửi email tự động. Mã xác minh đã được ghi log nội bộ để tiếp tục kiểm thử.";
                ViewBag.Step = "verify";
                ViewBag.ResetId = resetId;
                ViewBag.MaskedEmail = MaskEmail(khAccount.Email);
                return View();
            }

            ViewBag.ForgotSuccess = "Mã xác minh đã được gửi về email đăng ký. Vui lòng kiểm tra hộp thư.";
            ViewBag.Step = "verify";
            ViewBag.ResetId = resetId;
            ViewBag.MaskedEmail = MaskEmail(khAccount.Email);
            return View();
        }

        // ==========================================================
        // XÁC MINH MÃ (POST)
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XacMinhMa(int resetId, string resetCode)
        {
            EnsurePasswordResetTable();
            resetCode = (resetCode ?? "").Trim();
            if (string.IsNullOrWhiteSpace(resetCode))
            {
                ViewBag.ForgotError = "Vui lòng nhập mã xác minh.";
                ViewBag.Step = "verify";
                ViewBag.ResetId = resetId;
                return View("QuenMatKhau");
            }

            var token = GetPasswordResetToken(resetId);
            if (token == null || token.Status != "pending")
            {
                ViewBag.ForgotError = "Mã xác minh không hợp lệ hoặc đã hết hạn.";
                ViewBag.Step = "request";
                ViewBag.CaptchaCode = GenerateCaptchaCode();
                return View("QuenMatKhau");
            }

            if (token.ExpiresAt < DateTime.Now)
            {
                MarkResetTokenExpired(resetId);
                ViewBag.ForgotError = "Mã xác minh đã hết hạn. Vui lòng yêu cầu lại.";
                ViewBag.Step = "request";
                ViewBag.CaptchaCode = GenerateCaptchaCode();
                return View("QuenMatKhau");
            }

            if (!string.Equals(token.ResetCode, resetCode, StringComparison.Ordinal))
            {
                ViewBag.ForgotError = "Mã xác minh chưa đúng. Vui lòng kiểm tra lại.";
                ViewBag.Step = "verify";
                ViewBag.ResetId = resetId;
                ViewBag.MaskedEmail = MaskEmail(token.Email);
                return View("QuenMatKhau");
            }

            MarkResetTokenVerified(resetId);
            Session["ResetTokenId"] = resetId;
            return RedirectToAction("DatLaiMatKhau");
        }

        // ==========================================================
        // ĐẶT LẠI MẬT KHẨU (GET)
        // ==========================================================
        [HttpGet]
        public ActionResult DatLaiMatKhau()
        {
            if (Session["ResetTokenId"] == null)
            {
                return RedirectToAction("QuenMatKhau");
            }

            return View();
        }

        // ==========================================================
        // ĐẶT LẠI MẬT KHẨU (POST)
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DatLaiMatKhau(string matKhau, string xacNhanMatKhau)
        {
            if (Session["ResetTokenId"] == null)
            {
                return RedirectToAction("QuenMatKhau");
            }

            matKhau = (matKhau ?? "").Trim();
            xacNhanMatKhau = (xacNhanMatKhau ?? "").Trim();

            if (string.IsNullOrWhiteSpace(matKhau) || string.IsNullOrWhiteSpace(xacNhanMatKhau))
            {
                ViewBag.ResetError = "Vui lòng nhập đầy đủ mật khẩu mới.";
                return View();
            }

            if (matKhau.Length < 6 || matKhau.Length > 128)
            {
                ViewBag.ResetError = "Mật khẩu phải từ 6 đến 128 ký tự.";
                return View();
            }

            if (!string.Equals(matKhau, xacNhanMatKhau, StringComparison.Ordinal))
            {
                ViewBag.ResetError = "Mật khẩu nhập lại chưa khớp.";
                return View();
            }

            int resetId = (int)Session["ResetTokenId"];
            var token = GetPasswordResetToken(resetId);
            if (token == null || token.Status != "verified" || token.ExpiresAt < DateTime.Now)
            {
                ViewBag.ResetError = "Phiên đặt lại mật khẩu đã hết hạn. Vui lòng yêu cầu lại.";
                return View();
            }

            TaiKhoanKH account = null;
            if (token.MaKH.HasValue)
            {
                account = _db.TaiKhoanKH.FirstOrDefault(x => x.MaKH == token.MaKH.Value);
            }
            if (account == null && !string.IsNullOrWhiteSpace(token.TenDangNhap))
            {
                account = _db.TaiKhoanKH.FirstOrDefault(x => x.TenDangNhap == token.TenDangNhap);
            }
            if (account == null && !string.IsNullOrWhiteSpace(token.Email))
            {
                account = _db.TaiKhoanKH.FirstOrDefault(x => x.Email == token.Email);
            }

            if (account == null)
            {
                ViewBag.ResetError = "Không tìm thấy tài khoản để cập nhật.";
                return View();
            }

            account.MatKhau = PasswordHelper.HashSha256(matKhau);
            _db.SaveChanges();
            MarkResetTokenUsed(resetId);

            Session.Remove("ResetTokenId");
            TempData["ForgotSuccess"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();
            base.Dispose(disposing);
        }

        private static string NormalizeVietnamPhone(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            var digits = new string(raw.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("84"))
            {
                digits = digits.Substring(2);
            }
            if (digits.StartsWith("0"))
            {
                digits = digits.Substring(1);
            }

            return string.IsNullOrWhiteSpace(digits) ? null : "+84" + digits;
        }

        private void EnsurePasswordResetTable()
        {
            const string sql = @"
IF OBJECT_ID('dbo.PasswordResetTokens', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordResetTokens (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        MaKH INT NULL,
        TenDangNhap NVARCHAR(100) NULL,
        Email NVARCHAR(256) NULL,
        ResetCode NVARCHAR(6) NOT NULL,
        CreatedAt DATETIME NOT NULL,
        ExpiresAt DATETIME NOT NULL,
        UsedAt DATETIME NULL,
        Status NVARCHAR(20) NOT NULL,
        RequestIp NVARCHAR(50) NULL,
        UserAgent NVARCHAR(200) NULL
    );
END";
            _db.Database.ExecuteSqlCommand(sql);
        }

        private int CreatePasswordResetToken(TaiKhoanKH khAccount, string resetCode)
        {
            var now = DateTime.Now;
            var expires = now.AddMinutes(10);
            var sql = @"
INSERT INTO dbo.PasswordResetTokens (MaKH, TenDangNhap, Email, ResetCode, CreatedAt, ExpiresAt, Status, RequestIp, UserAgent)
VALUES (@MaKH, @TenDangNhap, @Email, @ResetCode, @CreatedAt, @ExpiresAt, @Status, @RequestIp, @UserAgent);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return _db.Database.SqlQuery<int>(
                sql,
                new SqlParameter("@MaKH", (object)khAccount.MaKH),
                new SqlParameter("@TenDangNhap", (object)khAccount.TenDangNhap ?? DBNull.Value),
                new SqlParameter("@Email", (object)khAccount.Email ?? DBNull.Value),
                new SqlParameter("@ResetCode", resetCode),
                new SqlParameter("@CreatedAt", now),
                new SqlParameter("@ExpiresAt", expires),
                new SqlParameter("@Status", "pending"),
                new SqlParameter("@RequestIp", (object)Request?.UserHostAddress ?? DBNull.Value),
                new SqlParameter("@UserAgent", (object)Request?.UserAgent ?? DBNull.Value)
            ).First();
        }

        private PasswordResetTokenRecord GetPasswordResetToken(int resetId)
        {
            var sql = @"SELECT TOP 1 Id, MaKH, TenDangNhap, Email, ResetCode, CreatedAt, ExpiresAt, UsedAt, Status
                        FROM dbo.PasswordResetTokens WHERE Id = @Id";
            return _db.Database.SqlQuery<PasswordResetTokenRecord>(
                sql,
                new SqlParameter("@Id", resetId)
            ).FirstOrDefault();
        }

        private void MarkResetTokenVerified(int resetId)
        {
            _db.Database.ExecuteSqlCommand(
                "UPDATE dbo.PasswordResetTokens SET Status = @Status WHERE Id = @Id",
                new SqlParameter("@Status", "verified"),
                new SqlParameter("@Id", resetId)
            );
        }

        private void MarkResetTokenUsed(int resetId)
        {
            _db.Database.ExecuteSqlCommand(
                "UPDATE dbo.PasswordResetTokens SET Status = @Status, UsedAt = @UsedAt WHERE Id = @Id",
                new SqlParameter("@Status", "used"),
                new SqlParameter("@UsedAt", DateTime.Now),
                new SqlParameter("@Id", resetId)
            );
        }

        private void MarkResetTokenExpired(int resetId)
        {
            _db.Database.ExecuteSqlCommand(
                "UPDATE dbo.PasswordResetTokens SET Status = @Status WHERE Id = @Id",
                new SqlParameter("@Status", "expired"),
                new SqlParameter("@Id", resetId)
            );
        }

        private string GenerateCaptchaCode()
        {
            var code = GenerateDigits(4);
            Session["ForgotCaptcha"] = code;
            return code;
        }

        private bool IsCaptchaValid(string captcha)
        {
            var expected = Session["ForgotCaptcha"] as string;
            return !string.IsNullOrWhiteSpace(expected) &&
                   string.Equals(expected, captcha, StringComparison.Ordinal);
        }

        private string GenerateDigits(int length)
        {
            var bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append((bytes[i] % 10).ToString());
            }
            return sb.ToString();
        }

        private string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "";
            var at = email.IndexOf("@");
            if (at <= 1) return "****" + email.Substring(at);
            var name = email.Substring(0, at);
            var masked = name.Substring(0, 2) + new string('*', Math.Max(2, name.Length - 2));
            return masked + email.Substring(at);
        }

        private bool IsSmtpConfigured()
        {
            var host = ConfigurationManager.AppSettings["SmtpHost"];
            var from = ConfigurationManager.AppSettings["SmtpFrom"];
            return !string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(from);
        }

        private void TryLogResetCode(string resetCode, string email)
        {
            try
            {
                string logPath = Server.MapPath("~/App_Data/reset.log");
                System.IO.File.AppendAllText(
                    logPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] RESET_CODE={resetCode} EMAIL={email}\r\n"
                );
            }
            catch { }
        }

        private class PasswordResetTokenRecord
        {
            public int Id { get; set; }
            public int? MaKH { get; set; }
            public string TenDangNhap { get; set; }
            public string Email { get; set; }
            public string ResetCode { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime ExpiresAt { get; set; }
            public DateTime? UsedAt { get; set; }
            public string Status { get; set; }
        }
    }
}
