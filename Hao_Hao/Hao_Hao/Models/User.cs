using Hao_Hao.Models;
using System.ComponentModel.DataAnnotations;
namespace Hao_Hao
{
    public class User
    {
        [Key]  // Chỉ định khóa chính cho bảng User
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // Mối quan hệ nhiều-nhiều với Role thông qua bảng UserRole
        public ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<ChiTietMuaHang> ChiTietMuaHangs { get; set; }
    }
}
