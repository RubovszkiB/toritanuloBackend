namespace toritanulo.DTOs;

public class TetelProgressDto
{
    public int TetelId { get; set; }

    public int HaladasSzazalek { get; set; }

    public int LastPage { get; set; } = 1;

    public decimal ScrollProgress { get; set; }

    public int PageCount { get; set; }

    public bool Completed { get; set; }

    public bool VanMentes { get; set; }

    public DateTime? UtolsoMentesAt { get; set; }
}
