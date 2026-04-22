using System.ComponentModel.DataAnnotations;

namespace toritanulo.Models;

public class EmeltKerdesbankTema : IHasTimestamps
{
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string EraId { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Cim { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? KovetelmenyTartomany { get; set; }

    public int Sorszam { get; set; }
    public bool Aktiv { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<EmeltKerdesbankResztema> Resztemak { get; set; } = new List<EmeltKerdesbankResztema>();
}
