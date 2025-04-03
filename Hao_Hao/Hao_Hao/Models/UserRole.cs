using System.ComponentModel.DataAnnotations;

namespace Hao_Hao
{
    public class UserRole
    {
        public int UserId { get; set; }  // Khóa ngoại từ bảng User
        public User User { get; set; }  // Liên kết với bảng User

        public int RoleId { get; set; }  // Khóa ngoại từ bảng Role
        public Role Role { get; set; }  // Liên kết với bảng Role
    }
}
