namespace toritanulo.Models;

public class TesztValasz
{
    public int Id { get; set; }
    public int ProbalkozasId { get; set; }
    public int KerdesId { get; set; }

    public string? ValaszSzoveg { get; set; }
    public string? ValaszJson { get; set; }

    public bool Helyes { get; set; }
    public int Pontszam { get; set; }
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

    public TesztProbalkozas Probalkozas { get; set; } = null!;
    public Kerdes Kerdes { get; set; } = null!;
}
