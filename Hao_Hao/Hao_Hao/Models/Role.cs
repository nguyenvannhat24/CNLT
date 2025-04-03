using System.ComponentModel.DataAnnotations;
namespace Hao_Hao
{
    public class Role
    {
        [Key]  // Chỉ định khóa chính cho bảng Role
        public int Id { get; set; } // ID của vai trò

        public string Name { get; set; } // Tên vai trò (Admin, User)

        // Mối quan hệ nhiều-nhiều với User thông qua bảng UserRole
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
