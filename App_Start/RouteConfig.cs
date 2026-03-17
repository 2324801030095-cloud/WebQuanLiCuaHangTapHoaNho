using System.Web.Mvc;
using System.Web.Routing;

namespace WebQuanLiCuaHangTapHoa
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("Content/{*pathInfo}");
            routes.IgnoreRoute("Scripts/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");

            // ============================
            // ✔ 1) Error Pages
            // ============================
            routes.MapRoute(
                name: "ErrorPages",
                url: "Error/{code}",
                defaults: new { controller = "Error", action = "Display", code = UrlParameter.Optional },
                namespaces: new[] { "WebQuanLiCuaHangTapHoa.Controllers" }
            );

            // ============================
            // ✔ 2) Route Chi Tiết Kiến Thức
            // ============================
            routes.MapRoute(
                name: "KienThucChiTiet",
                url: "kien-thuc/{slug}",
                defaults: new { controller = "KienThuc", action = "ChiTiet", slug = UrlParameter.Optional },
                namespaces: new[] { "WebQuanLiCuaHangTapHoa.Controllers" }
            );

            // ============================
            // ✔ 3) Route mặc định
            // ============================
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "WebQuanLiCuaHangTapHoa.Controllers" }
            );
        }
    }
}
