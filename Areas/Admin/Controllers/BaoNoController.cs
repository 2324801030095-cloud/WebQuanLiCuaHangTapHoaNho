using PagedList;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using WebQuanLiCuaHangTapHoa.Models.ViewModels;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class BaoNoController : Controller
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // =========================== INDEX ===========================
        public ActionResult Index(string filter = "all", int page = 1)
        {
            int pageSize = 10;

            var list = _db.BaoNo
                .ToList()
                .Select(b => new BaoNoVM
                {
                    BaoNo = b,
                    Days = (DateTime.Now - b.NgayTao).Days,
                    Stage =
                        (DateTime.Now - b.NgayTao).Days >= 30 ? 3 :
                        (DateTime.Now - b.NgayTao).Days >= 15 ? 2 :
                        (DateTime.Now - b.NgayTao).Days >= 7 ? 1 : 0
                });

            if (filter == "due7") list = list.Where(x => x.Stage == 1);
            if (filter == "due15") list = list.Where(x => x.Stage == 2);
            if (filter == "due30") list = list.Where(x => x.Stage == 3);

            ViewBag.Filter = filter;

            return View(list.OrderByDescending(x => x.BaoNo.NgayTao).ToPagedList(page, pageSize));
        }

        // =========================== THÊM BÁO NỢ ===========================
        public ActionResult ThemBaoNo()
        {
            ViewBag.KhachHang = _db.KhachHang.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemBaoNo(int MaKH, decimal SoTien, string GhiChu)
        {
            var bn = new BaoNo
            {
                MaKH = MaKH,
                SoTien = SoTien,
                GhiChu = GhiChu,
                NgayTao = DateTime.Now
            };

            _db.BaoNo.Add(bn);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        // =========================== CHI TIẾT ===========================
        public ActionResult ChiTietBaoNo(int id)
        {
            var bn = _db.BaoNo.Find(id);
            if (bn == null) return HttpNotFound();

            return View(bn);
        }

        // =========================== SỬA BÁO NỢ ===========================
        public ActionResult SuaBaoNo(int id)
        {
            var bn = _db.BaoNo.Find(id);
            if (bn == null) return HttpNotFound();

            ViewBag.KhachHang = _db.KhachHang.ToList();
            return View(bn);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaBaoNo(BaoNo model)
        {
            var bn = _db.BaoNo.Find(model.Id);
            if (bn == null) return HttpNotFound();

            bn.SoTien = model.SoTien;
            bn.GhiChu = model.GhiChu;
            bn.MaKH = model.MaKH;

            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // =========================== XÓA ===========================
        public ActionResult Xoa(int id)
        {
            var bn = _db.BaoNo.Find(id);
            if (bn != null)
            {
                _db.BaoNo.Remove(bn);
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // =========================== CHECK LIMIT (1 lần/ngày) ===========================
        private bool DaGuiHomNay(int baoNoId, string loai)
        {
            DateTime start = DateTime.Today;
            DateTime end = start.AddDays(1);

            return _db.LichSuThongBao.Any(x =>
                x.BaoNoId == baoNoId &&
                x.Loai == loai &&
                x.NgayGui >= start &&
                x.NgayGui < end);
        }

        // =========================== GỬI EMAIL ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuiEmail(int id)
        {
            var bn = _db.BaoNo.Find(id);
            if (bn == null) return Json(new { success = false, message = "Không tìm thấy báo nợ!" });

            var kh = bn.KhachHang;
            if (kh == null) return Json(new { success = false, message = "Khách hàng không tồn tại!" });
            if (string.IsNullOrEmpty(kh.Email)) return Json(new { success = false, message = "Khách hàng chưa có email!" });
            if (DaGuiHomNay(id, "EMAIL")) return Json(new { success = false, message = "Hôm nay đã gửi Email rồi!" });

            string noiDung = $"Xin chào {kh.TenKH}, bạn đang nợ {bn.SoTien:N0}₫.";

            try
            {
                // Lấy cấu hình SMTP từ AppSettings nếu có, fallback sang gmail demo
                var smtpHost = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
                var smtpUser = ConfigurationManager.AppSettings["SmtpUser"] ?? "email@gmail.com";
                var smtpPass = ConfigurationManager.AppSettings["SmtpPass"] ?? "password-app";
                var fromEmail = ConfigurationManager.AppSettings["FromEmail"] ?? smtpUser;

                using (var smtp = new SmtpClient(smtpHost, smtpPort))
                {
                    smtp.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["SmtpEnableSsl"] ?? "true");
                    if (!string.IsNullOrEmpty(smtpUser))
                        smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);

                    var mail = new MailMessage();
                    mail.From = new MailAddress(fromEmail);
                    mail.To.Add(kh.Email);
                    mail.Subject = "Nhắc nợ";
                    mail.Body = noiDung;
                    smtp.Send(mail);
                }

                _db.LichSuThongBao.Add(new LichSuThongBao
                {
                    BaoNoId = id,
                    NoiDung = noiDung,
                    Loai = "EMAIL",
                    NgayGui = DateTime.Now
                });
                _db.SaveChanges();

                return Json(new { success = true, message = "Email đã gửi!" });
            }
            catch (SmtpException sx)
            {
                return Json(new { success = false, message = "Lỗi SMTP: " + sx.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // =========================== GỬI SMS (giả lập) ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuiSMS(int id)
        {
            var bn = _db.BaoNo.Find(id);
            if (bn == null) return Json(new { success = false, message = "Không tìm thấy báo nợ!" });

            var kh = bn.KhachHang;
            if (kh == null) return Json(new { success = false, message = "Khách hàng không tồn tại!" });
            if (string.IsNullOrEmpty(kh.SoDT)) return Json(new { success = false, message = "Khách hàng không có SĐT!" });
            if (DaGuiHomNay(id, "SMS")) return Json(new { success = false, message = "Hôm nay đã gửi SMS rồi!" });

            string smsContent = $"Nhắc nợ: bạn còn nợ {bn.SoTien:N0}₫.";

            try
            {
                _db.LichSuThongBao.Add(new LichSuThongBao
                {
                    BaoNoId = id,
                    Loai = "SMS",
                    NoiDung = smsContent,
                    NgayGui = DateTime.Now
                });
                _db.SaveChanges();

                return Json(new { success = true, message = "SMS đã gửi (giả lập)!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi gửi SMS: " + ex.Message });
            }
        }

        // =========================== LỊCH SỬ THÔNG BÁO ===========================
        public ActionResult LichSuThongBao(DateTime? from, DateTime? to, string name, string type, int page = 1)
        {
            int size = 10;

            var q = _db.LichSuThongBao.AsQueryable();

            if (from != null)
                q = q.Where(x => x.NgayGui >= from.Value.Date);

            if (to != null)
                q = q.Where(x => x.NgayGui < to.Value.Date.AddDays(1));

            if (!string.IsNullOrEmpty(name))
                q = q.Where(x => x.BaoNo.KhachHang.TenKH.Contains(name));

            if (!string.IsNullOrEmpty(type))
                q = q.Where(x => x.Loai == type);

            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");
            ViewBag.Name = name;
            ViewBag.Type = type;

            return View(q.OrderByDescending(x => x.NgayGui).ToPagedList(page, size));
        }

        // =========================== SỬA NỘI DUNG TIN NHẮN (Lịch sử) ===========================
        public ActionResult SuaTinNhan(int id)
        {
            var ls = _db.LichSuThongBao.Find(id);
            if (ls == null) return HttpNotFound();
            return View(ls);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaTinNhan(LichSuThongBao model)
        {
            var ls = _db.LichSuThongBao.Find(model.Id);
            if (ls == null) return HttpNotFound();

            ls.NoiDung = model.NoiDung;
            _db.SaveChanges();

            return RedirectToAction("LichSuThongBao");
        }

        // =========================== GỬI LẠI (Resend) ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuiLai(int id)
        {
            var ls = _db.LichSuThongBao.Find(id);
            if (ls == null) return Json(new { success = false, message = "Không tìm thấy lịch sử!" });

            if (ls.Loai == "EMAIL")
            {
                try
                {
                    var kh = ls.BaoNo.KhachHang;
                    if (kh == null || string.IsNullOrEmpty(kh.Email))
                        return Json(new { success = false, message = "Khách hàng không có email!" });

                    var smtpHost = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
                    var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
                    var smtpUser = ConfigurationManager.AppSettings["SmtpUser"] ?? "email@gmail.com";
                    var smtpPass = ConfigurationManager.AppSettings["SmtpPass"] ?? "password-app";
                    var fromEmail = ConfigurationManager.AppSettings["FromEmail"] ?? smtpUser;

                    using (var smtp = new SmtpClient(smtpHost, smtpPort))
                    {
                        smtp.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["SmtpEnableSsl"] ?? "true");
                        if (!string.IsNullOrEmpty(smtpUser))
                            smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);

                        var mail = new MailMessage();
                        mail.From = new MailAddress(fromEmail);
                        mail.To.Add(kh.Email);
                        mail.Subject = "Nhắc nợ (gửi lại)";
                        mail.Body = ls.NoiDung ?? $"Nhắc nợ: bạn còn nợ {ls.BaoNo.SoTien:N0}₫.";
                        smtp.Send(mail);
                    }

                    _db.LichSuThongBao.Add(new LichSuThongBao
                    {
                        BaoNoId = ls.BaoNoId,
                        Loai = "EMAIL",
                        NoiDung = ls.NoiDung,
                        NgayGui = DateTime.Now
                    });
                    _db.SaveChanges();

                    return Json(new { success = true, message = "Đã gửi lại Email!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi: " + ex.Message });
                }
            }
            else
            {
                // Giả lập gửi SMS lại
                try
                {
                    _db.LichSuThongBao.Add(new LichSuThongBao
                    {
                        BaoNoId = ls.BaoNoId,
                        Loai = "SMS",
                        NoiDung = ls.NoiDung,
                        NgayGui = DateTime.Now
                    });
                    _db.SaveChanges();

                    return Json(new { success = true, message = "Đã gửi lại SMS!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi: " + ex.Message });
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
