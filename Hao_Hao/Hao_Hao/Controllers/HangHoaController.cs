using Hao_Hao.Attributes;
using Hao_Hao.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QRCoder;

namespace Hao_Hao.Controllers
{
    public class HangHoaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HangHoaController> _logger;


        public HangHoaController(ApplicationDbContext context, ILogger<HangHoaController> logger)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult them()
        {
            return View();
        }
        [AuthorizeRolesAttribute("admin", "User", "Employee")]
        [HttpPost]
        public IActionResult ThemGioHang(ThemHangHoaCotroler hanghoa)
        {
            var userEmail = "";

            if (Request.Cookies.TryGetValue("UserInfo", out string userInfoJson))
            {

                var user1 = System.Text.Json.JsonSerializer.Deserialize<UserInfo>(userInfoJson);

                userEmail = user1.UserEmail;


            }
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra xem giỏ hàng của người dùng đã tồn tại chưa
            var gioHang = _context.GioHang.FirstOrDefault(g => g.UserId == user.Id);
            if (gioHang == null)
            {
                // Nếu chưa có, tạo giỏ hàng mới
                gioHang = new GioHang { UserId = user.Id };
                _context.GioHang.Add(gioHang);
                _context.SaveChanges();
            }

            // Kiểm tra sản phẩm có trong giỏ hàng chưa
            var chiTietGioHang = _context.ChiTietGioHang
                .FirstOrDefault(ct => ct.GioHangId == gioHang.Id && ct.ProductId == hanghoa.MaHang);

            if (chiTietGioHang != null)
            {
                // Nếu sản phẩm đã tồn tại, cập nhật số lượng
                chiTietGioHang.Quantity += hanghoa.soLuong;
            }
            else
            {
                // Nếu chưa có, thêm sản phẩm vào giỏ hàng
                var sanPham = _context.SanPhams.Find(hanghoa.MaHang);
                if (sanPham == null)
                {
                    return NotFound();
                }

                chiTietGioHang = new ChiTietGioHang
                {
                    GioHangId = gioHang.Id,
                    ProductId = hanghoa.MaHang,
                    Quantity = hanghoa.soLuong,
                    Price = sanPham.Gia
                };
                _context.ChiTietGioHang.Add(chiTietGioHang);
            }

            _context.SaveChanges();
            // hiển thị trên alert là thêm thanh công
            // Lưu thông báo vào TempData để hiển thị alert
            TempData["Message"] = "Thêm vào giỏ hàng thành công!";
            return Redirect(Request.Headers["Referer"].ToString()); // Quay lại trang trước đó


        }
        [AuthorizeRolesAttribute("admin", "User", "Employee")]
        public IActionResult XemGioHang()
        {
            // Kiểm tra xem có cookie UserInfo không
            if (Request.Cookies.TryGetValue("UserInfo", out string userInfoJson))
            {
                try
                {
                    // Deserialize JSON từ cookie
                    var user = JsonConvert.DeserializeObject<UserInfo>(userInfoJson);

                    // Kiểm tra xem user có hợp lệ không
                    if (user == null || user.UserId <= 0)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    int idUser = user.UserId;  // Không cần int.Parse()

                    // Lấy giỏ hàng của người dùng
                    var gioHang = _context.ChiTietGioHang
                        .Include(ct => ct.SanPham)
                        .Include(ct => ct.GioHang)
                        .Where(ct => ct.GioHang.UserId == idUser)
                        .Select(ct => new GioHangViewModel
                        {
                            GioHangId = ct.GioHang.Id,
                            SanPhamId = ct.SanPham.Id,
                            TenSanPham = ct.SanPham.TenSanPham,
                            HinhAnh = ct.SanPham.HinhAnh,
                            SoLuong = ct.Quantity,
                            Gia = ct.Price
                        })
                        .ToList();

                    // Lấy 5 sản phẩm nổi bật để hiển thị thêm
                    ViewBag.sanpham = _context.SanPhams.Take(5).ToList();

                    return View(gioHang);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi khi parse JSON: " + ex.Message);
                    return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                // Không tìm thấy cookie, yêu cầu đăng nhập lại
                return RedirectToAction("Login", "Account");
            }
        }
        [AuthorizeRolesAttribute("admin", "User", "Employee")]
        public IActionResult XoaSanPhamKhoiGio(int id)
        {

            // tìm id chi tiết giỏ hàng từ id sản phẩm và từ id giỏ hang của người hiênj tại
            var chitietgiohang = _context.ChiTietGioHang.FirstOrDefault(c => c.ProductId == id && c.GioHangId == getIDGioHang());
            if (chitietgiohang == null)
            {
                return NotFound(new { Message = "Không tìm thấy sản phẩm trong giỏ hàng." });
            }

            try
            {
                _context.ChiTietGioHang.Remove(chitietgiohang);
                _context.SaveChanges();
                return RedirectToAction("XemGioHang", "HangHoa");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Lỗi khi xóa sản phẩm khỏi giỏ hàng");
                return BadRequest(new { Message = "Lỗi khi xóa sản phẩm", Error = e.Message });
            }
        }
        public int? getIDUser()
        {
            var iduser = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(iduser) || !int.TryParse(iduser, out int id))
            {
                _logger.LogWarning("Session UserId is missing or invalid.");
                return null; // Trả về null nếu UserId không tồn tại hoặc không hợp lệ
            }

            return id;
        }

