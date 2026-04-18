using System.ComponentModel.DataAnnotations;

namespace toritanulo.Models;

public class KerdesHelyesValasz
{
    public int Id { get; set; }
    public int KerdesId { get; set; }

    [MaxLength(255)]
    public string? ValaszSzoveg { get; set; }

    public int? ValaszSzam { get; set; }

    [Required]
    [MaxLength(10)]
    public string Era { get; set; } = "NONE";

    [Required]
    [MaxLength(255)]
    public string NormalizaltValasz { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Kerdes Kerdes { get; set; } = null!;
}
