using System.ComponentModel.DataAnnotations;

namespace toritanulo.Models;

public class TesztTemakor : IHasTimestamps
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Kod { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Nev { get; set; } = string.Empty;

    public string? Leiras { get; set; }
    public int Sorszam { get; set; }
    public bool Aktiv { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<KronologiaEsemeny> KronologiaEsemenyek { get; set; } = new List<KronologiaEsemeny>();
    public ICollection<Teszt> Tesztek { get; set; } = new List<Teszt>();
    public ICollection<Kerdes> Kerdesek { get; set; } = new List<Kerdes>();
}