        public int? getIDGioHang()
        {
            int? userId = getIDUser();
            if (userId == null)
            {
                _logger.LogWarning("UserId is null, cannot retrieve GioHang.");
                return null; // Không thể lấy giỏ hàng nếu UserId không tồn tại
            }

            var gioHang = _context.GioHang.FirstOrDefault(g => g.UserId == userId);
            if (gioHang == null)
            {
                _logger.LogWarning($"No GioHang found for UserId {userId}.");
                return null; // Trả về null nếu không tìm thấy giỏ hàng
            }

            return gioHang.Id;
        }
        [AuthorizeRolesAttribute("admin", "User", "Employee")]
        public IActionResult ThanhToan(int spId)
        {

            Console.WriteLine($"🔍 DEBUG: ID sản phẩm nhận được: {spId}");

            int? gioHangId = getIDGioHang();
            Console.WriteLine($"🔍 DEBUG: ID giỏ hàng lấy được: {gioHangId}");

            if (gioHangId == null)
            {
                return NotFound("Không tìm thấy giỏ hàng.");
            }

            var chitietgiohang = _context.ChiTietGioHang
                .Include(c => c.GioHang) // Load thêm thông tin giỏ hàng
                .Include(c => c.SanPham)  // Load thêm thông tin sản phẩm
                .FirstOrDefault(c => c.ProductId == spId && c.GioHangId == gioHangId);

            if (chitietgiohang == null)
            {
                Console.WriteLine($"⚠ Không tìm thấy chi tiết giỏ hàng với ProductId = {spId} và GioHangId = {gioHangId}");
                return NotFound($"Chi tiết giỏ hàng không tồn tại. ID sản phẩm: {spId}, ID giỏ hàng: {gioHangId}");
            }

            // Kiểm tra null trước khi sử dụng
            if (chitietgiohang.GioHang == null)
            {
                return NotFound("⚠ Lỗi: `GioHang` của sản phẩm bị null.");
            }
            if (chitietgiohang.SanPham == null)
            {
                return NotFound("⚠ Lỗi: `SanPham` của sản phẩm bị null.");
            }
            // kiểm tra còn hàng tốnf ko
            if ((chitietgiohang.SanPham.SoLuongTon - 1) < 0)
            {
                return NotFound("⚠ Lỗi: `SanPham` ko còn hàng");
            }
            var chiTietMuaHang = new ChiTietMuaHang
            {
                IdProduct = chitietgiohang.ProductId,
                IdUser = chitietgiohang.GioHang.UserId,
                TimeBuy = DateTime.Now,
                SoTien = chitietgiohang.SanPham.Gia * chitietgiohang.Quantity
            };

            _context.ChiTietMuaHangs.Add(chiTietMuaHang);
            _context.ChiTietGioHang.Remove(chitietgiohang);
            _context.SaveChanges();

            return RedirectToAction("XemGioHang", "HangHoa");
        }
        [AuthorizeRolesAttribute("admin", "User", "Employee")]
        public IActionResult LichSuMuaHang()
        {
            // lấy mail từ cookie chứ ko cần lâys tưf session nữa
            // Kiểm tra người dùng đã đăng nhập chưa
            var userEmail = "";

            if (Request.Cookies.TryGetValue("UserInfo", out string userInfoJson))
            {

                var user1 = System.Text.Json.JsonSerializer.Deserialize<UserInfo>(userInfoJson);

                userEmail = user1.UserEmail;


            }

            // Lấy thông tin người dùng từ cơ sở dữ liệu
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            // Truy vấn danh sách lịch sử mua hàng của người dùng
            var lichSu = _context.ChiTietMuaHangs
                .Where(mh => mh.IdUser == user.Id)
                .Include(mh => mh.SanPham) // Load thêm thông tin sản phẩm
                .Select(mh => new LichSuMuaHangView
                {
                    TenHang = mh.SanPham.TenSanPham, // Giả sử cột 'TenSanPham' trong bảng SanPham
                    HinhAnh = mh.SanPham.HinhAnh, // Giả sử có cột hình ảnh trong bảng SanPham
                    SoLuong = (int)(mh.SoTien / mh.SanPham.Gia), // Giả sử có cột SoLuong trong ChiTietMuaHangs
                    ThoiGianMua = mh.TimeBuy,
                    SoTien = mh.SoTien
                })
                .ToList();

            return View(lichSu);
        }
        [AuthorizeRolesAttribute("admin", "User", "Employee")]
        [HttpPost]
        public IActionResult ThanhToanTatCa(string danhSachSanPham)
        {
            var IDUser = -1;

            // 🛠 Lấy ID người dùng từ cookie
            if (Request.Cookies.TryGetValue("UserInfo", out string userInfoJson))
            {
                try
                {
                    var user = JsonConvert.DeserializeObject<UserInfo>(userInfoJson);
                    IDUser = user?.UserId ?? -1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi parse cookie: {ex.Message}");
                    TempData["thongbao"] = "Lỗi đăng nhập, vui lòng thử lại!";
                    return RedirectToAction("GioHang");
                }
            }

            Console.WriteLine($"📌 Dữ liệu JSON nhận được: {danhSachSanPham}");

            if (string.IsNullOrEmpty(danhSachSanPham))
            {
                TempData["thongbao"] = "Giỏ hàng trống!";
                return RedirectToAction("GioHang");
            }

            try
            {
                var danhSach = JsonConvert.DeserializeObject<List<SanPhamDTO>>(danhSachSanPham);

                foreach (var item in danhSach)
                {
                    var idSP = item.id;
                    if (!int.TryParse(item.soLuong.ToString(), out int soLuong) || soLuong <= 0)
                    {
                        TempData["thongbao"] = "Số lượng không hợp lệ";
                        Console.WriteLine($"❌ Số lượng không hợp lệ: {item.soLuong}");
                        continue;
                    }

                    var sp = _context.SanPhams.FirstOrDefault(sp => sp.Id == idSP);
                    if (sp == null)
                    {
                        TempData["thongbao"] = "không tìm thấy sản phẩm";
                        Console.WriteLine($"❌ Không tìm thấy sản phẩm : {idSP}");
                        continue;
                    }

                    if (sp.SoLuongTon < soLuong)
                    {
                        TempData["thongbao"] = "Không đủ hàng";
                        Console.WriteLine($"⚠ Không đủ hàng cho sản phẩm {sp.TenSanPham}. Tồn kho: {sp.SoLuongTon}, Yêu cầu: {soLuong}");
                        continue;
                    }

                    // 🛠 Lưu vào đơn hàng
                    var chiTietMuaHang = new ChiTietMuaHang
                    {
                        IdProduct = idSP,
                        IdUser = IDUser,
                        TimeBuy = DateTime.Now,
                        SoTien = sp.Gia * soLuong
                    };
                    _context.ChiTietMuaHangs.Add(chiTietMuaHang);

                    // 🛠 Cập nhật số lượng tồn kho
                    sp.SoLuongTon -= soLuong;
                }

                // 🛠 Lưu thay đổi vào CSDL
                _context.SaveChanges();

                // lấy id giỏ hàng của người dùng từ id người dung

                var giohang = _context.GioHang.FirstOrDefault(gh => gh.UserId == IDUser);

                // ✅ Xóa chi tiết giỏ hàng của các sản phẩm đã mua
                var chiTietGioHang = _context.ChiTietGioHang
                    .Where(ct => ct.GioHangId == giohang.Id && danhSach.Select(ds => ds.id).Contains(ct.ProductId))
                    .ToList();

                _context.ChiTietGioHang.RemoveRange(chiTietGioHang);
                _context.SaveChanges();

                ViewBag.danhSachSanPham = danhSach;
                TempData["Message"] = "Thanh toán thành công!";
                return View(danhSach);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi xử lý thanh toán: {ex.Message}");
                TempData["Message"] = "Lỗi trong quá trình thanh toán, vui lòng thử lại!";
                return RedirectToAction("XemGioHang");
            }
        }
        [AuthorizeRolesAttribute("admin", "User", "Employee")]
        [HttpGet]
        public IActionResult ThanhToanTrucTiep(int idSanPham, int soLuong)
        {
            // 🛠 Lấy ID người dùng từ cookie
            var IDUser = -1;
            if (Request.Cookies.TryGetValue("UserInfo", out string userInfoJson))
            {
                try
                {
                    var user = JsonConvert.DeserializeObject<UserInfo>(userInfoJson);
                    IDUser = user?.UserId ?? -1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi parse cookie: {ex.Message}");
                    TempData["Message"] = "Lỗi đăng nhập, vui lòng thử lại!";
                    return RedirectToAction("XemGioHang");
                }
            }

            // ❌ Kiểm tra nếu người dùng chưa đăng nhập
            if (IDUser == -1)
            {
                TempData["Message"] = "Vui lòng đăng nhập trước khi thanh toán!";
                return RedirectToAction("Login", "User");
            }

            // 🛠 Lấy sản phẩm từ database
            var sp = _context.SanPhams.FirstOrDefault(sp => sp.Id == idSanPham);
            if (sp == null)
            {
                TempData["Message"] = "Sản phẩm không tồn tại!";
                return RedirectToAction("XemGioHang");
            }

            // 🛠 Kiểm tra số lượng tồn kho
            if (sp.SoLuongTon < soLuong)
            {
                TempData["Message"] = $"Không đủ hàng cho sản phẩm {sp.TenSanPham}. Tồn kho: {sp.SoLuongTon}, Yêu cầu: {soLuong}";
                return RedirectToAction("XemGioHang");
            }

            // 🛠 Lưu vào đơn hàng
            var chiTietMuaHang = new ChiTietMuaHang
            {
                IdProduct = idSanPham,
                IdUser = IDUser,
                TimeBuy = DateTime.Now,
                SoTien = sp.Gia * soLuong
            };
            _context.ChiTietMuaHangs.Add(chiTietMuaHang);

            // 🛠 Cập nhật số lượng tồn kho
            sp.SoLuongTon -= soLuong;

            // 🛠 Lưu thay đổi vào CSDL
            _context.SaveChanges();

            TempData["Message"] = "Thanh toán thành công!";
            return RedirectToAction("LichSuMuaHang");
        }

