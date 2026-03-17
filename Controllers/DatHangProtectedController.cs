using System.Collections.Generic;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using System.Linq;

namespace WebQuanLiCuaHangTapHoa.Controllers
{
    public class DatHangProtectedController : BaseLoginController
    {
        private const string CART_KEY = "CART";

        private List<CartItem> LayGioHang()
        {
            return Session[CART_KEY] as List<CartItem> ?? new List<CartItem>();
        }

        // ================================
        // ⭐ FORM ĐẶT HÀNG – BẮT BUỘC LOGIN
        // ================================
        [HttpGet]
        public ActionResult DatHang()
        {
            var cart = LayGioHang();
            if (!cart.Any()) return RedirectToAction("Index", "GioHang");

            ViewBag.TongSoLuong = cart.Sum(x => x.SoLuong);
            ViewBag.TongTien = cart.Sum(x => x.ThanhTien);

            return View("~/Views/GioHang/DatHang.cshtml", cart);
        }

        // ================================
        // ⭐ LƯU ĐƠN HÀNG – BẮT BUỘC LOGIN
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DatHangSubmit()
        {
            // Chuyển request sang Action DatHang trong GioHangController
            return RedirectToAction("DatHang", "GioHang");
        }
    }
}
