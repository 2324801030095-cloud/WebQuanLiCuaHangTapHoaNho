using System.Web.Optimization;                  // Dùng hệ thống Bundling/Minification

namespace WebQuanLiCuaHangTapHoa
{
    public class BundleConfig
    {
        // Hàm đăng ký tất cả bundle của dự án, được gọi ở Global.asax
        public static void RegisterBundles(BundleCollection bundles)
        {
            // ---------------- BUNDLE MẶC ĐỊNH (thư viện) ----------------

            // jQuery core (dùng {version} để tự khớp phiên bản file trong /Scripts)
            bundles.Add(
                new ScriptBundle("~/bundles/jquery")
                    .Include("~/Scripts/jquery-{version}.js")           // jQuery chính
            );

            // jQuery Validate (client-side validation)
            bundles.Add(
                new ScriptBundle("~/bundles/jqueryval")
                    .Include("~/Scripts/jquery.validate*")              // validate + unobtrusive
            );

            // Modernizr (chỉ dùng bản dev khi phát triển; build custom khi production)
            bundles.Add(
                new ScriptBundle("~/bundles/modernizr")
                    .Include("~/Scripts/modernizr-*")                   // modernizr
            );

            // Bootstrap JS (nếu bạn có file bootstrap.js trong /Scripts)
            // Lưu ý: nếu bạn đang dùng Bootstrap từ CDN trong layout thì KHÔNG cần bundle này.
            bundles.Add(
                new ScriptBundle("~/bundles/bootstrap")
                    .Include("~/Scripts/bootstrap.js")                  // Bootstrap JS local (tuỳ dự án)
            );

            // CSS mặc định (nếu bạn còn dùng bootstrap.css và site.css local)
            // Lưu ý: nếu Bootstrap đang dùng CDN, có thể bỏ gói này.
            bundles.Add(
                new StyleBundle("~/Content/css")
                    .Include(
                        "~/Content/bootstrap.css",                      // Bootstrap CSS local (tuỳ dự án)
                        "~/Content/site.css"                            // CSS site mặc định của template MVC
                    )
            );

            // ---------------- BUNDLE RIÊNG CỦA DỰ ÁN (ThanhNhan) ----------------

            // CSS riêng của app (chỉ file của bạn, KHÔNG gộp Bootstrap/Swiper từ CDN)
            bundles.Add(
                new StyleBundle("~/content/app")
                    .Include("~/Content/ThanhNhan/Layout.css")          // CSS layout của bạn
            );

            // JS riêng của app (chỉ file của bạn, KHÔNG gộp Bootstrap/Swiper từ CDN)
            bundles.Add(
                new ScriptBundle("~/bundles/app")
                    .Include("~/Scripts/ThanhNhan/Layout.js")           // JS layout của bạn
            );

#if DEBUG
            // Dev: tắt tối ưu hoá (để gỡ lỗi dễ, không minify/concat)
            BundleTable.EnableOptimizations = false;                    // Không nén/gộp khi Debug
#else
            // Release: bật tối ưu hoá (minify + concat)
            BundleTable.EnableOptimizations = true;                     // Nén/gộp khi Release
#endif
        }
    }
}
