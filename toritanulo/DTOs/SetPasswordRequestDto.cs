using System.ComponentModel.DataAnnotations;

namespace toritanulo.DTOs;

public class SetPasswordRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;
}
