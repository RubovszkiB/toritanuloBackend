using System.ComponentModel.DataAnnotations;

namespace toritanulo.Models;

public class TetelOlvasasiAllapot
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int TetelId { get; set; }

    [Range(0, 100)]
    public int HaladasSzazalek { get; set; }

    public DateTime LastOpenedAt { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
