// using cần cho MVC
using System.Web;                 // HttpApplication
using System.Web.Mvc;            // MVC filter/areas
using System.Web.Optimization;   // Bundling
using System.Web.Routing;        // Routes

namespace WebQuanLiCuaHangTapHoa   // ★ PHẢI TRÙNG Global.asax
{
    public class MvcApplication : HttpApplication
    {
        // Chạy 1 lần khi app khởi động
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();                          // Đăng ký Areas (nếu có)
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);    // Filter toàn cục
            RouteConfig.RegisterRoutes(RouteTable.Routes);                // Routes
            BundleConfig.RegisterBundles(BundleTable.Bundles);            // Bundles
        }
    }
}
