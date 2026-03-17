using System;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using WebQuanLiCuaHangTapHoa.Models;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class LuongController : BaseController
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        private string SalaryLogPath => Server.MapPath("~/App_Data/salary_history.log");

        private void WriteSalaryLog(int maNV, int oldSalary, int newSalary, string user)
        {
            try
            {
                var nv = _db.NhanVien.Find(maNV);
                var name = nv != null ? nv.TenNV : "-";
                var line = string.Format("{0:O}|{1}|{2}|{3}|{4}|{5}", DateTime.UtcNow, maNV, name, oldSalary, newSalary, user ?? "Admin");
                var dir = Path.GetDirectoryName(SalaryLogPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                System.IO.File.AppendAllText(SalaryLogPath, line + Environment.NewLine, Encoding.UTF8);
            }
            catch
            {
                // logging must not break main flow
            }
        }

        // INDEX: list employees + current salary with filters and paging
        public ActionResult Index(string keyword, string chucVu, int? minSalary, int? maxSalary, string sort, int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 15;

            var q = _db.NhanVien.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                q = q.Where(x => x.TenNV.Contains(keyword) || x.MaNV.ToString().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(chucVu))
            {
                q = q.Where(x => x.ChucVu == chucVu);
            }

            if (minSalary.HasValue)
            {
                q = q.Where(x => x.MucLuong.HasValue && x.MucLuong.Value >= minSalary.Value);
            }

            if (maxSalary.HasValue)
            {
                q = q.Where(x => x.MucLuong.HasValue && x.MucLuong.Value <= maxSalary.Value);
            }

            // Apply sorting
            switch (sort)
            {
                case "salary_desc":
                    q = q.OrderByDescending(x => x.MucLuong ?? 0);
                    break;
                case "salary_asc":
                    q = q.OrderBy(x => x.MucLuong ?? 0);
                    break;
                case "ma_desc":
                    q = q.OrderByDescending(x => x.MaNV);
                    break;
                case "ma_asc":
                    q = q.OrderBy(x => x.MaNV);
                    break;
                case "ten_az":
                    q = q.OrderBy(x => x.TenNV);
                    break;
                case "ten_za":
                    q = q.OrderByDescending(x => x.TenNV);
                    break;
                default:
                    q = q.OrderBy(x => x.MaNV);
                    break;
            }

            // Populate filter lists for view (provide plain List<string> to simplify view logic)
            var chucVuList = _db.NhanVien
                .Where(n => !string.IsNullOrEmpty(n.ChucVu))
                .Select(n => n.ChucVu.Trim())
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            ViewBag.ChucVuList = chucVuList;
            ViewBag.Keyword = keyword;
            ViewBag.ChucVu = chucVu;
            ViewBag.MinSalary = minSalary;
            ViewBag.MaxSalary = maxSalary;
            ViewBag.Sort = sort;

            return View(q.ToPagedList(pageNumber, pageSize));
        }

        // GET: load edit salary modal
        [HttpGet]
        public ActionResult Sua(int id)
        {
            var nv = _db.NhanVien.Find(id);
            if (nv == null) return HttpNotFound();
            return PartialView("_SuaLuong", nv);
        }

        // POST: update salary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Sua(int MaNV, int MucLuong)
        {
            try
            {
                if (MucLuong < 0)
                    return Json(new { success = false, message = "Lương phải lớn hơn hoặc bằng 0." });

                var nv = _db.NhanVien.Find(MaNV);
                if (nv == null) return Json(new { success = false, message = "Không tìm thấy nhân viên." });

                var old = nv.MucLuong ?? 0;
                nv.MucLuong = MucLuong;
                _db.SaveChanges();

                // log change
                var user = (Session["UserName"] ?? "Admin").ToString();
                WriteSalaryLog(MaNV, old, MucLuong, user);

                return Json(new { success = true, message = "Cập nhật lương thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // Salary history view (reads from App_Data log)
        public ActionResult History(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 25;

            var list = new List<WebQuanLiCuaHangTapHoa.Areas.Admin.Models.SalaryLogEntry>();
            try
            {
                if (System.IO.File.Exists(SalaryLogPath))
                {
                    var lines = System.IO.File.ReadAllLines(SalaryLogPath, Encoding.UTF8);
                    // show newest first
                    foreach (var line in lines.Reverse().Take(500))
                    {
                        var parts = line.Split('|');
                        if (parts.Length >= 6)
                        {
                            DateTime when;
                            DateTime.TryParse(parts[0], out when);
                            int maNV = int.TryParse(parts[1], out var t1) ? t1 : 0;
                            var name = parts[2];
                            int oldS = int.TryParse(parts[3], out var t2) ? t2 : 0;
                            int newS = int.TryParse(parts[4], out var t3) ? t3 : 0;
                            var by = parts[5];
                            list.Add(new WebQuanLiCuaHangTapHoa.Areas.Admin.Models.SalaryLogEntry { When = when, MaNV = maNV, TenNV = name, OldSalary = oldS, NewSalary = newS, ChangedBy = by });
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }

            return View(list.ToPagedList(pageNumber, pageSize));
        }

        // Bulk update salaries by role
        [HttpGet]
        public ActionResult BulkUpdate()
        {
            var chucVuList = _db.NhanVien
                .Where(n => !string.IsNullOrEmpty(n.ChucVu))
                .Select(n => n.ChucVu)
                .Distinct()
                .OrderBy(s => s)
                .ToList();
            ViewBag.ChucVuList = new SelectList(chucVuList);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult BulkUpdate(string chucVu, string mode, int value, bool applyToAll = false)
        {
            try
            {
                // sanitize inputs
                mode = (mode ?? "").Trim().ToLowerInvariant();
                if (mode != "percent" && mode != "amount")
                    return Json(new { success = false, message = "Chế độ không hợp lệ." });

                if (Math.Abs(value) > 1000000000)
                    return Json(new { success = false, message = "Giá trị quá lớn." });

                var q = _db.NhanVien.AsQueryable();
                if (!applyToAll && !string.IsNullOrWhiteSpace(chucVu))
                {
                    chucVu = chucVu.Trim();
                    q = q.Where(x => x.ChucVu == chucVu);
                }

                var list = q.ToList();
                if (!list.Any()) return Json(new { success = false, message = "Không có nhân viên nào để cập nhật." });

                var user = (Session["UserName"] ?? "Admin").ToString();
                foreach (var nv in list)
                {
                    var old = nv.MucLuong ?? 0;
                    long newsal = old;
                    if (mode == "percent")
                    {
                        newsal = old + (long)Math.Round(old * (value / 100.0));
                    }
                    else
                    {
                        // amount
                        newsal = old + value;
                    }

                    // clamp to int range
                    if (newsal < int.MinValue) newsal = int.MinValue;
                    if (newsal > int.MaxValue) newsal = int.MaxValue;

                    nv.MucLuong = (int)newsal;
                    WriteSalaryLog(nv.MaNV, old, nv.MucLuong ?? 0, user);
                }
                _db.SaveChanges();
                return Json(new { success = true, message = $"Đã cập nhật {list.Count} nhân viên." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}