using Microsoft.AspNetCore.Identity;

namespace Hao_Hao.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Thêm các thuộc tính bổ sung nếu cần
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string Password { get; internal set; }
    }
}
