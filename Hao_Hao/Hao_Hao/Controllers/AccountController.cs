using Hao_Hao.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;


namespace Hao_Hao.Controllers
{
    
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();  // Khởi tạo PasswordHasher
        }



        
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {

            if (ModelState.IsValid)
            {
                // Kiểm tra xem email đã tồn tại trong hệ thống chưa
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email đã được đăng ký.");
                    return View(model);
                }

                // Tạo người dùng mới từ RegisterViewModel
                var user = new User
                {
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    Gender = model.Gender,
                    // Mã hóa mật khẩu trước khi lưu vào DB
                    Password = _passwordHasher.HashPassword(null, model.Password) // Mã hóa mật khẩu
                };

                // Lưu người dùng vào bảng Users
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Tìm vai trò "user" trong bảng Roles
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");

                if (role != null)
                {
                    // Tạo một đối tượng UserRole để liên kết người dùng với vai trò
                    var userRole = new UserRole
                    {
                        UserId = user.Id, // ID người dùng
                        RoleId = role.Id  // ID vai trò
                    };

                    // Lưu thông tin vào bảng UserRoles
                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                }

                // Chuyển hướng đến trang đăng ký xác nhận
                return RedirectToAction("Registercomfirm");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel()); // Đảm bảo model không null khi render view
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null)
                {
                    var result = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);

                    if (result == PasswordVerificationResult.Success)
                    {


                        var roles = _context.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Select(ur => _context.Roles.FirstOrDefault(r => r.Id == ur.RoleId).Name)
                    .ToList();

                        HttpContext.Session.SetString("UserEmail", user.Email);
                        HttpContext.Session.SetString("UserId", user.Id.ToString());
                        HttpContext.Session.SetString("UserRole", string.Join(",", roles)); // Lưu vai trò vào Session



                        var userInfo = new UserInfo
                        {
                            UserId = user.Id,
                            UserEmail = user.Email,
                            UserRole = roles
                        };

                        // Chuyển đổi thành chuỗi JSON
                        var userInfoJson = System.Text.Json.JsonSerializer.Serialize(userInfo);

                        // Lưu vào cookie (tên cookie , nội dung)
                        Response.Cookies.Append("UserInfo", userInfoJson, new CookieOptions
                        {
                            Expires = DateTime.UtcNow.AddDays(7), // Cookie hết hạn sau 7 ngày
                            HttpOnly = true, // Chỉ có thể truy cập cookie qua HTTP, không thể đọc từ JavaScript
                            IsEssential = true, // Cookie sẽ luôn được lưu ngay cả khi người dùng từ chối cookie không thiết yếu
                            Secure = true, // Chỉ gửi cookie qua HTTPS
                            SameSite = SameSiteMode.Lax // Cookie được gửi cùng request GET từ trang khác
                        });

                        string roleNames = string.Join(", ", roles);
                        if (roleNames.Equals("admin"))
                        {
                           return  RedirectToAction("Home", "Admin");
                        }

                        // Kiểm tra xem người dùng đã đăng nhập chưa
                        return RedirectToAction("Index", "Home"); // Chuyển hướng đến trang khác


                    }
                    else
                    {
                        return Unauthorized(new { Message = "Sai thông tin đăng nhập" });
                    }
                }
                return Unauthorized(new { Message = "Sai thông tin đăng nhập" });
            }
            return BadRequest(new { Message = "Sai thông tin đăng nhập " });
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = HttpContext.Session.GetString("UserId");

           
            return View();

        }

        // đảng xuất

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear(); // Xóa toàn bộ Session
            foreach (var cookie in Request.Cookies.Keys)
            {
                // Xóa từng cookie
                Response.Cookies.Delete(cookie);
            }
            return RedirectToAction("Login", "Account");  // Chuyển hướng về trang đăng nhập
        }


        // Trang xác nhận đăng ký thành công
        public IActionResult Registercomfirm()
        {
            return View();
        }



        [HttpGet]
        public IActionResult Forgot()
        {
            // Khởi tạo ViewModel nếu cần thiết
            var model = new ForgotPasswordViewModel();
            return View(model); // Truyền model vào View
        }

        // Xử lý gửi mã xác nhận
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Forgot(ForgotPasswordViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                ModelState.AddModelError("Email", "Email không được để trống.");
                return View(model); // Đảm bảo truyền model lại vào View khi có lỗi
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("Email", "Email không tồn tại trong hệ thống.");
                return View(model); // Đảm bảo truyền model lại vào View khi có lỗi
            }

            // Tạo mã xác nhận ngẫu nhiên
            string verificationCode = new Random().Next(100000, 999999).ToString();

            // Lưu mã vào session hoặc cơ sở dữ liệu để xác minh
            TempData["VerificationCode"] = verificationCode;
            TempData["UserEmail"] = user.Email;

            // Gửi mã qua email
            string emailError = SendVerificationEmail(user.Email, verificationCode);

            if (emailError == null)
            {
                return RedirectToAction("VerifyCode");
            }

            // Nếu có lỗi khi gửi email, thêm lỗi vào ModelState để hiển thị trong View
            ModelState.AddModelError("", emailError);
            return View(model); // Đảm bảo truyền model lại vào View khi có lỗi
        }



        private string SendVerificationEmail(string email, string verificationCode)
        {
            try
            {
                var fromAddress = new MailAddress("nhatx5xxx@gmail.com", "HaoHao");
                var toAddress = new MailAddress(email);
                const string subject = "Mã xác nhận thay đổi mật khẩu";
                string body = $"Mã xác nhận của bạn là: {verificationCode}";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com", // SMTP server của Gmail
                    Port = 587, // Cổng SMTP của Gmail (587 cho TLS)
                    EnableSsl = true, // Bật SSL/TLS vnaz eifb tqdm yrhh
                    Credentials = new NetworkCredential(fromAddress.Address, "rbod nmes mdme vrgg"),
                    Timeout = 90000
                };


                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }

                return null; // Gửi thành công, không có lỗi
            }
            catch (SmtpException smtpEx)
            {
                // Trả về thông báo lỗi khi có lỗi SMTP
                return "Lỗi SMTP: " + smtpEx.Message;
            }
            catch (Exception ex)
            {
                // Trả về thông báo lỗi khi có lỗi khác
                return "Lỗi khi gửi email: " + ex.Message;
            }
        }




        // Trang nhập mã xác nhận
        [HttpGet]
        public IActionResult VerifyCode()
        {
            return View();
        }

        // Xử lý xác nhận mã và thay đổi mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(string verificationCode, string newPassword)
        {
            var savedCode = TempData["VerificationCode"]?.ToString();
            if (savedCode != verificationCode)
            {
                ModelState.AddModelError("", "Mã xác nhận không chính xác.");
                return View(); // Trả về lại View với lỗi
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                ModelState.AddModelError("NewPassword", "Mật khẩu không được để trống.");
                return View(); // Trả về lại View nếu mật khẩu mới rỗng
            }

            var userEmail = TempData["UserEmail"]?.ToString();
            if (string.IsNullOrEmpty(userEmail))
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin người dùng.");
                return View(); // Trả về lại View nếu không có email người dùng
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user != null)
            {
                user.Password = _passwordHasher.HashPassword((User)user, newPassword); // Mã hóa mật khẩu mới
                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Mật khẩu đã được thay đổi thành công.";
                return RedirectToAction("Login"); // Chuyển hướng đến trang đăng nhập
            }

            ModelState.AddModelError("", "Không tìm thấy người dùng.");
            return View(); // Nếu không tìm thấy người dùng, quay lại View
        }



    }
}
