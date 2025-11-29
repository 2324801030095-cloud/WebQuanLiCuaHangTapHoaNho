using System.Web.Mvc;

namespace WebQuanLiCuaHangTapHoa.Controllers
{
    public class AccountController : Controller
    {
        // ✅ Giả lập login thành công
        public ActionResult LoginSuccess()
        {
            Session["UserName"] = "Admin";  // hoặc lấy từ DB
            return RedirectToAction("Index", "Admin", new { area = "Admin" });
        }

        // ✅ Đăng xuất
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
