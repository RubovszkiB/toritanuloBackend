namespace toritanulo.DTOs;

public class TetelDetailDto
{
    public int Id { get; set; }
    public int Sorszam { get; set; }
    public string Cim { get; set; } = string.Empty;
    public string ForrasFajlnev { get; set; } = string.Empty;
    public string Tartalom { get; set; } = string.Empty;
    public int BekezdesDb { get; set; }
    public bool Aktiv { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
