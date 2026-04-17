using System.ComponentModel.DataAnnotations;

namespace toritanulo.Models;

public class Tetel
{
    public int Id { get; set; }

    public int Sorszam { get; set; }

    [Required]
    [MaxLength(255)]
    public string Cim { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string ForrasFajlnev { get; set; } = string.Empty;

    [Required]
    public string Tartalom { get; set; } = string.Empty;

    public int BekezdesDb { get; set; }

    public bool Aktiv { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
