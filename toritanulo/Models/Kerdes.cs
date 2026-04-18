using System.ComponentModel.DataAnnotations;

namespace toritanulo.Models;

public class Kerdes : IHasTimestamps
{
    public int Id { get; set; }
    public int TemakorId { get; set; }
    public int KerdesTipusId { get; set; }
    public int? KronologiaEsemenyId { get; set; }

    [Required]
    public string KerdesSzoveg { get; set; } = string.Empty;

    public string? Instrukcio { get; set; }
    public string? Magyarazat { get; set; }
    public byte Nehezseg { get; set; } = 2;
    public bool Aktiv { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public TesztTemakor Temakor { get; set; } = null!;
    public KerdesTipus KerdesTipus { get; set; } = null!;
    public KronologiaEsemeny? KronologiaEsemeny { get; set; }

    public ICollection<KerdesHelyesValasz> HelyesValaszok { get; set; } = new List<KerdesHelyesValasz>();
    public ICollection<KerdesValaszOpcio> ValaszOpcioK { get; set; } = new List<KerdesValaszOpcio>();
    public ICollection<KerdesPar> Parok { get; set; } = new List<KerdesPar>();
    public ICollection<TesztKerdes> TesztKerdesek { get; set; } = new List<TesztKerdes>();
    public ICollection<TesztValasz> TesztValaszok { get; set; } = new List<TesztValasz>();
}
