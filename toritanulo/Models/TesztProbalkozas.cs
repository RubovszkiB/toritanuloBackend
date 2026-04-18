namespace toritanulo.Models;

public class TesztProbalkozas
{
    public int Id { get; set; }
    public int TesztId { get; set; }
    public int UserId { get; set; }

    public string Statusz { get; set; } = "started";
    public DateTime KezdveAt { get; set; } = DateTime.UtcNow;
    public DateTime? BekuldveAt { get; set; }

    public int Pontszam { get; set; }
    public int MaxPontszam { get; set; }
    public int HelyesDb { get; set; }
    public int OsszesKerdesDb { get; set; }
    public int? ElteltMs { get; set; }

    public Teszt Teszt { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<TesztValasz> Valaszok { get; set; } = new List<TesztValasz>();
}
