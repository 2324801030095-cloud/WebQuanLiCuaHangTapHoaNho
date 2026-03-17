using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;

namespace WebQuanLiCuaHangTapHoa.Controllers
{
    public class ThongBaoController : Controller
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // GET: ThongBao
        public ActionResult Index()
        {
            return View();
        }

        // Child action that returns a partial list of notifications for current user
        [ChildActionOnly]
        public ActionResult GetNotifications(int take = 10)
        {
            try
            {
                var kh = Session["KH"];
                int? maKH = null;
                if (kh != null)
                {
                    var prop = kh.GetType().GetProperty("MaKH");
                    if (prop != null) maKH = prop.GetValue(kh, null) as int?;
                }

                var now = DateTime.Now;

                // Query ThongBao with simple rules: DaGui and within date range and target matches
                var q = _db.ThongBao.Where(t => t.TrangThai == "DaGui" && (t.NgayGui == null || t.NgayGui <= now)
                            && (t.NgayHetHan == null || t.NgayHetHan >= now));

                q = q.OrderByDescending(t => t.NgayGui).Take(take);

                var list = new List<NotificationVm>();
                foreach (var t in q)
                {
                    var isForAll = string.Equals(t.DoiTuong, "TatCa", StringComparison.OrdinalIgnoreCase);
                    var isForCustomer = string.Equals(t.DoiTuong, "KhachHang", StringComparison.OrdinalIgnoreCase);

                    bool include = false;
                    if (isForAll) include = true;
                    else if (isForCustomer && maKH == null) include = true; // general customer
                    else if (t.MaKH == null) include = true;
                    else if (maKH != null && t.MaKH == maKH) include = true;

                    if (!include) continue;

                    list.Add(new NotificationVm
                    {
                        MaThongBao = t.MaThongBao,
                        TieuDe = t.TieuDe,
                        NoiDung = t.NoiDung,
                        Icon = t.Icon,
                        NgayGui = t.NgayGui,
                        Link = t.Link,
                        IsRead = false // will update later if you track read state
                    });
                }

                ViewBag.NotificationCount = list.Count(n => !n.IsRead);
                return PartialView("_NotificationList", list);
            }
            catch (Exception ex)
            {
                // log if needed
                try { System.Diagnostics.Trace.TraceError("GetNotifications error: " + ex); } catch { }
                return PartialView("_NotificationList", new List<NotificationVm>());
            }
        }

        // API to mark all notifications as read for current user
        [HttpPost]
        public JsonResult MarkAllRead()
        {
            if (Session == null || Session["KH"] == null) return Json(new { ok = false });

            var kh = Session["KH"];
            var prop = kh.GetType().GetProperty("MaKH");
            if (prop == null) return Json(new { ok = false });

            int maKH = (int)prop.GetValue(kh, null);

            try
            {
                // Create LichSuXemThongBao records for all ThongBao for this user that don't have one
                var now = DateTime.Now;
                var toMark = _db.ThongBao.Where(t => t.TrangThai == "DaGui").ToList();
                foreach (var t in toMark)
                {
                    bool exists = _db.LichSuXemThongBao.Any(l => l.MaThongBao == t.MaThongBao && l.MaKH == maKH);
                    if (!exists)
                    {
                        _db.LichSuXemThongBao.Add(new LichSuXemThongBao
                        {
                            MaThongBao = t.MaThongBao,
                            MaKH = maKH,
                            NgayXem = now
                        });
                    }
                }
                _db.SaveChanges();

                var unread = 0; // now zero
                return Json(new { ok = true, unread });
            }
            catch
            {
                return Json(new { ok = false });
            }
        }
    }
}