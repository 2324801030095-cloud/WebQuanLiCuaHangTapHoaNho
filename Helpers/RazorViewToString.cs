using System.IO;
using System.Web.Mvc;

namespace WebQuanLiCuaHangTapHoa.Helpers
{
    public static class RazorViewToString
    {
        public static string RenderPartialToString(ControllerContext context, string viewPath, object model)
        {
            var viewEngineResult = ViewEngines.Engines.FindPartialView(context, viewPath);
            var view = viewEngineResult.View;

            context.Controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewContext = new ViewContext(
                    context,
                    view,
                    context.Controller.ViewData,
                    context.Controller.TempData,
                    sw
                );

                view.Render(viewContext, sw);
                return sw.ToString();
            }
        }
    }
}
