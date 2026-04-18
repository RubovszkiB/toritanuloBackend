using System.ComponentModel.DataAnnotations;

namespace toritanulo.Models;

public class KerdesValaszOpcio
{
    public int Id { get; set; }
    public int KerdesId { get; set; }

    [Required]
    [MaxLength(255)]
    public string ValaszSzoveg { get; set; } = string.Empty;

    public bool Helyes { get; set; }
    public int? HelyesSorrend { get; set; }
    public int Sorszam { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Kerdes Kerdes { get; set; } = null!;
}
