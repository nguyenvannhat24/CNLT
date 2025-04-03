namespace Hao_Hao.Models
{
    public class VerifyCodeViewModel
    {
        // Mã xác nhận người dùng nhập vào
        public string VerificationCode { get; set; }

        // Mật khẩu mới mà người dùng muốn thay đổi
        public string NewPassword { get; set; }

        // Xác nhận mật khẩu mới
        public string ConfirmPassword { get; set; }
    }
}
