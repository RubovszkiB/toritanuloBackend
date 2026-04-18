using System.ComponentModel.DataAnnotations;

namespace toritanulo.Models;

public class KerdesPar
{
    public int Id { get; set; }
    public int KerdesId { get; set; }

    [Required]
    [MaxLength(255)]
    public string BalOldal { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string JobbOldal { get; set; } = string.Empty;

    public int Sorszam { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Kerdes Kerdes { get; set; } = null!;
}
