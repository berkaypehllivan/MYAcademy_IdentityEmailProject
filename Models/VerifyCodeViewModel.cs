using System.ComponentModel.DataAnnotations;

namespace MYAcademy_IdentityEmailProject.Models;

public class VerifyCodeViewModel
{
    [Required]
    public string Email { get; set; }

    [Required(ErrorMessage = "Doğrulama kodu zorunludur.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Doğrulama kodu 6 haneli olmalıdır.")]
    public string Code { get; set; }

    public int ResendCooldown { get; set; }
}
