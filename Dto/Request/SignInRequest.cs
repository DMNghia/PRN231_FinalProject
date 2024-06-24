using System.ComponentModel.DataAnnotations;

namespace FinalProject.Dto.Request
{
    public class SignInRequest
    {
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Required(ErrorMessage = "Email không được để trống")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; }
    }
}
