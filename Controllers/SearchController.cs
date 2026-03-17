using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using WebQuanLiCuaHangTapHoa.Models.ViewModels;

namespace WebQuanLiCuaHangTapHoa.Controllers
{
    public class SearchController : Controller
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db =
            new QuanLyTapHoaThanhNhanEntities1();

        // ============================================================
        // 1) FULL PAGE SEARCH (SearchIndex)
        // ============================================================
        public ActionResult Search(
            string kw,
            int page = 1,
            int pageSize = 24,
            string sort = "default",
            bool? inStock = null,
            bool? discount = null)
        {
            kw = (kw ?? "").Trim();

            // Không có từ khóa → trả list rỗng
            if (string.IsNullOrEmpty(kw))
            {
                ViewBag.Keyword = "";
                ViewBag.SearchResult = new List<SanPhamView>();
                return View("SearchIndex");
            }

            // BASE QUERY an toàn, không Reflection
            var query =
                from sp in _db.SanPham
                join k in _db.Kho on sp.MaSP equals k.MaSP
                where sp.HoatDong == true
                select new
                {
                    sp.MaSP,
                    sp.TenSP,
                    sp.GiaBan,
                    Ton = k.Ton,
                    sp.MaDM,
                    sp.HinhAnh,
                    GiaGoc = (decimal?)null // KHÔNG dùng reflection
                };

            // Theo từ khóa
            if (!string.IsNullOrEmpty(kw))
                query = query.Where(x => x.TenSP.Contains(kw));

            // Lọc tồn kho
            if (inStock == true)
                query = query.Where(x => x.Ton > 0);

            // Lọc giảm giá (vì không có GiaGoc → bỏ qua)
            if (discount == true)
            {
                // Chỉ kích hoạt nếu sau này có GiaGoc
                query = query.Where(x => x.GiaGoc != null && x.GiaGoc > x.GiaBan);
            }

            // SORTING
            switch (sort)
            {
                case "priceAsc":
                    query = query.OrderBy(x => x.GiaBan);
                    break;
                case "priceDesc":
                    query = query.OrderByDescending(x => x.GiaBan);
                    break;
                case "nameAsc":
                    query = query.OrderBy(x => x.TenSP);
                    break;
                case "nameDesc":
                    query = query.OrderByDescending(x => x.TenSP);
                    break;
                default:
                    query = query.OrderBy(x => x.TenSP);
                    break;
            }

            // PAGING
            int total = query.Count();
            int skip = (page - 1) * pageSize;

            var items = query
                .Skip(skip)
                .Take(pageSize)
                .AsEnumerable()
                .Select(x => new SanPhamView
                {
                    MaSP = x.MaSP,
                    TenSP = x.TenSP,
                    GiaBan = x.GiaBan,
                    Ton = x.Ton,
                    MaDM = x.MaDM,
                    HinhAnh = x.HinhAnh
                })
                .ToList();

            // ----------- Gửi sang View ----------------
            ViewBag.Keyword = kw;
            ViewBag.TotalItems = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Sort = sort;
            ViewBag.InStock = inStock;
            ViewBag.Discount = discount;

            ViewBag.SearchResult = items;
            return View("SearchIndex");
        }

        // ============================================================
        // 2) PARTIAL SEARCH (header search hiển thị ngay trong Index)
        // ============================================================
        public ActionResult SearchResultsPartial(
            string kw,
            int page = 1,
            int pageSize = 12,
            string sort = "default",
            bool? inStock = null,
            bool? discount = null)
        {
            kw = (kw ?? "").Trim();

            if (string.IsNullOrEmpty(kw))
                return PartialView("_SearchResult", new List<SanPhamView>());

            var query =
                from sp in _db.SanPham
                join k in _db.Kho on sp.MaSP equals k.MaSP
                where sp.HoatDong == true
                select new
                {
                    sp.MaSP,
                    sp.TenSP,
                    sp.GiaBan,
                    Ton = k.Ton,
                    sp.MaDM,
                    sp.HinhAnh,
                    GiaGoc = (decimal?)null
                };

            if (!string.IsNullOrEmpty(kw))
                query = query.Where(x => x.TenSP.Contains(kw));

            if (inStock == true)
                query = query.Where(x => x.Ton > 0);

            if (discount == true)
                query = query.Where(x => x.GiaGoc != null && x.GiaGoc > x.GiaBan);

            switch (sort)
            {
                case "priceAsc":
                    query = query.OrderBy(x => x.GiaBan);
                    break;
                case "priceDesc":
                    query = query.OrderByDescending(x => x.GiaBan);
                    break;
                case "nameAsc":
                    query = query.OrderBy(x => x.TenSP);
                    break;
                case "nameDesc":
                    query = query.OrderByDescending(x => x.TenSP);
                    break;
                default:
                    query = query.OrderBy(x => x.TenSP);
                    break;
            }

            int skip = (page - 1) * pageSize;

            var items = query
                .Skip(skip)
                .Take(pageSize)
                .AsEnumerable()
                .Select(x => new SanPhamView
                {
                    MaSP = x.MaSP,
                    TenSP = x.TenSP,
                    GiaBan = x.GiaBan,
                    Ton = x.Ton,
                    MaDM = x.MaDM,
                    HinhAnh = x.HinhAnh
                })
                .ToList();

            return PartialView("_SearchResult", items);
        }

        // ============================================================
        // 3) AUTOCOMPLETE JSON (header gợi ý)
        // ============================================================
        [HttpGet]
        public JsonResult SearchAjax(string q, int take = 8)
        {
            q = (q ?? "").Trim();

            if (string.IsNullOrEmpty(q))
                return Json(new { items = new object[0] }, JsonRequestBehavior.AllowGet);

            var items = (from sp in _db.SanPham
                         join k in _db.Kho on sp.MaSP equals k.MaSP
                         where sp.HoatDong == true &&
                               sp.TenSP.Contains(q)
                         orderby sp.TenSP
                         select new
                         {
                             id = sp.MaSP,
                             name = sp.TenSP,
                             price = sp.GiaBan,
                             inStock = k.Ton,
                             img = sp.HinhAnh
                         })
                        .Take(take)
                        .ToList()
                        .Select(x => new
                        {
                            id = x.id,
                            name = x.name,
                            price = x.price,
                            inStock = x.inStock,
                            img = Url.Content(
                                "~/Content/Images/" +
                                (string.IsNullOrWhiteSpace(x.img)
                                    ? "no-image.png"
                                    : x.img.Trim())
                            )
                        });

            return Json(new { items }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
