using Hao_Hao.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Hao_Hao.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger , ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var sanpham = _context.SanPhams.ToList();
            return View(sanpham);
        }
        public IActionResult ThucPhamChucNang()
        {
            var searchKey = "Thực phẩm chức năng";

            var sp = _context.SanPhams
                .Where(sp => EF.Functions.Like(sp.MoTa, $"%{searchKey}%"))
                .ToList();

            return View(sp); // Trả về danh sách sản phẩm
        }
        public IActionResult ThietbiYte()
        {
            var searchKey = "Thiết bị y tế";

            var sp = _context.SanPhams
                .Where(sp => EF.Functions.Like(sp.MoTa, $"%{searchKey}%"))
                .ToList();

            return View(sp); // Trả về danh sách sản phẩm
        }
        public IActionResult DuocMiPham()
        {
            var searchKey = "Dược mỹ phẩm";

            var sp = _context.SanPhams
                .Where(sp => EF.Functions.Like(sp.MoTa, $"%{searchKey}%"))
                .ToList();

            return View(sp); // Trả về danh sách sản phẩm
        }


        // Đảm bảo trang yêu cầu xác thực
        public IActionResult Privacy()

        {

            // xác thực 
            // phân quyền 
            if (Request.Cookies.TryGetValue("UserInfo", out string userInfoJson))
            {

             var user =   System.Text.Json.JsonSerializer.Deserialize<UserInfo>(userInfoJson);
                ViewBag.quyen = user.UserRole;
                ViewBag.ten = user.UserId;
                ViewBag.Mail = user.UserEmail;
                var sanpham = _context.SanPhams.ToList();
                return View(sanpham);
            }
            else
            {

                // Cookie không tồn tại, có thể yêu cầu người dùng đăng nhập lại
                return RedirectToAction("Login", "Account");
            }

            
        }
        public IActionResult test()

        {


            var sanpham = _context.SanPhams.Take(3).ToList();
            return View(sanpham);

        }
        public IActionResult GioiThieu()
        {
            return View();
        }

        // API để lấy toàn bộ sản phẩm khi nhấn "Xem thêm"
        public IActionResult GetAllProducts()
        {
            var sanpham = _context.SanPhams.ToList(); // Lấy toàn bộ sản phẩm
            return Json(sanpham);
        }
        public IActionResult NotRolers()
        {
            return View();
        }

        public IActionResult Notsign() {
            return View(); }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
