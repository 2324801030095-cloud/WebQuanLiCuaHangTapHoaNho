// ==============================
// ⚙️ Nạp thư viện cần thiết
// ==============================
using QRCoder;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models; // DbContext + CartItem

namespace WebQuanLiCuaHangTapHoa.Controllers
{
    public class GioHangController : Controller
    {
        // ==============================
        // 🧩 1. Khai báo DbContext
        // ==============================
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // ==============================
        // 🧩 2. Tên khóa lưu giỏ trong Session
        // ==============================
        private const string CART_KEY = "CART";

        // ==============================
        // ⚙️ 3. Hàm tiện ích: Lấy giỏ từ Session
        // ==============================
        private List<CartItem> LayGioHang()
        {
            var cart = Session[CART_KEY] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session[CART_KEY] = cart;
            }
            return cart;
        }

        // ======================================================
        // 🧺 1️⃣ Trang xem giỏ hàng
        // ======================================================
        public ActionResult Index()
        {
            var cart = LayGioHang();
            ViewBag.TongSoLuong = cart.Sum(x => x.SoLuong);
            ViewBag.TongTien = cart.Sum(x => x.ThanhTien);
            return View(cart);
        }

        // ======================================================
        // ➕ 2️⃣ Thêm sản phẩm vào giỏ
        // ======================================================
        [HttpGet]
        public ActionResult Them(int maSP, string returnUrl)
        {
            var sp = _db.SanPham.SingleOrDefault(x => x.MaSP == maSP && x.HoatDong == true);
            if (sp == null)
                return RedirectToAction("Index", "Home");

            var cart = LayGioHang();
            var item = cart.SingleOrDefault(x => x.MaSP == maSP);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    DonGia = sp.GiaBan,
                    SoLuong = 1,
                    HinhAnh = sp.HinhAnh
                });
            }
            else
            {
                item.SoLuong++;
            }

            Session[CART_KEY] = cart;

            // Nếu AJAX -> trả JSON
            if (Request.IsAjaxRequest())
                return Json(new { ok = true, tong = cart.Sum(x => x.SoLuong) }, JsonRequestBehavior.AllowGet);

            // Nếu có returnUrl thì quay lại
            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        // ======================================================
        // ✏️ 3️⃣ Cập nhật số lượng
        // ======================================================
        [HttpPost]
        public ActionResult CapNhat(int maSP, int soLuong)
        {
            var cart = LayGioHang();
            var item = cart.SingleOrDefault(x => x.MaSP == maSP);
            if (item != null)
                item.SoLuong = soLuong > 0 ? soLuong : 1;

            Session[CART_KEY] = cart;
            return RedirectToAction("Index");
        }

        // ======================================================
        // ❌ 4️⃣ Xóa 1 sản phẩm
        // ======================================================
        public ActionResult Xoa(int maSP)
        {
            var cart = LayGioHang();
            var item = cart.SingleOrDefault(x => x.MaSP == maSP);
            if (item != null)
                cart.Remove(item);

            Session[CART_KEY] = cart;
            return RedirectToAction("Index");
        }

        // ======================================================
        // 🧹 5️⃣ Xóa toàn bộ giỏ hàng
        // ======================================================
        public ActionResult XoaTatCa()
        {
            Session.Remove(CART_KEY);
            return RedirectToAction("Index");
        }

        // ======================================================
        // 🧾 6️⃣ Form đặt hàng
        // ======================================================
        [HttpGet]
        public ActionResult DatHang()
        {
            var cart = LayGioHang();
            if (!cart.Any()) return RedirectToAction("Index", "Home");

            ViewBag.TongSoLuong = cart.Sum(x => x.SoLuong);
            ViewBag.TongTien = cart.Sum(x => x.ThanhTien);
            return View(cart);
        }

        // ======================================================
        // 💾 7️⃣ Lưu đơn hàng vào CSDL
        // ======================================================
        [HttpPost]
        public ActionResult DatHang(string tenKH, string sdt, string diaChi)
        {
            var cart = LayGioHang();
            if (!cart.Any())
                return RedirectToAction("Index");

            // --- Khách hàng ---
            var kh = _db.KhachHang.SingleOrDefault(x => x.SoDT == sdt);
            if (kh == null)
            {
                kh = new KhachHang
                {
                    TenKH = tenKH,
                    SoDT = sdt,
                    DiaChi = diaChi
                };
                _db.KhachHang.Add(kh);
                _db.SaveChanges();
            }

            // --- Hóa đơn ---
            var hd = new HoaDon
            {
                Ngay = DateTime.Now,
                MaKH = kh.MaKH,
                MaNV = 1 // Nhân viên demo
            };
            _db.HoaDon.Add(hd);
            _db.SaveChanges();

            // --- Chi tiết hóa đơn ---
            foreach (var item in cart)
            {
                var ct = new ChiTietHoaDon
                {
                    MaHD = hd.MaHD,
                    MaSP = item.MaSP,
                    SoLuong = item.SoLuong,
                    DonGia = item.DonGia
                };
                _db.ChiTietHoaDon.Add(ct);

                // Giảm tồn kho
                var kho = _db.Kho.SingleOrDefault(k => k.MaSP == item.MaSP);
                if (kho != null)
                    kho.Ton -= item.SoLuong;
            }
            _db.SaveChanges();

            // --- Dọn giỏ sau khi lưu ---
            Session.Remove(CART_KEY);   // ✅ Xóa session để giỏ trống hoàn toàn

            // --- Chuyển đến trang thành công ---
            return RedirectToAction("ThanhCong", new { id = hd.MaHD });
        }

        // ======================================================
        // ✅ 8️⃣ Trang xác nhận đơn hàng
        // ======================================================
        public ActionResult ThanhCong(int id)
        {
            ViewBag.MaHD = id;
            return View();
        }

        // ======================================================
        // 🔢 9️⃣ Đếm tổng số sản phẩm (AJAX cho header)
        // ======================================================
        public JsonResult DemSoLuong()
        {
            var cart = Session[CART_KEY] as List<CartItem>;
            var tong = cart?.Sum(x => x.SoLuong) ?? 0;
            return Json(new { tong }, JsonRequestBehavior.AllowGet);
        }

        // ======================================================
        // 🔟 10️⃣ PartialView giỏ hàng
        // ======================================================
        public PartialViewResult PartialCart()
        {
            var cart = LayGioHang();
            return PartialView("_CartOffcanvasPartial", cart);
        }

        // ======================================================
        // 🔚 11️⃣ Giải phóng DbContext
        // ======================================================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();
            base.Dispose(disposing);
        }

        [HttpGet]
        public ActionResult ThanhToan()
        {
            var cart = LayGioHang();
            if (!cart.Any())
                return RedirectToAction("Index");

            ViewBag.TongTien = cart.Sum(x => x.ThanhTien);
            return View(cart);
        }

        [HttpGet]
        public FileContentResult QrCode(decimal amount)
        {
            string bank = "Techcombank";
            string account = "3398329567";
            string name = "Thanh Nhan";

            string payload = $"{bank}|{account}|{name}|Amount:{amount}";

            using (QRCodeGenerator qr = new QRCodeGenerator())
            using (QRCodeData data = qr.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode png = new PngByteQRCode(data))
            {
                byte[] image = png.GetGraphic(20);
                return File(image, "image/png");
            }
        }

    }
}
