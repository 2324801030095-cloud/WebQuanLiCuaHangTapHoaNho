// dùng MVC (Controller, ActionExecutingContext)
using System.Web.Mvc;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    // BaseController: mọi controller trong Area Admin sẽ kế thừa class này
    // Mục tiêu: chặn truy cập khi CHƯA đăng nhập (chưa có Session["UserName"])
    public class BaseController : Controller
    {
        // Hàm chạy trước mỗi Action trong Admin
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Kiểm tra session Admin
            if (Session["Admin"] == null)
            {
                // Chuyển hướng về trang đăng nhập admin
                filterContext.Result = new RedirectResult("/Admin/AdminAuth/Login");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
