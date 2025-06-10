using Hao_Hao.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ Razor Pages
builder.Services.AddRazorPages();

// Đăng ký dịch vụ khác như Authentication, Database Context, v.v.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "MyAppCookie";
        options.LoginPath = "/Account/login";  // Đảm bảo đường dẫn API đăng nhập
        options.LogoutPath = "/Acount/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;

        // Cấu hình yêu cầu cookie chỉ hoạt động qua HTTPS
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // Cookie chỉ hoạt động qua HTTPS
        options.Cookie.SameSite = SameSiteMode.None;  // Cho phép cookie cross-site nếu cần thiết
    });

// Thêm Session dung sesstion
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session tồn tại 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Đăng ký dịch vụ controllers và views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Cấu hình HSTS (HTTP Strict Transport Security) cho môi trường production
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();  // Bật HSTS trong môi trường production để yêu cầu HTTPS
}

// Chuyển hướng tất cả yêu cầu HTTP sang HTTPS
app.UseHttpsRedirection();

// Các middleware khác
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
// Đảm bảo Authentication và Authorization được sử dụng
app.UseAuthentication();
app.UseAuthorization();

// Đảm bảo định tuyến Razor Pages được cấu hình đúng
app.MapRazorPages();

// Định tuyến cho các controller
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
