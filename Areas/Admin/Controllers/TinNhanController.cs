using System;
using System.Linq;
using System.Web.Mvc;
using WebQuanLiCuaHangTapHoa.Models;
using PagedList;

namespace WebQuanLiCuaHangTapHoa.Areas.Admin.Controllers
{
    public class TinNhanController : BaseController
    {
        private readonly QuanLyTapHoaThanhNhanEntities1 _db = new QuanLyTapHoaThanhNhanEntities1();

        // ===========================================================
        // INDEX - Danh sách tin nhắn (placeholder - sẽ phát triển thêm)
        // ===========================================================
        public ActionResult Index(int? page)
        {
            // Hiện tại chưa có bảng TinNhan trong database
            // Tạo view placeholder để hiển thị thông báo
            ViewBag.Message = "Chức năng quản lý tin nhắn sẽ được phát triển thêm trong tương lai.";
            ViewBag.HasData = false;

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}

