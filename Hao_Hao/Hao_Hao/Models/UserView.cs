using System.Reflection.Metadata;

namespace Hao_Hao.Models
{
    public class UserView
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public string Email { get; set; }
        public List<Role> Role { get; set; }
        public List<int> SelectedRoleIds { get; set; }  // ✅ Chấp nhận danh sách quyền
        public List<Role> AvailableRoles { get; set; }  // Danh sách quyền có thể chọn
    }
}
