using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using Rotativa;
using Rotativa.MVC;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class ThuChiController : Controller
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
            return _db.ThuChi
                .Where(tc => tc.Loai == loai &&
                             tc.Ngay >= start &&
                             tc.Ngay <= end)
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

            var data = _db.ThuChi
                .Where(tc => tc.Ngay >= start && tc.Ngay <= today)
                .GroupBy(tc => tc.Ngay)
                .Select(g => new
                {
                    Ngay = g.Key,
                    Thu = g.Where(x => x.Loai == "Thu").Sum(x => (int?)x.SoTien) ?? 0,
                    Chi = g.Where(x => x.Loai == "Chi").Sum(x => (int?)x.SoTien) ?? 0
                })
                .OrderBy(x => x.Ngay)
                .ToList();

            return Json(new
            {
                labels = data.Select(x => x.Ngay.ToString("dd/MM")).ToList(),
                thu = data.Select(x => x.Thu).ToList(),
                chi = data.Select(x => x.Chi).ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        // ================================================================
        // 3) API BIỂU ĐỒ LỢI NHUẬN (ECharts)
        // ================================================================
        public JsonResult GetProfitChart(string range)
        {
            DateTime today = DateTime.Today;
            DateTime start = GetStartDate(range, today);

            var data = _db.ThuChi
                .Where(tc => tc.Ngay >= start && tc.Ngay <= today)
                .GroupBy(tc => tc.Ngay)
                .Select(g => new
                {
                    Ngay = g.Key,
                    LoiNhuan = (
                        (g.Where(x => x.Loai == "Thu").Sum(x => (int?)x.SoTien) ?? 0)
                        -
                        (g.Where(x => x.Loai == "Chi").Sum(x => (int?)x.SoTien) ?? 0)
                    )
                })
                .OrderBy(x => x.Ngay)
                .ToList();

            return Json(new
            {
                labels = data.Select(x => x.Ngay.ToString("dd/MM")).ToList(),
                values = data.Select(x => x.LoiNhuan).ToList(),
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
        public ActionResult DanhSach(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 12;

            var list = _db.ThuChi
                .OrderBy(tc => tc.Ngay)
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

            int thu = _db.ThuChi
                .Where(x => x.Loai == "Thu" && x.Ngay >= start && x.Ngay <= today)
                .Select(x => (int?)x.SoTien).Sum() ?? 0;

            int chi = _db.ThuChi
                .Where(x => x.Loai == "Chi" && x.Ngay >= start && x.Ngay <= today)
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
