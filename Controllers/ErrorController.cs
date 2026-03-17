using System;
using System.IO;
using System.Web.Mvc;

namespace WebQuanLiCuaHangTapHoa.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        // GET: /Error/{code}
        public ActionResult Display(int? code)
        {
            try
            {
                int status = code ?? 500;

                // sanitize
                if (status < 400 || status > 599) status = 500;

                string viewPath = $"~/Views/Shared/Error/Error_{status}.cshtml";
                string fallback = "~/Views/Shared/Error/ErrorTemplate.cshtml";

                string physicalPath = Server.MapPath(viewPath);
                if (!string.IsNullOrEmpty(physicalPath) && System.IO.File.Exists(physicalPath))
                {
                    Response.StatusCode = status;
                    return View(viewPath);
                }

                // fallback to generic template if specific view missing
                physicalPath = Server.MapPath(fallback);
                if (!string.IsNullOrEmpty(physicalPath) && System.IO.File.Exists(physicalPath))
                {
                    Response.StatusCode = status;
                    ViewBag.StatusCode = status;
                    return View(fallback);
                }

                // ultimate fallback: plain content
                Response.StatusCode = status;
                return Content($"Error {status}");
            }
            catch (Exception ex)
            {
                // In case something goes wrong while rendering the error page, avoid throwing again
                // log the exception if logging is available
                try
                {
                    // use System.Diagnostics to avoid adding new dependencies
                    System.Diagnostics.Trace.TraceError($"ErrorController.Display error: {ex}");
                }
                catch
                {
                    // swallow any logging failure
                }

                Response.StatusCode = 500;
                return View("~/Views/Shared/Error/ErrorTemplate.cshtml");
            }
        }
    }
}