        public IActionResult ChiTietSanPham(int id)
        {
            // Lấy chi tiết sản phẩm từ database
            var sanPham = _context.SanPhams.FirstOrDefault(sp => sp.Id == id);

            // Kiểm tra nếu sản phẩm không tồn tại
            if (sanPham == null)
            {
                return NotFound(); // Trả về trang 404 nếu không tìm thấy
            }

            // Lấy danh sách tất cả sản phẩm (nếu cần)
            var danhSachSanPham = _context.SanPhams.Take(5).ToList();

            // Dùng ViewModel thay vì ViewBag
            var viewModel = new ChiTietSanPhamViewModel
            {
                SanPham = sanPham,
                DanhSachSanPham = danhSachSanPham
            };

            return View(viewModel);
        }

        public IActionResult TimKiem()
        {
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public IActionResult TimKiem(string a)
        {
            if (string.IsNullOrWhiteSpace(a))
            {
                return View(new List<SanPham>());
            }

            string searchKey = a.ToLower(); // Chỉ gọi ToLower() một lần

            var sp = _context.SanPhams
                .Where(sp => EF.Functions.Like(sp.TenSanPham, $"%{searchKey}%") || EF.Functions.Like(sp.MoTa, $"%{searchKey}%"))
                .ToList();

            ViewBag.timkiem = a;
            return View(sp);
        }

        // Action tạo mã QR cho thanh toán
        public IActionResult GenerateQRCode(string totalAmount)
        {
            // Thông tin thanh toán
            string bankId = "MB"; // Mã ngân hàng MB
            string bankAccountNumber = "66624012005"; // Số tài khoản người nhận
            string bankRecipientName = "Nguyen Van Nhat"; // Tên người nhận
            string transactionCode = "TX-" + Guid.NewGuid().ToString("N"); // Mã giao dịch ngẫu nhiên (hoặc mã đơn hàng)

            // Mô tả giao dịch
            string description = "Thanh toán đơn hàng 123";

            // Loại bỏ dấu chấm và chuyển đổi số tiền thành integer
            totalAmount = totalAmount.Replace(".", "");  // Loại bỏ dấu chấm
            int amountInCents;

            if (!int.TryParse(totalAmount, out amountInCents))
            {
                // Xử lý khi không thể chuyển đổi số tiền
                return BadRequest("Số tiền không hợp lệ.");
            }

            // Nhân số tiền với 1000 để thêm 3 số 0
            string formattedAmount = (amountInCents * 1).ToString(); // Nhân với 1000 để có đơn vị "000"

            // Cấu trúc URL thanh toán qua VietQR
            string url = $"https://img.vietqr.io/image/{bankId}-{bankAccountNumber}-01.png?amount={formattedAmount}&addInfo={Uri.EscapeDataString(description)}&accountName={Uri.EscapeDataString(bankRecipientName)}";

            // Trả về ảnh QR từ URL thanh toán ngân hàng MB
            return Redirect(url); // Chuyển hướng đến URL của mã QR thanh toán
        }




    }
}


