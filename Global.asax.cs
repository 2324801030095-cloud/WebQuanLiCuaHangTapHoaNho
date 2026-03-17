using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebQuanLiCuaHangTapHoa
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error()
        {
            if (!Context.IsCustomErrorEnabled)
            {
                return;
            }

            Exception ex = Server.GetLastError();
            Response.Clear();

            HttpException httpEx = ex as HttpException;
            int httpCode = httpEx != null ? httpEx.GetHttpCode() : 500;

            // Log error for debugging
            try
            {
                string logPath = Server.MapPath("~/App_Data/error.log");
                System.IO.File.AppendAllText(
                    logPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {httpCode} {ex}\r\n\r\n"
                );
            }
            catch { }

            // Clear the error so we can render a friendly page
            Server.ClearError();
            Response.TrySkipIisCustomErrors = true;
            Response.StatusCode = httpCode;

            // Render error page without redirect loops
            try
            {
                Server.Execute($"/Error/Display/{httpCode}");
            }
            catch
            {
                Response.ContentType = "text/html";
                Response.Write($"<h2>Lỗi {httpCode}</h2><p>Không thể tải trang lỗi. Vui lòng thử lại sau.</p>");
            }
        }
    }
}
