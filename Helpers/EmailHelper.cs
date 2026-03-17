using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace WebQuanLiCuaHangTapHoa.Helpers
{
    public static class EmailHelper
    {
        public static bool TrySendResetCode(string toEmail, string code, out string error)
        {
            error = null;
            try
            {
                string host = ConfigurationManager.AppSettings["SmtpHost"];
                string portStr = ConfigurationManager.AppSettings["SmtpPort"];
                string user = ConfigurationManager.AppSettings["SmtpUser"];
                string pass = ConfigurationManager.AppSettings["SmtpPass"];
                string from = ConfigurationManager.AppSettings["SmtpFrom"];
                string sslStr = ConfigurationManager.AppSettings["SmtpEnableSsl"];

                if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
                {
                    error = "SMTP chưa cấu hình.";
                    return false;
                }

                int port = 587;
                int.TryParse(portStr, out port);
                bool enableSsl = true;
                bool.TryParse(sslStr, out enableSsl);

                var msg = new MailMessage();
                msg.From = new MailAddress(from, "Thanh Nhàn Grocery");
                msg.To.Add(toEmail);
                msg.Subject = "Mã xác minh đặt lại mật khẩu";
                msg.Body = BuildResetBody(code);
                msg.IsBodyHtml = true;
                msg.BodyEncoding = Encoding.UTF8;
                msg.SubjectEncoding = Encoding.UTF8;

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = enableSsl;
                    if (!string.IsNullOrWhiteSpace(user))
                    {
                        client.Credentials = new NetworkCredential(user, pass);
                    }
                    client.Send(msg);
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static string BuildResetBody(string code)
        {
            return $@"
<div style=""font-family:Arial,Helvetica,sans-serif;line-height:1.6;color:#0f172a"">
  <h2>Mã xác minh đặt lại mật khẩu</h2>
  <p>Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản Thanh Nhàn Grocery.</p>
  <p>Mã xác minh của bạn là:</p>
  <div style=""font-size:28px;font-weight:700;letter-spacing:4px;padding:10px 14px;background:#f1f5f9;border-radius:8px;display:inline-block"">{code}</div>
  <p>Mã có hiệu lực trong 10 phút.</p>
  <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.</p>
  <p>Trân trọng,<br/>Thanh Nhàn Grocery</p>
</div>";
        }
    }
}
