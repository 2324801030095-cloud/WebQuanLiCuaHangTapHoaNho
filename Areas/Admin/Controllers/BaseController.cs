using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    // Mọi controller trong Admin đều kế thừa BaseController
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string controller = filterContext.RouteData.Values["controller"]?.ToString() ?? "";
            string action = filterContext.RouteData.Values["action"]?.ToString() ?? "";

            if (IsPublicAdminAction(controller, action))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // Nếu chưa đăng nhập Admin thì chặn truy cập
            if (Session["Admin"] == null && Session["Role"] == null)
            {
                // Lưu lại URL đang cố truy cập → để login xong quay lại
                string returnUrl = filterContext.HttpContext.Request.RawUrl;

                // Chuyển về trang đăng nhập chung
                filterContext.Result =
                    new RedirectResult("/TaiKhoan/DangNhap?returnUrl=" + returnUrl);

                return;
            }

            string role = GetAdminRole();
            if (string.IsNullOrWhiteSpace(role))
            {
                filterContext.Result = new RedirectResult("/Error/Display/403");
                return;
            }

            if (string.Equals(role, "QuanTri", StringComparison.OrdinalIgnoreCase))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            if (string.Equals(role, "NhanVien", StringComparison.OrdinalIgnoreCase))
            {
                if (IsAdminOnlyController(controller))
                {
                    filterContext.Result = new RedirectResult("/Error/Display/403");
                    return;
                }

                if (!string.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                {
                    filterContext.Result = new RedirectResult("/Error/Display/403");
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }

        private string GetAdminRole()
        {
            if (Session["Role"] != null)
            {
                return Session["Role"] as string;
            }

            var admin = Session["Admin"];
            if (admin == null)
            {
                return null;
            }

            try
            {
                var prop = admin.GetType().GetProperty("Quyen");
                return prop?.GetValue(admin, null) as string;
            }
            catch
            {
                return null;
            }
        }

        private bool IsPublicAdminAction(string controller, string action)
        {
            if (!string.Equals(controller, "AdminAuth", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return string.Equals(action, "Login", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "Register", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "ForgotPassword", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "Logout", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsAdminOnlyController(string controller)
        {
            var adminOnly = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "NhanVien",
                "TaiKhoanKH",
                "ThuChi",
                "Luong",
                "BaoNo",
                "PhieuNhap"
            };

            return adminOnly.Contains(controller);
        }
    }
}
