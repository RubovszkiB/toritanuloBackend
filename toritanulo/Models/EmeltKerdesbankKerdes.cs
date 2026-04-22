using System.ComponentModel.DataAnnotations;

namespace toritanulo.Models;

public class EmeltKerdesbankKerdes : IHasTimestamps
{
    public int Id { get; set; }
    public int TemaId { get; set; }
    public int ResztemaId { get; set; }

    [Required]
    [MaxLength(80)]
    public string KulsoAzonosito { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string KerdesTipus { get; set; } = string.Empty;

    [Required]
    public string KerdesSzoveg { get; set; } = string.Empty;

    public string? Instrukcio { get; set; }
    public string? Magyarazat { get; set; }

    [MaxLength(30)]
    public string Nehezseg { get; set; } = "hard";

    [MaxLength(40)]
    public string Scope { get; set; } = "vegyes";

    [MaxLength(40)]
    public string Kategoria { get; set; } = "vegyes";

    public bool Forrasos { get; set; }
    public bool ExamInspired { get; set; }
    public int Sorszam { get; set; }

    public string InteractionJson { get; set; } = "{}";
    public string SourceBlocksJson { get; set; } = "[]";
    public string KnowledgeElementsJson { get; set; } = "{}";
    public string TagsJson { get; set; } = "[]";
    public string RawJson { get; set; } = "{}";

    public bool Aktiv { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public EmeltKerdesbankTema Tema { get; set; } = null!;
    public EmeltKerdesbankResztema Resztema { get; set; } = null!;
}
