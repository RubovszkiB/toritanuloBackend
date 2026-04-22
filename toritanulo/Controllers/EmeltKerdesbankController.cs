using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using toritanulo.Data;
using toritanulo.Models;
using toritanulo.Services;

namespace toritanulo.Controllers;

[ApiController]
[Route("api/emelt-kerdesbank")]
[Authorize]
public class EmeltKerdesbankController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly EmeltKerdesbankImportService _importService;

    public EmeltKerdesbankController(ApplicationDbContext dbContext, EmeltKerdesbankImportService importService)
    {
        _dbContext = dbContext;
        _importService = importService;
    }

    [AllowAnonymous]
    [HttpGet("temak")]
    public async Task<ActionResult> GetTemak()
    {
        var items = await _dbContext.EmeltKerdesbankTemak
            .AsNoTracking()
            .Where(x => x.Aktiv)
            .OrderBy(x => x.Sorszam)
            .Select(x => new
            {
                id = x.Id,
                topicId = x.Id,
                code = x.EraId,
                kod = x.EraId,
                title = x.Cim,
                nev = x.Cim,
                description = x.KovetelmenyTartomany,
                order = x.Sorszam,
                questionCount = x.Resztemak.SelectMany(r => r.Kerdesek).Count(k => k.Aktiv),
                testCount = x.Resztemak.Count(r => r.Aktiv)
            })
            .ToListAsync();

        return Ok(items);
    }

    [AllowAnonymous]
    [HttpGet("resztemak")]
    public async Task<ActionResult> GetResztemak([FromQuery] int? temaId = null)
    {
        var query = _dbContext.EmeltKerdesbankResztemak
            .AsNoTracking()
            .Include(x => x.Tema)
            .Where(x => x.Aktiv);

        if (temaId.HasValue)
        {
            query = query.Where(x => x.TemaId == temaId.Value);
        }

        var items = await query
            .OrderBy(x => x.Tema.Sorszam)
            .ThenBy(x => x.Sorszam)
            .Select(x => new
            {
                id = x.Id,
                topicId = x.TemaId,
                topicTitle = x.Tema.Cim,
                slug = "emelt-" + x.Id,
                title = x.RequirementRef + " - " + x.Cim,
                description = x.Scope,
                type = "kerdesbank",
                difficulty = "nehez",
                questionCount = x.Kerdesek.Count(k => k.Aktiv),
                sourceQuestionCount = x.Kerdesek.Count(k => k.Aktiv && k.Forrasos),
                questions = Array.Empty<object>()
            })
            .ToListAsync();

        return Ok(items);
    }

    [AllowAnonymous]
    [HttpGet("tesztek")]
    [HttpGet("tests")]
    public Task<ActionResult> GetTesztek([FromQuery] int? topicId = null, [FromQuery] int? temakorId = null)
    {
        return GetResztemak(topicId ?? temakorId);
    }

    [AllowAnonymous]
    [HttpGet("tesztek/{slug}")]
    [HttpGet("tests/{slug}")]
    public async Task<ActionResult> GetTeszt(string slug, [FromQuery] bool sourcesOnly = false, [FromQuery] string? questionType = null)
    {
        var idPart = slug.Replace("emelt-", "", StringComparison.OrdinalIgnoreCase);
        if (!int.TryParse(idPart, out var resztemaId))
        {
            return NotFound(new { message = "A kérdésbank teszt nem található." });
        }

        var resztema = await _dbContext.EmeltKerdesbankResztemak
            .AsNoTracking()
            .Include(x => x.Tema)
            .FirstOrDefaultAsync(x => x.Id == resztemaId && x.Aktiv);

        if (resztema is null)
        {
            return NotFound(new { message = "A kérdésbank teszt nem található." });
        }

        var questions = await BuildQuestionQuery(resztemaId: resztemaId, sourcesOnly: sourcesOnly, questionType: questionType)
            .OrderBy(x => x.Sorszam)
            .Take(30)
            .ToListAsync();

        return Ok(new
        {
            id = resztema.Id,
            topicId = resztema.TemaId,
            topicTitle = resztema.Tema.Cim,
            slug = "emelt-" + resztema.Id,
            title = resztema.RequirementRef + " - " + resztema.Cim,
            description = "Emelt kérdésbank gyakorlás adatbázisból, forrásos és vegyes feladatokkal.",
            type = "kerdesbank",
            difficulty = "nehez",
            timeLimitSec = 1800,
            questionCount = questions.Count,
            questions = questions.Select(MapQuestion).ToList()
        });
    }

    [AllowAnonymous]
    [HttpGet("kerdesek")]
    public async Task<ActionResult> GetKerdesek(
        [FromQuery] int? temaId = null,
        [FromQuery] int? resztemaId = null,
        [FromQuery] string? questionType = null,
        [FromQuery] string? category = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] bool sourcesOnly = false,
        [FromQuery] int limit = 25)
    {
        var questions = await BuildQuestionQuery(temaId, resztemaId, questionType, category, difficulty, sourcesOnly)
            .OrderBy(x => x.Sorszam)
            .Take(Math.Clamp(limit, 1, 60))
            .ToListAsync();

        return Ok(questions.Select(MapQuestion));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("import")]
    public async Task<ActionResult> Import()
    {
        await _importService.ImportAsync();
        var count = await _dbContext.EmeltKerdesbankKerdesek.CountAsync();
        return Ok(new { message = "Az emelt kérdésbank importja elkészült.", questionCount = count });
    }

    private IQueryable<EmeltKerdesbankKerdes> BuildQuestionQuery(
        int? temaId = null,
        int? resztemaId = null,
        string? questionType = null,
        string? category = null,
        string? difficulty = null,
        bool sourcesOnly = false)
    {
        var query = _dbContext.EmeltKerdesbankKerdesek
            .AsNoTracking()
            .Include(x => x.Tema)
            .Include(x => x.Resztema)
            .Where(x => x.Aktiv);

        if (temaId.HasValue) query = query.Where(x => x.TemaId == temaId.Value);
        if (resztemaId.HasValue) query = query.Where(x => x.ResztemaId == resztemaId.Value);
        if (!string.IsNullOrWhiteSpace(questionType)) query = query.Where(x => x.KerdesTipus == questionType.Trim());
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(x => x.Kategoria == category.Trim());
        if (!string.IsNullOrWhiteSpace(difficulty)) query = query.Where(x => x.Nehezseg == difficulty.Trim());
        if (sourcesOnly) query = query.Where(x => x.Forrasos);

        return query;
    }

    private static object MapQuestion(EmeltKerdesbankKerdes question)
    {
        var interaction = ReadJson(question.InteractionJson);
        return new
        {
            id = question.Id,
            externalId = question.KulsoAzonosito,
            type = question.KerdesTipus,
            typeLabel = GetTypeLabel(question.KerdesTipus),
            text = question.KerdesSzoveg,
            instruction = question.Instrukcio,
            explanation = question.Magyarazat,
            difficulty = question.Nehezseg,
            points = question.KerdesTipus.Contains("multiple") || question.KerdesTipus.Contains("matrix") || question.KerdesTipus.Contains("matching") ? 2 : 1,
            order = question.Sorszam,
            category = question.Kategoria,
            skill = question.Scope,
            sourceHint = question.Forrasos ? "Forrásos emelt feladat" : "",
            sourceBlocks = ReadJson(question.SourceBlocksJson),
            interaction,
            knowledgeElements = ReadJson(question.KnowledgeElementsJson),
            tags = ReadJson(question.TagsJson),
            options = ExtractArray(interaction, "options"),
            pairs = ExtractPairs(interaction),
            statements = ExtractArray(interaction, "statements"),
            items = ExtractArray(interaction, "items"),
            sentenceWithBlank = ExtractString(interaction, "sentenceWithBlank"),
            correctOptionIds = ExtractStringArray(interaction, "correctOptionIds"),
            correctOrderIds = ExtractStringArray(interaction, "correctOrderIds"),
            correctPairs = ExtractArray(interaction, "correctPairs")
        };
    }

    private static JsonElement ReadJson(string json) => JsonSerializer.Deserialize<JsonElement>(json);

    private static JsonElement[] ExtractArray(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray().ToArray()
            : [];
    }

    private static string[] ExtractStringArray(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray().Select(x => x.GetString() ?? "").Where(x => x.Length > 0).ToArray()
            : [];
    }

    private static object[] ExtractPairs(JsonElement element)
    {
        if (!element.TryGetProperty("correctPairs", out var pairs) || pairs.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var leftItems = ExtractArray(element, "leftItems");
        var rightItems = ExtractArray(element, "rightItems");

        return pairs.EnumerateArray().Select((pair, index) =>
        {
            var leftId = pair.GetProperty("leftId").GetString();
            var rightId = pair.GetProperty("rightId").GetString();
            var leftText = leftItems.FirstOrDefault(x => ExtractString(x, "id") == leftId);
            var rightText = rightItems.FirstOrDefault(x => ExtractString(x, "id") == rightId);
            return new
            {
                id = leftId ?? $"L{index + 1}",
                left = ExtractString(leftText, "text"),
                right = ExtractString(rightText, "text"),
                leftId,
                rightId,
                order = index + 1
            };
        }).ToArray();
    }

    private static string ExtractString(JsonElement element, string property)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(property, out var value) &&
            value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? ""
                : "";
    }

    private static string GetTypeLabel(string type) => type switch
    {
        "single_choice" or "source_single_choice" => "Egyválasztós",
        "multiple_choice" or "source_multiple_choice" => "Többválasztós",
        "ordering" or "ordering_exam_style" => "Sorrendezés",
        "matching" or "source_statement_match" => "Párosítás",
        "true_false_matrix" => "Igaz-hamis mátrix",
        "fill_in_options" => "Kiegészítés",
        _ => "Kérdés"
    };
}
