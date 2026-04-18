using System.ComponentModel.DataAnnotations;

namespace toritanulo.Models;

public class KronologiaEsemeny : IHasTimestamps
{
    public int Id { get; set; }
    public int TemakorId { get; set; }
    public int? TetelId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Cim { get; set; } = string.Empty;

    public string? RovidLeiras { get; set; }
    public int EvKezd { get; set; }
    public int? EvVeg { get; set; }

    [Required]
    [MaxLength(10)]
    public string Idoszamitas { get; set; } = "CE";

    [Required]
    [MaxLength(50)]
    public string EvszamSzoveg { get; set; } = string.Empty;

    public int RendezesiEv { get; set; }
    public byte Fontossag { get; set; } = 3;
    public bool Aktiv { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public TesztTemakor Temakor { get; set; } = null!;
    public Tetel? Tetel { get; set; }

    public ICollection<Kerdes> Kerdesek { get; set; } = new List<Kerdes>();
}
