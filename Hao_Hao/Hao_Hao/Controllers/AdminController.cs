using Hao_Hao.Attributes;
using Hao_Hao.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;

namespace Hao_Hao.Controllers
{
    [AuthorizeRolesAttribute("admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult home()
        {
            var result = _context.ChiTietMuaHangs
             .Include(c => c.SanPham) // Lấy thông tin sản phẩm
             .GroupBy(c => new { c.SanPham.TenSanPham, c.SanPham.SoLuongTon, c.SanPham.Gia })
             .Select(g => new BaoCaoMuaHang
             {
                 TenSanPham = g.Key.TenSanPham,
                 SoLuongTon = g.Key.SoLuongTon,
                 Gia = g.Key.Gia,
                 TongTien = g.Sum(c => c.SoTien),
                 SoLuong = g.Sum(c => c.SoTien) / g.Key.Gia
             })
             .ToList();

            return View(result);
        }
        public IActionResult QuanLyNguoiDung()
        {
            var Users = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToList();

            // hiển thị Tên Email quyền người dùng
            List<UserView> users = new List<UserView>();    
            foreach (var u in Users)
            {
                // lấy ra danh sách quyền của người dùng
               List<Role> roles = new List<Role>();
                foreach (var ur in u.UserRoles) {
                    Role role = new Role
                    {   Id = ur.Role.Id,
                        Name = ur.Role.Name,
                    };
                    roles.Add(role);
                }

                UserView userview = new UserView
                {
                    Id = u.Id,
                    Name = u.FullName,
                    Email = u.Email,
                    Role = roles 

                };
                users.Add(userview);
            }

            return View(users);
        }
        public IActionResult PhanQuyen(int idUser)
        {
            var user = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefault(u => u.Id == idUser);

            if (user == null)
            {
                return NotFound();
            }

            var allRoles = _context.Roles.ToList();  // Lấy tất cả quyền từ database
            var userRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();  // Lấy quyền của user

            var userView = new UserView
            {
                Id = idUser,
                Name = user.FullName,
                Email = user.Email,
                SelectedRoleIds = userRoleIds,
                AvailableRoles = allRoles
            };

            return View(userView);
        }

        public IActionResult XoaQuyen(int iDUser, int IdRole )
        {
            TempData["Message"] = "id người dùng là " + iDUser + "quyền người dùng là " + IdRole;
                    
            

            var userRole = _context.UserRoles
                .FirstOrDefault(ur => ur.RoleId == IdRole && ur.UserId == iDUser);
            if (userRole == null)
            {
                TempData["Message"] = "ko tìm thấy quyền";

                return NotFound();

            }

            _context.UserRoles.Remove(userRole);
            _context.SaveChanges();
            TempData["Message"] = "Xóa quyền thành công!";

            return RedirectToAction("QuanLyNguoiDung", "Admin");
        }
        public IActionResult capnhat(int Id , List<int> SelectedRoleIds)
        {
          
            ViewBag.id = Id;
            ViewBag.Quyen = string.Join(", ", SelectedRoleIds);

            if (SelectedRoleIds == null || !SelectedRoleIds.Any())
            {
                TempData["Message"] = "Không có quyền nào được chọn!";
                return RedirectToAction("QuanLyNguoiDung");
            }
            var user = _context.Users
        .Include(u => u.UserRoles)
        .FirstOrDefault(u => u.Id == Id);
            if (user == null)
            {
                return NotFound();
            }

            // **Chỉ thêm quyền mới, không xóa quyền cũ**
            foreach (var roleId in SelectedRoleIds)
            {
                if (!user.UserRoles.Any(ur => ur.RoleId == roleId))
                {
                    user.UserRoles.Add(new UserRole { UserId = Id, RoleId = roleId });
                }
            }
            _context.SaveChanges();
            TempData["Message"] = "Cập nhật quyền thành công!";

            return RedirectToAction("QuanLyNguoiDung", "Admin");
        }
        public IActionResult QuanLyhang()
        {
            var hang = _context.SanPhams.ToList();
            return View(hang);
        }
        [HttpPost]
        public IActionResult XoaSanPham(int id)
        {
            bool daMua = _context.ChiTietMuaHangs.Any(x => x.IdProduct == id);
            if (daMua)
            {
                TempData["Loi"] = "Không thể xoá sản phẩm đã có trong đơn mua hàng.";
                return RedirectToAction("home"); // Đổi từ "Index" sang action hiện có
            }

            var sanPham = _context.SanPhams.FirstOrDefault(s => s.Id == id);
            if (sanPham != null)
            {
                _context.SanPhams.Remove(sanPham);
                _context.SaveChanges();
            }

            return RedirectToAction("home"); // Hoặc "home" nếu danh sách sản phẩm ở đó
        }
        // GET
        [HttpGet]
        public IActionResult ChinhSuaSanPham(int id)
        {
            var sanPham = _context.SanPhams.FirstOrDefault(sp => sp.Id == id);
            if (sanPham == null) return NotFound();
            return View(sanPham);
        }

        [HttpPost]
        public async Task<IActionResult> ChinhSuaSanPham(int id, string TenSanPham, string MoTa, decimal Gia, int SoLuongTon, string HinhAnhLink, string HinhAnhCu)
        {
            var sp = _context.SanPhams.FirstOrDefault(sp => sp.Id == id);
            if (sp == null) return NotFound();

            sp.TenSanPham = TenSanPham;
            sp.MoTa = MoTa;
            sp.Gia = Gia;
            sp.SoLuongTon = SoLuongTon;

            // Nếu người dùng nhập link hình ảnh mới, sử dụng link đó, nếu không giữ nguyên hình ảnh cũ
            if (!string.IsNullOrEmpty(HinhAnhLink))
            {
                sp.HinhAnh = HinhAnhLink; // Cập nhật với đường dẫn ảnh mới
            }
            else
            {
                sp.HinhAnh = HinhAnhCu; // Giữ lại hình ảnh cũ
            }

            _context.SaveChanges();
            return RedirectToAction("Home");
        }

        public IActionResult ThemSanPham()
        { 
            return View(new SanPham());
        }
        [HttpPost]
        public IActionResult ThemSanPham(SanPham sanPham)
        {
            try
            {
                // Kiểm tra nếu thông tin sản phẩm hợp lệ
                if (ModelState.IsValid)
                {
                    // Nếu có URL hình ảnh, lưu URL vào thuộc tính HinhAnh
                    if (!string.IsNullOrEmpty(sanPham.HinhAnh))
                    {
                        // Có thể kiểm tra tính hợp lệ của URL hình ảnh nếu cần
                        if (!Uri.IsWellFormedUriString(sanPham.HinhAnh, UriKind.Absolute))
                        {
                            ModelState.AddModelError("HinhAnh", "Đường dẫn hình ảnh không hợp lệ.");
                            return View(sanPham);
                        }
                    }

                    // Thêm sản phẩm vào database
                    _context.SanPhams.Add(sanPham);
                    _context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu

                    // Hiển thị thông báo và chuyển hướng về trang danh sách sản phẩm
                    TempData["Message"] = "Đã thêm thành công vào sanpham";
                    return RedirectToAction("home"); // Điều hướng về danh sách sản phẩm
                }
                else
                {
                    // Nếu có lỗi, trả lại form để người dùng sửa
                    TempData["Message"] = "Có lỗi khi thêm sản phẩm.";
                    return View(sanPham);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                return Content("❌ Lỗi khi thêm sản phẩm: " + ex.Message);
            }
        }

    }
}
