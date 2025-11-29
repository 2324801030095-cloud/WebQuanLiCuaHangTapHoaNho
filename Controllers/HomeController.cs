// Nạp namespace cần dùng
using System.Linq;                                                // Cho LINQ: Where, OrderBy, Take
using System.Web.Mvc;                                             // Cho Controller, ActionResult, PartialView
using WebQuanLiCuaHangTapHoa.Models;                              // Cho EDMX context + SanPhamView (ViewModel)

namespace WebQuanLiCuaHangTapHoa.Controllers                      // Namespace phải khớp project
{
    public class HomeController : Controller                       // Controller trang chủ
    {
        // Khởi tạo DbContext từ EDMX (Tên theo EDMX của bạn)
        private readonly QuanLyTapHoaThanhNhanEntities1 _db
            = new QuanLyTapHoaThanhNhanEntities1();               // Dùng connectionString trong Web.config

        // =======================
        // Action: Index (Trang chủ)
        // URL: /Home/Index?kw=&maDM=
        // =======================
        public ActionResult Index(string kw = null, int? maDM = null)
        {
            // Lưu lại filter vào ViewBag để hiển thị lại trên UI (ô search, breadcrumb…)
            ViewBag.Keyword = kw;                                  // Từ khóa đang tìm
            ViewBag.MaDM = maDM;                                   // Danh mục đang lọc

            // Không lấy dữ liệu nặng ở đây -> chỉ return view khung
            // Phần danh sách sản phẩm sẽ nạp qua Partial (đúng phong cách Lab chia nhỏ)
            return View();                                         // Trả về Index.cshtml (không model)
        }

        // ==========================================
        // ChildAction: NewProducts (khối “DB mới nhất”)
        // Gọi trong Index qua Html.Action
        // ==========================================
        [ChildActionOnly]                                          // Chỉ gọi nội bộ từ View
        public ActionResult NewProducts(string kw = null, int? maDM = null, int take = 12)
        {
            // Tạo query base: chỉ sp đang hoạt động
            var query =
                from sp in _db.SanPham                              // Từ bảng SanPham
                where sp.HoatDong == true                           // Chỉ lấy SP hoạt động
                select new SanPhamView                              // Map về ViewModel nhẹ
                {
                    MaSP = sp.MaSP,                                 // Mã sản phẩm
                    TenSP = sp.TenSP,                               // Tên
                    GiaBan = sp.GiaBan,                             // Giá
                    Ton = _db.Kho                                   // Lấy tồn kho
                          .Where(k => k.MaSP == sp.MaSP)
                          .Select(k => (int?)k.Ton)                 // chọn nullable để FirstOrDefault không lỗi
                          .FirstOrDefault() ?? 0,                   // nếu null -> 0
                    MaDM = sp.MaDM                                  // Mã danh mục (phục vụ link)
                };

            // Lọc theo danh mục nếu có
            if (maDM.HasValue)                                      // Nếu có maDM trên URL
                query = query.Where(x => x.MaDM == maDM.Value);     // Thì lọc đúng danh mục

            // Lọc theo từ khóa nếu có (tìm theo tên)
            if (!string.IsNullOrWhiteSpace(kw))                     // Nếu có từ khóa
                query = query.Where(x => x.TenSP.Contains(kw));     // Lọc tên chứa từ khóa

            // Lấy mới nhất theo MaSP (ID tăng dần), giới hạn take
            var ds = query
                     .OrderByDescending(x => x.MaSP)               // Mới nhất trước
                     .Take(take)                                   // Lấy N sp (mặc định 12)
                     .ToList();                                    // Thực thi query

            // Trả Partial lưới sản phẩm, model là danh sách ViewModel
            return PartialView("_ProductGridPartial", ds);          // Dùng lại partial tái sử dụng
        }

        // ==========================================
        // ChildAction: TopSelling (khối bán chạy – demo)
        // Sau này bạn thay bằng thống kê thật (Lab sau)
        // ==========================================
        [ChildActionOnly]
        public ActionResult TopSelling(int take = 9)                // Cho phép cấu hình số lượng
        {
            // Demo: tái sử dụng NewProducts (vì chưa có chỉ số doanh số)
            // Bạn có thể thay bằng join CTHD/HoaDon để tính bán chạy theo kỳ
            var ds = (from sp in _db.SanPham
                      where sp.HoatDong == true
                      orderby sp.MaSP descending                    // tạm xem “mới” như “hot”
                      select new SanPhamView
                      {
                          MaSP = sp.MaSP,
                          TenSP = sp.TenSP,
                          GiaBan = sp.GiaBan,
                          Ton = _db.Kho.Where(k => k.MaSP == sp.MaSP)
                                       .Select(k => (int?)k.Ton)
                                       .FirstOrDefault() ?? 0,
                          MaDM = sp.MaDM
                      })
                      .Take(take)
                      .ToList();

            return PartialView("_ProductGridPartial", ds);          // Render bằng lưới chung
        }

        // Giải phóng DbContext chuẩn
        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();                           // Hủy context khi controller dispose
            base.Dispose(disposing);                                 // Gọi dispose lớp cha
        }

        // =================== KHÁCH HÀNG XEM SẢN PHẨM KHUYẾN MÃI ===================
        // ✅ ACTION MỚI — TRANG KHUYẾN MÃI
        public ActionResult KhuyenMai()
        {
            // Lọc sản phẩm có khuyến mãi = true
            var ds = _db.SanPham
            .Where(sp => sp.KhuyenMai != null)
            .ToList();


            ViewBag.Title = "Sản phẩm khuyến mãi";
            return View(ds);
        }
        public ActionResult GioiThieu()
        {
            return View();   // tìm file Views/Home/GioiThieu.cshtml
        }

    }
}
