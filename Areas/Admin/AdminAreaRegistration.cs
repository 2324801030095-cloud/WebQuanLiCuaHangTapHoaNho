using System.Web.Mvc;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "Admin"; }  // 👈 Tên phải đúng với tên folder Area
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            // 🔹 Định nghĩa route riêng cho khu vực Admin
            context.MapRoute(
                name: "Admin_default",
                url: "Admin/{controller}/{action}/{id}",
                defaults: new { controller = "Admin", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers" }
            );
        }
    }
}
