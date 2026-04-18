using System.ComponentModel.DataAnnotations;

namespace toritanulo.DTOs;

public class AdminValaszOpcioDto
{
    public int Id { get; set; }
    public string ValaszSzoveg { get; set; } = string.Empty;
    public bool Helyes { get; set; }
    public int? HelyesSorrend { get; set; }
    public int Sorszam { get; set; }
}

public class AdminHelyesValaszDto
{
    public int Id { get; set; }
    public string? ValaszSzoveg { get; set; }
    public int? ValaszSzam { get; set; }
    public string Era { get; set; } = "NONE";
    public string NormalizaltValasz { get; set; } = string.Empty;
}

public class AdminKerdesParDto
{
    public int Id { get; set; }
    public string BalOldal { get; set; } = string.Empty;
    public string JobbOldal { get; set; } = string.Empty;
    public int Sorszam { get; set; }
}

public class AdminValaszOpcioUpsertDto
{
    [Required]
    [StringLength(255)]
    public string ValaszSzoveg { get; set; } = string.Empty;

    public bool Helyes { get; set; }
    public int? HelyesSorrend { get; set; }
    public int Sorszam { get; set; }
}

public class AdminHelyesValaszUpsertDto
{
    [StringLength(255)]
    public string? ValaszSzoveg { get; set; }

    public int? ValaszSzam { get; set; }

    [StringLength(10)]
    public string Era { get; set; } = "NONE";

    [StringLength(255)]
    public string? NormalizaltValasz { get; set; }
}

public class AdminKerdesParUpsertDto
{
    [Required]
    [StringLength(255)]
    public string BalOldal { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string JobbOldal { get; set; } = string.Empty;

    public int Sorszam { get; set; }
}

public class AdminKerdesUpsertDto
{
    [Required]
    public int TemakorId { get; set; }

    [Required]
    public int KerdesTipusId { get; set; }

    public int? KronologiaEsemenyId { get; set; }

    [Required]
    public string KerdesSzoveg { get; set; } = string.Empty;

    public string? Instrukcio { get; set; }
    public string? Magyarazat { get; set; }

    [Range(1, 5)]
    public int Nehezseg { get; set; } = 2;

    public bool Aktiv { get; set; } = true;

    public List<AdminHelyesValaszUpsertDto> HelyesValaszok { get; set; } = new();
    public List<AdminValaszOpcioUpsertDto> ValaszOpcioK { get; set; } = new();
    public List<AdminKerdesParUpsertDto> Parok { get; set; } = new();
}

public class AdminKerdesDto
{
    public int Id { get; set; }
    public int TemakorId { get; set; }
    public string TemakorNev { get; set; } = string.Empty;
    public int KerdesTipusId { get; set; }
    public string KerdesTipusKod { get; set; } = string.Empty;
    public string KerdesTipusNev { get; set; } = string.Empty;
    public int? KronologiaEsemenyId { get; set; }
    public string? KronologiaEsemenyCim { get; set; }
    public string KerdesSzoveg { get; set; } = string.Empty;
    public string? Instrukcio { get; set; }
    public string? Magyarazat { get; set; }
    public int Nehezseg { get; set; }
    public bool Aktiv { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AdminHelyesValaszDto> HelyesValaszok { get; set; } = new();
    public List<AdminValaszOpcioDto> ValaszOpcioK { get; set; } = new();
    public List<AdminKerdesParDto> Parok { get; set; } = new();
}

public class AdminTesztKerdesUpsertDto
{
    [Required]
    public int KerdesId { get; set; }

    public int Sorszam { get; set; }
    public int Pontszam { get; set; } = 1;
}

public class AdminTesztKerdesDto
{
    public int Id { get; set; }
    public int KerdesId { get; set; }
    public int Sorszam { get; set; }
    public int Pontszam { get; set; }
    public string KerdesSzoveg { get; set; } = string.Empty;
    public string KerdesTipusKod { get; set; } = string.Empty;
}

public class AdminTesztUpsertDto
{
    [Required]
    public int TemakorId { get; set; }

    [Required]
    [StringLength(150)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Cim { get; set; } = string.Empty;

    public string? Leiras { get; set; }

    [Required]
    [StringLength(20)]
    public string TesztTipus { get; set; } = "evszam";

    [Required]
    [StringLength(20)]
    public string Nehezseg { get; set; } = "konnyu";

    public int? IdokeretMp { get; set; }
    public bool Aktiv { get; set; } = true;

    public List<AdminTesztKerdesUpsertDto> Kerdesek { get; set; } = new();
}

public class AdminTesztDto
{
    public int Id { get; set; }
    public int TemakorId { get; set; }
    public string TemakorNev { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Cim { get; set; } = string.Empty;
    public string? Leiras { get; set; }
    public string TesztTipus { get; set; } = string.Empty;
    public string Nehezseg { get; set; } = string.Empty;
    public int? IdokeretMp { get; set; }
    public bool Aktiv { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int KerdesDb { get; set; }
    public int OsszPont { get; set; }
    public List<AdminTesztKerdesDto> Kerdesek { get; set; } = new();
}
