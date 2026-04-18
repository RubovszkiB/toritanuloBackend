namespace toritanulo.Models;

public class TesztKerdes
{
    public int Id { get; set; }
    public int TesztId { get; set; }
    public int KerdesId { get; set; }
    public int Sorszam { get; set; }
    public int Pontszam { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Teszt Teszt { get; set; } = null!;
    public Kerdes Kerdes { get; set; } = null!;
}
