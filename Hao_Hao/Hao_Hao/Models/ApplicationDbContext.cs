using Hao_Hao;
using Hao_Hao.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<GioHang> GioHang { get; set; }
    public DbSet<ChiTietGioHang> ChiTietGioHang { get; set; }
    public DbSet<SanPham> SanPhams { get; set; } // Thêm bảng Sản phẩm vào DbContext
    public DbSet<ChiTietMuaHang> ChiTietMuaHangs { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cấu hình kiểu dữ liệu decimal
        modelBuilder.Entity<ChiTietGioHang>()
            .Property(ct => ct.Price)
            .HasColumnType("decimal(18,2)"); // 18 chữ số, 2 chữ số sau dấu thập phân

        modelBuilder.Entity<SanPham>()
            .Property(sp => sp.Gia)
            .HasColumnType("decimal(18,2)"); // 18 chữ số, 2 chữ số sau dấu thập phân

        // Thiết lập mối quan hệ nhiều-nhiều giữa User và Role thông qua bảng UserRole
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });  // thêm khóa chính

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        // Thiết lập quan hệ giữa Giỏ Hàng và Chi Tiết Giỏ Hàng
        modelBuilder.Entity<ChiTietGioHang>()
            .HasOne(ct => ct.GioHang)
            .WithMany(g => g.ChiTietGioHangs)
            .HasForeignKey(ct => ct.GioHangId)
            .OnDelete(DeleteBehavior.Cascade); // Khi xóa giỏ hàng, các chi tiết giỏ hàng cũng bị xóa

        // Thiết lập quan hệ giữa Giỏ Hàng và User
        modelBuilder.Entity<GioHang>()
            .HasOne(g => g.User)
            .WithMany()
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Khi xóa User, giỏ hàng của họ cũng bị xóa

        // Thiết lập quan hệ giữa Chi Tiết Giỏ Hàng và Sản Phẩm
        modelBuilder.Entity<ChiTietGioHang>()
            .HasOne(ct => ct.SanPham)
            .WithMany(p => p.ChiTietGioHangs)
            .HasForeignKey(ct => ct.ProductId)
            .OnDelete(DeleteBehavior.Restrict); // Nếu xóa sản phẩm, không tự động xóa các chi tiết giỏ hàng

        // Thiết lập quan hệ giữa ChiTietMuaHang và User (1 User có nhiều đơn hàng)
        modelBuilder.Entity<ChiTietMuaHang>()
            .HasOne(ctmh => ctmh.User)
            .WithMany(u => u.ChiTietMuaHangs)
            .HasForeignKey(ctmh => ctmh.IdUser)
            .OnDelete(DeleteBehavior.Cascade); // Khi xóa User, các đơn hàng cũng bị xóa

        // Thiết lập quan hệ giữa ChiTietMuaHang và SanPham (1 Sản phẩm có thể có nhiều lượt mua)
        modelBuilder.Entity<ChiTietMuaHang>()
            .HasOne(ctmh => ctmh.SanPham)
            .WithMany(sp => sp.ChiTietMuaHangs)
            .HasForeignKey(ctmh => ctmh.IdProduct)
            .OnDelete(DeleteBehavior.Restrict); // Nếu xóa sản phẩm, không xóa đơn hàng

    }
}
