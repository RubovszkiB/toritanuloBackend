using System.ComponentModel.DataAnnotations;

namespace toritanulo.DTOs;

public class CreateUserRequestDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [StringLength(100)]
    public string? FullName { get; set; }

    [Required]
    public string Role { get; set; } = "Student";

    public bool IsActive { get; set; } = true;
}
