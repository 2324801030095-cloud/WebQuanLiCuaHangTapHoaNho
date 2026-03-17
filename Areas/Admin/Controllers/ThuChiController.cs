using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using WebQuanLiCuaHangTapHoa.Models;
using Rotativa;
using Rotativa.MVC;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class ThuChiController : BaseController
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db =
            new QuanLyTapHoaThanhNhanEntities1();

        // ================================================================
        // 1) DASHBOARD THU – CHI – LỢI NHUẬN
        // ================================================================
        public ActionResult Index()
        {
            DateTime today = DateTime.Today;
            DateTime weekStart = today.AddDays(-(int)today.DayOfWeek + 1);
            DateTime monthStart = new DateTime(today.Year, today.Month, 1);
            DateTime yearStart = new DateTime(today.Year, 1, 1);

            // ==== KPI: THU – CHI – LỢI NHUẬN ====
            ViewBag.ThuNgay = GetSum("Thu", today, today);
            ViewBag.ChiNgay = GetSum("Chi", today, today);
            ViewBag.LNNgay = ViewBag.ThuNgay - ViewBag.ChiNgay;

            ViewBag.ThuTuan = GetSum("Thu", weekStart, today);
            ViewBag.ChiTuan = GetSum("Chi", weekStart, today);
            ViewBag.LNTuan = ViewBag.ThuTuan - ViewBag.ChiTuan;

            ViewBag.ThuThang = GetSum("Thu", monthStart, today);
            ViewBag.ChiThang = GetSum("Chi", monthStart, today);
            ViewBag.LNThang = ViewBag.ThuThang - ViewBag.ChiThang;

            return View();
        }

        // Hàm tính tổng theo ngày + loại
        private int GetSum(string loai, DateTime start, DateTime end)
        {
            DateTime startDate = start.Date;
            DateTime endDate = end.Date;
            return _db.ThuChi
                .Where(tc => tc.Loai == loai &&
                             DbFunctions.TruncateTime(tc.Ngay) >= startDate &&
                             DbFunctions.TruncateTime(tc.Ngay) <= endDate)
                .Select(tc => (int?)tc.SoTien)
                .Sum() ?? 0;
        }

        // ================================================================
        // 2) API BIỂU ĐỒ: THU – CHI (ApexCharts)
        // ================================================================
        public JsonResult GetDataChart(string range)
        {
            DateTime today = DateTime.Today;
            DateTime start = GetStartDate(range, today);
            DateTime startDate = start.Date;
            DateTime endDate = today.Date;

            var data = _db.ThuChi
                .Where(tc => DbFunctions.TruncateTime(tc.Ngay) >= startDate &&
                             DbFunctions.TruncateTime(tc.Ngay) <= endDate)
                .GroupBy(tc => DbFunctions.TruncateTime(tc.Ngay))
                .Select(g => new
                {
                    Ngay = g.Key.Value,
                    Thu = g.Where(x => x.Loai == "Thu").Sum(x => (int?)x.SoTien) ?? 0,
                    Chi = g.Where(x => x.Loai == "Chi").Sum(x => (int?)x.SoTien) ?? 0
                })
                .OrderBy(x => x.Ngay)
                .ToList();

            var map = data.ToDictionary(x => x.Ngay.Date, x => x);
            var labels = new List<string>();
            var thuValues = new List<int>();
            var chiValues = new List<int>();
            for (var d = startDate; d <= endDate; d = d.AddDays(1))
            {
                labels.Add(d.ToString("dd/MM"));
                if (map.TryGetValue(d.Date, out var row))
                {
                    thuValues.Add(row.Thu);
                    chiValues.Add(row.Chi);
                }
                else
                {
                    thuValues.Add(0);
                    chiValues.Add(0);
                }
            }

            return Json(new
            {
                labels = labels,
                thu = thuValues,
                chi = chiValues
            }, JsonRequestBehavior.AllowGet);
        }

        // ================================================================
        // 3) API BIỂU ĐỒ LỢI NHUẬN (ECharts)
        // ================================================================
        public JsonResult GetProfitChart(string range)
        {
            DateTime today = DateTime.Today;
            DateTime start = GetStartDate(range, today);
            DateTime startDate = start.Date;
            DateTime endDate = today.Date;

            var data = _db.ThuChi
                .Where(tc => DbFunctions.TruncateTime(tc.Ngay) >= startDate &&
                             DbFunctions.TruncateTime(tc.Ngay) <= endDate)
                .GroupBy(tc => DbFunctions.TruncateTime(tc.Ngay))
                .Select(g => new
                {
                    Ngay = g.Key.Value,
                    LoiNhuan = (
                        (g.Where(x => x.Loai == "Thu").Sum(x => (int?)x.SoTien) ?? 0)
                        -
                        (g.Where(x => x.Loai == "Chi").Sum(x => (int?)x.SoTien) ?? 0)
                    )
                })
                .OrderBy(x => x.Ngay)
                .ToList();

            var map = data.ToDictionary(x => x.Ngay.Date, x => x.LoiNhuan);
            var labels = new List<string>();
            var values = new List<int>();
            for (var d = startDate; d <= endDate; d = d.AddDays(1))
            {
                labels.Add(d.ToString("dd/MM"));
                values.Add(map.TryGetValue(d.Date, out var v) ? v : 0);
            }

            return Json(new
            {
                labels = labels,
                values = values,
            }, JsonRequestBehavior.AllowGet);
        }

        private DateTime GetStartDate(string range, DateTime today)
        {
            switch (range)
            {
                case "today": return today;
                case "week": return today.AddDays(-(int)today.DayOfWeek + 1);
                case "month": return new DateTime(today.Year, today.Month, 1);
                case "year": return new DateTime(today.Year, 1, 1);
                default: return DateTime.MinValue;
            }
        }

        // ================================================================
        // 4) TAB DANH SÁCH THU – CHI
        // ================================================================
        public ActionResult DanhSach(int? page, DateTime? from, DateTime? to, string loai, int? money)
        {
            int pageNumber = page ?? 1;
            int pageSize = 12;

            var query = _db.ThuChi.AsQueryable();

            if (from.HasValue)
            {
                DateTime fromDate = from.Value.Date;
                query = query.Where(tc => DbFunctions.TruncateTime(tc.Ngay) >= fromDate);
                ViewBag.From = fromDate.ToString("yyyy-MM-dd");
            }
            else
            {
                ViewBag.From = "";
            }

            if (to.HasValue)
            {
                DateTime toDate = to.Value.Date;
                query = query.Where(tc => DbFunctions.TruncateTime(tc.Ngay) <= toDate);
                ViewBag.To = toDate.ToString("yyyy-MM-dd");
            }
            else
            {
                ViewBag.To = "";
            }

            if (!string.IsNullOrEmpty(loai))
            {
                query = query.Where(tc => tc.Loai == loai);
            }

            if (money.HasValue)
            {
                query = query.Where(tc => tc.SoTien >= money.Value);
            }

            ViewBag.Loai = loai;
            ViewBag.Money = money;

            var list = query
                .OrderByDescending(tc => tc.Ngay)
                .ThenByDescending(tc => tc.MaTC)
                .ToList();

            return PartialView("_DanhSachThuChi", list.ToPagedList(pageNumber, pageSize));
        }

        // ================================================================
        // 5) THÊM
        // ================================================================
        [HttpGet]
        public ActionResult Them()
        {
            ViewBag.Loai = new SelectList(new[] { "Thu", "Chi" });
            return PartialView("_ThemThuChi");
        }

        [HttpPost]
        public JsonResult Them(ThuChi model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.Loai))
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });

                if (model.SoTien < 0)
                    return Json(new { success = false, message = "Số tiền phải >= 0!" });

                _db.ThuChi.Add(model);
                _db.SaveChanges();

                return Json(new { success = true, message = "Đã thêm mới!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ================================================================
        // 6) SỬA
        // ================================================================
        [HttpGet]
        public ActionResult Sua(int id)
        {
            var item = _db.ThuChi.Find(id);
            if (item == null) return HttpNotFound();

            ViewBag.Loai = new SelectList(new[] { "Thu", "Chi" }, item.Loai);
            return PartialView("_SuaThuChi", item);
        }

        [HttpPost]
        public JsonResult Sua(ThuChi model)
        {
            try
            {
                var old = _db.ThuChi.Find(model.MaTC);
                if (old == null)
                    return Json(new { success = false, message = "Không tìm thấy bản ghi!" });

                _db.Entry(old).CurrentValues.SetValues(model);
                _db.SaveChanges();

                return Json(new { success = true, message = "Đã cập nhật!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ================================================================
        // 7) CHI TIẾT
        // ================================================================
        public ActionResult ChiTiet(int id)
        {
            var item = _db.ThuChi.Find(id);
            if (item == null) return HttpNotFound();

            return PartialView("_ChiTietThuChi", item);
        }

        // ================================================================
        // 8) XÓA
        // ================================================================
        [HttpPost]
        public JsonResult Xoa(int id)
        {
            try
            {
                var item = _db.ThuChi.Find(id);
                if (item == null)
                    return Json(new { success = false, message = "Không tìm thấy bản ghi!" });

                _db.ThuChi.Remove(item);
                _db.SaveChanges();

                return Json(new { success = true, message = "Đã xóa!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        public ActionResult PDF(string range)
        {
            DateTime today = DateTime.Today;
            DateTime start = GetStartDate(range, today);

            var list = _db.ThuChi
                .Where(tc => tc.Ngay >= start && tc.Ngay <= today)
                .OrderBy(tc => tc.Ngay)
                .ToList();

            return View(list);
        }

        public ActionResult ExportPDF(string range)
        {
            return new Rotativa.ActionAsPdf("PDF", new { range })
            {
                FileName = $"BaoCaoThuChi_{range}_{DateTime.Now:yyyyMMdd}.pdf",
                CustomSwitches = "--page-size A4 --orientation Portrait --margin-top 10mm --margin-bottom 10mm"
            };
        }

        public JsonResult GetKPI(string range)
        {
            DateTime today = DateTime.Today;
            DateTime start = GetStartDate(range, today);
            DateTime startDate = start.Date;
            DateTime endDate = today.Date;

            int thu = _db.ThuChi
                .Where(x => x.Loai == "Thu" &&
                            DbFunctions.TruncateTime(x.Ngay) >= startDate &&
                            DbFunctions.TruncateTime(x.Ngay) <= endDate)
                .Select(x => (int?)x.SoTien).Sum() ?? 0;

            int chi = _db.ThuChi
                .Where(x => x.Loai == "Chi" &&
                            DbFunctions.TruncateTime(x.Ngay) >= startDate &&
                            DbFunctions.TruncateTime(x.Ngay) <= endDate)
                .Select(x => (int?)x.SoTien).Sum() ?? 0;

            int loinhuan = thu - chi;

            return Json(new
            {
                thu,
                chi,
                loinhuan
            }, JsonRequestBehavior.AllowGet);
        }


    }
}
