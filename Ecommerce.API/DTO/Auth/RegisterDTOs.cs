using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.DTO.Auth;

public class RegisterDTO
{
   [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
   [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 100 ký tự.")]
   public string FullName { get; set; } = string.Empty;

   [Required(ErrorMessage = "Vui lòng nhập email.")]
   [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
   public string Email { get; set; } = string.Empty;

   [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
   [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
   public string Password { get; set; } = string.Empty;
}