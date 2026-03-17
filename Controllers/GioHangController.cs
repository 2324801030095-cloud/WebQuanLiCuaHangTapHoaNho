using QRCoder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;

namespace WebQuanLiCuaHangTapHoa.Controllers
{
    public class GioHangController : Controller
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();
        private const string CART_KEY = "CART";

        #region GIỎ HÀNG - CORE

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

        public ActionResult Index()
        {
            var cart = LayGioHang();
            ViewBag.TongSoLuong = cart.Sum(x => x.SoLuong);
            ViewBag.TongTien = cart.Sum(x => x.ThanhTien);
            return View(cart);
        }

        #endregion

        #region THÊM SẢN PHẨM

        [HttpGet]
        public ActionResult Them(int maSP, string returnUrl)
        {
            var sp = _db.SanPham.SingleOrDefault(x => x.MaSP == maSP && x.HoatDong == true);
            if (sp == null) return RedirectToAction("Index", "Home");

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
            else item.SoLuong++;

            Session[CART_KEY] = cart;

            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    ok = true,
                    count = cart.Sum(x => x.SoLuong),
                    total = cart.Sum(x => x.ThanhTien)
                }, JsonRequestBehavior.AllowGet);
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public JsonResult ThemAjax(int maSP, int soLuong = 1)
        {
            var sp = _db.SanPham.SingleOrDefault(x => x.MaSP == maSP && x.HoatDong == true);
            if (sp == null)
                return Json(new { ok = false, message = "Sản phẩm không tồn tại!" }, JsonRequestBehavior.AllowGet);

            var cart = LayGioHang();
            var item = cart.SingleOrDefault(x => x.MaSP == maSP);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    DonGia = sp.GiaBan,
                    SoLuong = soLuong,
                    HinhAnh = sp.HinhAnh
                });
            }
            else item.SoLuong += soLuong;

            Session[CART_KEY] = cart;

            return Json(new
            {
                ok = true,
                count = cart.Sum(x => x.SoLuong),
                total = cart.Sum(x => x.ThanhTien),
                message = "Đã thêm vào giỏ!"
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region CẬP NHẬT - XOÁ

        [HttpPost]
        public ActionResult CapNhat(int maSP, int soLuong)
        {
            var cart = LayGioHang();
            var item = cart.SingleOrDefault(x => x.MaSP == maSP);

            if (item != null)
                item.SoLuong = soLuong > 0 ? soLuong : 1;

            Session[CART_KEY] = cart;

            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    ok = true,
                    count = cart.Sum(x => x.SoLuong),
                    total = cart.Sum(x => x.ThanhTien),
                    itemSubtotal = item.ThanhTien
                }, JsonRequestBehavior.AllowGet);
            }

            return RedirectToAction("Index");
        }

        public JsonResult Xoa(int maSP)
        {
            var cart = LayGioHang();
            var item = cart.SingleOrDefault(x => x.MaSP == maSP);
            if (item != null) cart.Remove(item);

            Session[CART_KEY] = cart;

            return Json(new
            {
                ok = true,
                count = cart.Sum(x => x.SoLuong),
                total = cart.Sum(x => x.ThanhTien)
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult XoaTatCa()
        {
            Session.Remove(CART_KEY);
            return Json(new { ok = true, count = 0, total = 0 }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region BADGE GIỎ HÀNG

        [HttpGet]
        public JsonResult DemSoLuong()
        {
            var cart = LayGioHang();
            return Json(new
            {
                tong = cart?.Sum(x => x.SoLuong) ?? 0,
                total = cart?.Sum(x => x.ThanhTien) ?? 0
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region THANH TOÁN

        [HttpGet]
        public ActionResult ThanhToan()
        {
            var cart = LayGioHang();
            if (!cart.Any()) return RedirectToAction("Index", "Home");

            ViewBag.TongSoLuong = cart.Sum(x => x.SoLuong);
            ViewBag.TongTien = cart.Sum(x => x.ThanhTien);

            return View("~/Views/GioHang/ThanhToan.cshtml", cart);
        }

        #endregion

        #region ĐẶT HÀNG (COD – BANK – MOMO)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DatHang()
        {
            var cart = LayGioHang();
            if (!cart.Any()) return RedirectToAction("Index");

            dynamic kh = Session["KH"];
            if (kh == null) return RedirectToAction("DangNhap", "TaiKhoan");

            int maKH = kh.MaKH;
            var hd = new HoaDon
            {
                Ngay = DateTime.Now,
                MaKH = maKH,
                MaNV = 1
            };

            _db.HoaDon.Add(hd);
            _db.SaveChanges();

            decimal tongTien = 0;

            foreach (var item in cart)
            {
                tongTien += item.ThanhTien;

                _db.ChiTietHoaDon.Add(new ChiTietHoaDon
                {
                    MaHD = hd.MaHD,
                    MaSP = item.MaSP,
                    SoLuong = item.SoLuong,
                    DonGia = item.DonGia
                });

                var kho = _db.Kho.SingleOrDefault(k => k.MaSP == item.MaSP);
                if (kho != null) kho.Ton -= item.SoLuong;
            }

            _db.SaveChanges();

            string type = Request.Form["PaymentMethod"] ?? "cod";
            string status = "Đang xử lý";

            if (type == "bank" || type == "momo") status = "Đã thanh toán";
            if (type == "cod") status = "Đợi lấy hàng";

            _db.PhuongThucThanhToan.Add(new PhuongThucThanhToan
            {
                MaHD = hd.MaHD,
                MaLoaiThanhToan = type,
                TrangThai = status,
                NgayTao = DateTime.Now
            });

            _db.SaveChanges();

            if (type == "bank" || type == "momo")
            {
                _db.ThuChi.Add(new ThuChi
                {
                    Ngay = DateTime.Now,
                    Loai = "Thu",
                    SoTien = (int)tongTien,
                    DienGiai = "Doanh thu hóa đơn HD" + hd.MaHD
                });

                _db.SaveChanges();
            }

            Session.Remove(CART_KEY);

            // ✅ Truyền MaHD để ThanhCong có thể truy vấn dữ liệu chính xác
            return RedirectToAction("ThanhCong", new { id = hd.MaHD });
        }

        #endregion

        #region TRANG THÀNH CÔNG (Fix Review)

        public ActionResult ThanhCong(int id)
        {
            ViewBag.MaHD = id;

            // ✅ FIX: Query ChiTietHoaDon and map to strongly-typed ViewModel
            var spList = _db.ChiTietHoaDon
                .Where(c => c.MaHD == id)
                .Include("SanPham")
                .ToList()
                .Select(c => new WebQuanLiCuaHangTapHoa.Models.ViewModels.OrderProductVM
                {
                    MaSP = c.MaSP,
                    TenSP = c.SanPham?.TenSP ?? "Sản phẩm",
                    HinhAnh = c.SanPham?.HinhAnh ?? "no-image.jpg",
                    SoLuong = c.SoLuong,
                    DonGia = (decimal)c.DonGia,
                    ThanhTien = (decimal)(c.SoLuong * c.DonGia)
                })
                .ToList();

            // ✅ THÊM: Lưu danh sách sản phẩm vào Session với key dựa trên MaHD
            Session["OrderProducts_" + id] = spList;
            Session.Timeout = 60; // Timeout 1 giờ cho an toàn
            
            // ✅ THÊM: Lưu vào ViewBag để view có thể dùng ngay
            ViewBag.SanPhamList = spList;
            ViewBag.TongSanPham = spList.Count;
            ViewBag.TongTien = spList.Sum(x => x.DonGia * x.SoLuong);

            return View("~/Views/GioHang/DatHangThanhCong.cshtml");
        }

        #endregion

        #region VNPay (Tạo URL + Confirm)

        public ActionResult PaymentVNPay()
        {
            var cart = LayGioHang();
            if (cart.Count == 0) return RedirectToAction("Index");

            decimal amount = cart.Sum(x => x.ThanhTien);

            string url = ConfigurationManager.AppSettings["Url"];
            string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"]
                ?? ConfigurationManager.AppSettings["Returnurl"];
            string tmnCode = ConfigurationManager.AppSettings["TmnCode"];
            string hashSecret = ConfigurationManager.AppSettings["HashSecret"];

            PayLib pay = new PayLib();
            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", tmnCode);
            pay.AddRequestData("vnp_Amount", ((long)amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress());
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang");
            pay.AddRequestData("vnp_ReturnUrl", returnUrl);
            pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString());

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);

            return Redirect(paymentUrl);
        }

        public ActionResult PaymentConfirm()
        {
            if (Request.QueryString.Count == 0)
            {
                ViewBag.Message = "Không có dữ liệu trả về!";
                return View("~/Views/GioHang/PaymentConfirm.cshtml");
            }

            string hashSecret = ConfigurationManager.AppSettings["HashSecret"];
            PayLib pay = new PayLib();

            foreach (string s in Request.QueryString)
            {
                if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                {
                    pay.AddResponseData(s, Request.QueryString[s]);
                }
            }

            string rspCode = pay.GetResponseData("vnp_ResponseCode");
            string secureHash = Request.QueryString["vnp_SecureHash"];

            bool valid = pay.ValidateSignature(secureHash, hashSecret);

            if (!valid)
            {
                ViewBag.Message = "Sai chữ ký - giao dịch không hợp lệ!";
                return View("~/Views/GioHang/PaymentConfirm.cshtml");
            }

            if (rspCode == "00")
            {
                SaveOrderForVNPay();
                ViewBag.Message = "Thanh toán thành công!";
            }
            else
            {
                ViewBag.Message = "Thanh toán thất bại hoặc bị hủy!";
            }

            return View("~/Views/GioHang/PaymentConfirm.cshtml");
        }

        #endregion

        #region LƯU ĐƠN HÀNG CHO VNPAY

        private void SaveOrderForVNPay()
        {
            var cart = LayGioHang();
            if (cart.Count == 0) return;

            dynamic kh = Session["KH"];
            if (kh == null) return;

            int maKH = kh.MaKH;

            var hd = new HoaDon
            {
                Ngay = DateTime.Now,
                MaKH = maKH,
                MaNV = 1
            };

            _db.HoaDon.Add(hd);
            _db.SaveChanges();

            decimal tong = 0;

            foreach (var item in cart)
            {
                tong += item.ThanhTien;

                _db.ChiTietHoaDon.Add(new ChiTietHoaDon
                {
                    MaHD = hd.MaHD,
                    MaSP = item.MaSP,
                    SoLuong = item.SoLuong,
                    DonGia = item.DonGia
                });

                var kho = _db.Kho.SingleOrDefault(k => k.MaSP == item.MaSP);
                if (kho != null) kho.Ton -= item.SoLuong;
            }

            _db.SaveChanges();

            _db.PhuongThucThanhToan.Add(new PhuongThucThanhToan
            {
                MaHD = hd.MaHD,
                MaLoaiThanhToan = "vnpay",
                TrangThai = "Đã thanh toán",
                NgayTao = DateTime.Now
            });
            _db.SaveChanges();

            _db.ThuChi.Add(new ThuChi
            {
                Ngay = DateTime.Now,
                Loai = "Thu",
                SoTien = (int)tong,
                DienGiai = "Doanh thu hóa đơn HD" + hd.MaHD
            });
            _db.SaveChanges();

            Session.Remove(CART_KEY);
        }

        #endregion

        #region QR MOMO

        [HttpGet]
        public FileContentResult QrCodeMomo(decimal amount)
        {
            string phone = "0398329567";
            string name = "NGUYEN QUANG LONG";
            string payload = $"2|99|{phone}|{name}|0|0|{amount}|ThanhToanHoaDon";

            using (QRCodeGenerator qr = new QRCodeGenerator())
            using (QRCodeData data = qr.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode png = new PngByteQRCode(data))
            {
                return File(png.GetGraphic(20), "image/png");
            }
        }

        #endregion

        #region OFFCANVAS CART

        public PartialViewResult PartialCart()
        {
            return PartialView("_CartOffcanvasPartial", LayGioHang());
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
