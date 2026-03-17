using System.Web.Mvc;

namespace WebQuanLiCuaHangTapHoa.Controllers
{
    // Base cho trang mua hàng
    // Chỉ dùng để chặn các Action cần đăng nhập
    public class BaseLoginController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Nếu chưa đăng nhập khách hàng
            if (Session["KH"] == null)
            {
                string returnUrl = filterContext.HttpContext.Request.RawUrl;

                filterContext.Result = new RedirectResult(
                    "/TaiKhoan/DangNhap?returnUrl=" + returnUrl
                );
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
