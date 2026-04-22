using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using toritanulo.Data;
using toritanulo.Models;

namespace toritanulo.Services;

public class EmeltKerdesbankImportService
{
    private const string SourceFileName = "emelt_tortenelem_teljes_kerdesbank_codexbarat_v2_forrasos.json";
    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<EmeltKerdesbankImportService> _logger;

    public EmeltKerdesbankImportService(
        ApplicationDbContext dbContext,
        IWebHostEnvironment environment,
        ILogger<EmeltKerdesbankImportService> logger)
    {
        _dbContext = dbContext;
        _environment = environment;
        _logger = logger;
    }

    public async Task ImportAsync(CancellationToken cancellationToken = default)
    {
        await EnsureTablesAsync(cancellationToken);

        var filePath = Path.Combine(_environment.ContentRootPath, "Data", SourceFileName);
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Az emelt kérdésbank forrásfájl nem található: {Path}", filePath);
            return;
        }

        await using var stream = File.OpenRead(filePath);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;
        var questions = root.GetProperty("questions").EnumerateArray().ToList();
        var currentCount = await _dbContext.EmeltKerdesbankKerdesek.CountAsync(cancellationToken);

        if (currentCount == questions.Count && currentCount > 0)
        {
            return;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await _dbContext.EmeltKerdesbankKerdesek.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.EmeltKerdesbankResztemak.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.EmeltKerdesbankTemak.ExecuteDeleteAsync(cancellationToken);

        var temaMap = new Dictionary<string, EmeltKerdesbankTema>();
        var resztemaMap = new Dictionary<string, EmeltKerdesbankResztema>();

        var themes = root.TryGetProperty("themes", out var themesElement)
            ? themesElement.EnumerateArray().ToList()
            : [];

        for (var index = 0; index < themes.Count; index++)
        {
            var item = themes[index];
            var eraId = GetString(item, "eraId", $"era_{index + 1}");
            var tema = new EmeltKerdesbankTema
            {
                EraId = eraId,
                Cim = GetString(item, "eraTitle", eraId),
                KovetelmenyTartomany = GetNullableString(item, "requirementRange"),
                Sorszam = index + 1,
                Aktiv = true
            };

            _dbContext.EmeltKerdesbankTemak.Add(tema);
            temaMap[eraId] = tema;

            if (item.TryGetProperty("subtopics", out var subtopicsElement) && subtopicsElement.ValueKind == JsonValueKind.Array)
            {
                var subtopics = subtopicsElement.EnumerateArray().ToList();
                for (var subIndex = 0; subIndex < subtopics.Count; subIndex++)
                {
                    var subtopic = subtopics[subIndex];
                    var requirementRef = GetString(subtopic, "requirementRef", "");
                    var title = GetString(subtopic, "title", GetString(subtopic, "subtopicTitle", "Résztéma"));
                    var key = BuildSubtopicKey(eraId, requirementRef, title);
                    var resztema = new EmeltKerdesbankResztema
                    {
                        Tema = tema,
                        RequirementRef = requirementRef,
                        Cim = title,
                        Scope = GetString(subtopic, "scope", "vegyes"),
                        Sorszam = subIndex + 1,
                        Aktiv = true
                    };

                    _dbContext.EmeltKerdesbankResztemak.Add(resztema);
                    resztemaMap[key] = resztema;
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        for (var index = 0; index < questions.Count; index++)
        {
            var question = questions[index];
            var eraId = GetString(question, "eraId", "unknown");
            if (!temaMap.TryGetValue(eraId, out var tema))
            {
                tema = new EmeltKerdesbankTema
                {
                    EraId = eraId,
                    Cim = GetString(question, "eraTitle", eraId),
                    Sorszam = temaMap.Count + 1,
                    Aktiv = true
                };
                _dbContext.EmeltKerdesbankTemak.Add(tema);
                temaMap[eraId] = tema;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var requirementRef = GetString(question, "requirementRef", "");
            var subtopicTitle = GetString(question, "subtopicTitle", "Résztéma");
            var subtopicKey = BuildSubtopicKey(eraId, requirementRef, subtopicTitle);
            if (!resztemaMap.TryGetValue(subtopicKey, out var resztema))
            {
                resztema = new EmeltKerdesbankResztema
                {
                    TemaId = tema.Id,
                    RequirementRef = requirementRef,
                    Cim = subtopicTitle,
                    Scope = GetString(question, "scope", "vegyes"),
                    Sorszam = resztemaMap.Count + 1,
                    Aktiv = true
                };
                _dbContext.EmeltKerdesbankResztemak.Add(resztema);
                await _dbContext.SaveChangesAsync(cancellationToken);
                resztemaMap[subtopicKey] = resztema;
            }

            var sourceBlocksJson = GetRawJson(question, "sourceBlocks", "[]");
            var knowledgeJson = GetRawJson(question, "knowledgeElements", "{}");
            var tagsJson = GetRawJson(question, "tags", "[]");

            _dbContext.EmeltKerdesbankKerdesek.Add(new EmeltKerdesbankKerdes
            {
                TemaId = tema.Id,
                ResztemaId = resztema.Id,
                KulsoAzonosito = GetString(question, "id", $"Q{index + 1:D4}"),
                KerdesTipus = GetString(question, "questionType", "single_choice"),
                KerdesSzoveg = GetString(question, "prompt", "Kérdés"),
                Instrukcio = GetNullableString(question, "instruction"),
                Magyarazat = GetNullableString(question, "explanation"),
                Nehezseg = GetString(question, "difficulty", "hard"),
                Scope = GetString(question, "scope", "vegyes"),
                Kategoria = DetectCategory(knowledgeJson, tagsJson),
                Forrasos = sourceBlocksJson != "[]",
                ExamInspired = GetBool(question, "examInspired"),
                Sorszam = index + 1,
                InteractionJson = GetRawJson(question, "interaction", "{}"),
                SourceBlocksJson = sourceBlocksJson,
                KnowledgeElementsJson = knowledgeJson,
                TagsJson = tagsJson,
                RawJson = question.GetRawText(),
                Aktiv = true
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        _logger.LogInformation("Emelt kérdésbank import kész: {QuestionCount} kérdés.", questions.Count);
    }

    private async Task EnsureTablesAsync(CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
CREATE TABLE IF NOT EXISTS emelt_kerdesbank_temak (
  id INT NOT NULL AUTO_INCREMENT,
  era_id VARCHAR(80) NOT NULL,
  cim VARCHAR(255) NOT NULL,
  kovetelmeny_tartomany VARCHAR(100) NULL,
  sorszam INT NOT NULL DEFAULT 0,
  aktiv TINYINT(1) NOT NULL DEFAULT 1,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY ix_emelt_kerdesbank_temak_era_id (era_id),
  KEY ix_emelt_kerdesbank_temak_sorszam (sorszam)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_hungarian_ci;
""",
            """
CREATE TABLE IF NOT EXISTS emelt_kerdesbank_resztemak (
  id INT NOT NULL AUTO_INCREMENT,
  tema_id INT NOT NULL,
  requirement_ref VARCHAR(80) NOT NULL,
  cim VARCHAR(255) NOT NULL,
  scope VARCHAR(40) NOT NULL DEFAULT 'vegyes',
  sorszam INT NOT NULL DEFAULT 0,
  aktiv TINYINT(1) NOT NULL DEFAULT 1,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY ix_emelt_kerdesbank_resztemak_unique (tema_id, requirement_ref, cim),
  CONSTRAINT fk_emelt_resztemak_temak FOREIGN KEY (tema_id) REFERENCES emelt_kerdesbank_temak(id) ON DELETE CASCADE
) CHARACTER SET utf8mb4 COLLATE utf8mb4_hungarian_ci;
""",
            """
CREATE TABLE IF NOT EXISTS emelt_kerdesbank_kerdesek (
  id INT NOT NULL AUTO_INCREMENT,
  tema_id INT NOT NULL,
  resztema_id INT NOT NULL,
  kulso_azonosito VARCHAR(80) NOT NULL,
  kerdes_tipus VARCHAR(80) NOT NULL,
  kerdes_szoveg TEXT NOT NULL,
  instrukcio TEXT NULL,
  magyarazat TEXT NULL,
  nehezseg VARCHAR(30) NOT NULL DEFAULT 'hard',
  scope VARCHAR(40) NOT NULL DEFAULT 'vegyes',
  kategoria VARCHAR(40) NOT NULL DEFAULT 'vegyes',
  forrasos TINYINT(1) NOT NULL DEFAULT 0,
  exam_inspired TINYINT(1) NOT NULL DEFAULT 0,
  sorszam INT NOT NULL DEFAULT 0,
  interaction_json JSON NOT NULL,
  source_blocks_json JSON NOT NULL,
  knowledge_elements_json JSON NOT NULL,
  tags_json JSON NOT NULL,
  raw_json JSON NOT NULL,
  aktiv TINYINT(1) NOT NULL DEFAULT 1,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY ix_emelt_kerdesbank_kerdesek_kulso (kulso_azonosito),
  KEY ix_emelt_kerdesbank_kerdesek_tema (tema_id),
  KEY ix_emelt_kerdesbank_kerdesek_resztema (resztema_id),
  KEY ix_emelt_kerdesbank_kerdesek_tipus (kerdes_tipus),
  KEY ix_emelt_kerdesbank_kerdesek_kategoria (kategoria),
  KEY ix_emelt_kerdesbank_kerdesek_forrasos (forrasos),
  CONSTRAINT fk_emelt_kerdesek_temak FOREIGN KEY (tema_id) REFERENCES emelt_kerdesbank_temak(id) ON DELETE CASCADE,
  CONSTRAINT fk_emelt_kerdesek_resztemak FOREIGN KEY (resztema_id) REFERENCES emelt_kerdesbank_resztemak(id) ON DELETE CASCADE
) CHARACTER SET utf8mb4 COLLATE utf8mb4_hungarian_ci;
"""
        };

        foreach (var statement in statements)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }

    private static string BuildSubtopicKey(string eraId, string requirementRef, string title) => $"{eraId}|{requirementRef}|{title}";

    private static string GetString(JsonElement element, string name, string fallback)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? fallback
            : fallback;
    }

    private static string? GetNullableString(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static bool GetBool(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;
    }

    private static string GetRawJson(JsonElement element, string name, string fallback)
    {
        return element.TryGetProperty(name, out var value) ? value.GetRawText() : fallback;
    }

    private static string DetectCategory(string knowledgeJson, string tagsJson)
    {
        using var knowledge = JsonDocument.Parse(knowledgeJson);
        var root = knowledge.RootElement;
        if (HasArrayItems(root, "dates")) return "evszam";
        if (HasArrayItems(root, "persons")) return "szemely";
        if (HasArrayItems(root, "places")) return "helyszin";
        if (HasArrayItems(root, "concepts")) return "fogalom";
        return tagsJson.Contains("forrás", StringComparison.OrdinalIgnoreCase) ? "forras" : "vegyes";
    }

    private static bool HasArrayItems(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var value) &&
            value.ValueKind == JsonValueKind.Array &&
            value.GetArrayLength() > 0;
    }
}
