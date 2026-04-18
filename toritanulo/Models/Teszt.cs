using System.ComponentModel.DataAnnotations;

namespace toritanulo.Models;

public class Teszt : IHasTimestamps
{
    public int Id { get; set; }
    public int TemakorId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Cim { get; set; } = string.Empty;

    public string? Leiras { get; set; }

    [Required]
    [MaxLength(20)]
    public string TesztTipus { get; set; } = "evszam";

    [Required]
    [MaxLength(20)]
    public string Nehezseg { get; set; } = "konnyu";

    public int? IdokeretMp { get; set; }
    public bool Aktiv { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public TesztTemakor Temakor { get; set; } = null!;
    public ICollection<TesztKerdes> TesztKerdesek { get; set; } = new List<TesztKerdes>();
    public ICollection<TesztProbalkozas> TesztProbalkozasok { get; set; } = new List<TesztProbalkozas>();
}
