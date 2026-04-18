namespace toritanulo.Models;

public class TetelOlvasasiAllapot : IHasTimestamps
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TetelId { get; set; }
    public int HaladasSzazalek { get; set; }
    public DateTime LastOpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Tetel Tetel { get; set; } = null!;
}
