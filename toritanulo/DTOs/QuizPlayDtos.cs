using System.Text.Json.Serialization;
using System.Text.Json;

namespace toritanulo.DTOs;

public class QuizTemakorDto
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Nev { get; set; } = string.Empty;
    public string? Leiras { get; set; }
    public int TesztDb { get; set; }
}

public class QuizTesztListItemDto
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
    public int KerdesDb { get; set; }
    public int OsszPont { get; set; }
}

public class QuizValaszOpcioDto
{
    public int Id { get; set; }
    public string ValaszSzoveg { get; set; } = string.Empty;
    public int Sorszam { get; set; }
}

public class QuizKerdesParDto
{
    public string BalOldal { get; set; } = string.Empty;
    public int Sorszam { get; set; }
}

public class QuizKerdesDto
{
    public int Id { get; set; }
    public string KerdesTipusKod { get; set; } = string.Empty;
    public string KerdesTipusNev { get; set; } = string.Empty;
    public string KerdesSzoveg { get; set; } = string.Empty;
    public string? Instrukcio { get; set; }
    public int Pontszam { get; set; }
    public int Nehezseg { get; set; }
    public List<QuizValaszOpcioDto> ValaszOpcioK { get; set; } = new();
    public List<QuizKerdesParDto> Parok { get; set; } = new();
}

public class QuizTesztReszletekDto : QuizTesztListItemDto
{
    public List<QuizKerdesDto> Kerdesek { get; set; } = new();
}

public class StartQuizAttemptRequestDto
{
    public int TesztId { get; set; }
}

public class SubmitQuizAnswerDto
{
    public int KerdesId { get; set; }
    public string? ValaszSzoveg { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? ValaszJson { get; set; }
}

public class SubmitQuizAttemptDto
{
    public int? ElteltMs { get; set; }
    public List<SubmitQuizAnswerDto> Valaszok { get; set; } = new();
}

public class QuizAttemptSummaryDto
{
    public int Id { get; set; }
    public int TesztId { get; set; }
    public string TesztCim { get; set; } = string.Empty;
    public string Statusz { get; set; } = string.Empty;
    public DateTime KezdveAt { get; set; }
    public DateTime? BekuldveAt { get; set; }
    public int Pontszam { get; set; }
    public int MaxPontszam { get; set; }
    public int HelyesDb { get; set; }
    public int OsszesKerdesDb { get; set; }
    public int? ElteltMs { get; set; }
}

public class QuizQuestionResultDto
{
    public int KerdesId { get; set; }
    public bool Helyes { get; set; }
    public int Pontszam { get; set; }
    public int MaxPontszam { get; set; }
    public string HelyesValaszOsszegzes { get; set; } = string.Empty;
    public string? Magyarazat { get; set; }
}

public class QuizSubmitResultDto
{
    public QuizAttemptSummaryDto Probalkozas { get; set; } = new();
    public List<QuizQuestionResultDto> KerdesEredmenyek { get; set; } = new();
}
