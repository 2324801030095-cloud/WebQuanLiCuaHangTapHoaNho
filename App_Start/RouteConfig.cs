using System.Web.Mvc;     // Cho MVC
using System.Web.Routing; // Cho RouteCollection

namespace WebQuanLiCuaHangTapHoa
{
    public class RouteConfig
    {
        // Hàm đăng ký route, gọi ở Global.asax Application_Start
        public static void RegisterRoutes(RouteCollection routes)
        {
            // Bỏ qua các route mặc định không cần thiết
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("Content/{*pathInfo}");
            routes.IgnoreRoute("Scripts/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");

            // 🔹 Route mặc định cho khu vực người dùng (User)
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },

                // 👇 Thêm namespace này để chỉ rõ route này dành cho controller ngoài Area
                namespaces: new[] { "WebQuanLiCuaHangTapHoa.Controllers" }
            );
        }
    }
}
