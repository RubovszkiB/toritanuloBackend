using System.ComponentModel.DataAnnotations;

namespace toritanulo.Models;

public class EmeltKerdesbankResztema : IHasTimestamps
{
    public int Id { get; set; }
    public int TemaId { get; set; }

    [Required]
    [MaxLength(80)]
    public string RequirementRef { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Cim { get; set; } = string.Empty;

    [MaxLength(40)]
    public string Scope { get; set; } = "vegyes";

    public int Sorszam { get; set; }
    public bool Aktiv { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public EmeltKerdesbankTema Tema { get; set; } = null!;
    public ICollection<EmeltKerdesbankKerdes> Kerdesek { get; set; } = new List<EmeltKerdesbankKerdes>();
}
