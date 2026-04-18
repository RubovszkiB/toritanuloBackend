using System.Text.Json;
using toritanulo.Models;

namespace toritanulo.Helpers;

public class QuizEvaluationResult
{
    public bool Helyes { get; set; }
    public int Pontszam { get; set; }
    public string HelyesValaszOsszegzes { get; set; } = string.Empty;
}

public static class QuizEvaluator
{
    public static QuizEvaluationResult Evaluate(Kerdes kerdes, int elerhetoPont, string? valaszSzoveg, JsonElement? valaszJson)
    {
        var tipus = kerdes.KerdesTipus.Kod;

        return tipus switch
        {
            "single_choice" => EvaluateChoice(kerdes, elerhetoPont, valaszSzoveg, valaszJson, allowMultiple: false),
            "true_false" => EvaluateChoice(kerdes, elerhetoPont, valaszSzoveg, valaszJson, allowMultiple: false),
            "multi_choice" => EvaluateChoice(kerdes, elerhetoPont, valaszSzoveg, valaszJson, allowMultiple: true),
            "year_input" => EvaluateYearInput(kerdes, elerhetoPont, valaszSzoveg),
            "chronology_order" => EvaluateChronology(kerdes, elerhetoPont, valaszSzoveg, valaszJson),
            "matching" => EvaluateMatching(kerdes, elerhetoPont, valaszSzoveg, valaszJson),
            _ => new QuizEvaluationResult
            {
                Helyes = false,
                Pontszam = 0,
                HelyesValaszOsszegzes = "Ismeretlen kérdéstípus."
            }
        };
    }

    private static QuizEvaluationResult EvaluateChoice(Kerdes kerdes, int elerhetoPont, string? valaszSzoveg, JsonElement? valaszJson, bool allowMultiple)
    {
        var correctOptionIds = kerdes.ValaszOpcioK
            .Where(x => x.Helyes)
            .OrderBy(x => x.Sorszam)
            .Select(x => x.Id)
            .ToList();

        var selectedOptionIds = ExtractIntList(valaszSzoveg, valaszJson);

        bool helyes;
        if (selectedOptionIds.Count > 0)
        {
            helyes = allowMultiple
                ? selectedOptionIds.OrderBy(x => x).SequenceEqual(correctOptionIds.OrderBy(x => x))
                : selectedOptionIds.Count == 1 && correctOptionIds.Count == 1 && selectedOptionIds[0] == correctOptionIds[0];
        }
        else
        {
            var selectedTexts = ExtractStringList(valaszSzoveg, valaszJson)
                .Select(QuizAnswerNormalizer.NormalizeLooseText)
                .OrderBy(x => x)
                .ToList();

            var correctTexts = kerdes.ValaszOpcioK
                .Where(x => x.Helyes)
                .Select(x => QuizAnswerNormalizer.NormalizeLooseText(x.ValaszSzoveg))
                .OrderBy(x => x)
                .ToList();

            helyes = selectedTexts.SequenceEqual(correctTexts);
        }

        return new QuizEvaluationResult
        {
            Helyes = helyes,
            Pontszam = helyes ? elerhetoPont : 0,
            HelyesValaszOsszegzes = string.Join("; ", kerdes.ValaszOpcioK.Where(x => x.Helyes).OrderBy(x => x.Sorszam).Select(x => x.ValaszSzoveg))
        };
    }

    private static QuizEvaluationResult EvaluateYearInput(Kerdes kerdes, int elerhetoPont, string? valaszSzoveg)
    {
        var normalized = QuizAnswerNormalizer.NormalizeYear(valaszSzoveg);

        var correctAnswers = kerdes.HelyesValaszok
            .Select(x => x.NormalizaltValasz)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var helyes = !string.IsNullOrWhiteSpace(normalized) && correctAnswers.Contains(normalized);

        return new QuizEvaluationResult
        {
            Helyes = helyes,
            Pontszam = helyes ? elerhetoPont : 0,
            HelyesValaszOsszegzes = string.Join("; ", kerdes.HelyesValaszok.Select(x => x.ValaszSzoveg ?? x.ValaszSzam?.ToString() ?? string.Empty))
        };
    }

    private static QuizEvaluationResult EvaluateChronology(Kerdes kerdes, int elerhetoPont, string? valaszSzoveg, JsonElement? valaszJson)
    {
        var expectedIds = kerdes.ValaszOpcioK
            .Where(x => x.HelyesSorrend.HasValue)
            .OrderBy(x => x.HelyesSorrend)
            .Select(x => x.Id)
            .ToList();

        var selectedIds = ExtractIntList(valaszSzoveg, valaszJson);

        bool helyes;
        if (selectedIds.Count > 0)
        {
            helyes = selectedIds.SequenceEqual(expectedIds);
        }
        else
        {
            var expectedTexts = kerdes.ValaszOpcioK
                .Where(x => x.HelyesSorrend.HasValue)
                .OrderBy(x => x.HelyesSorrend)
                .Select(x => QuizAnswerNormalizer.NormalizeLooseText(x.ValaszSzoveg))
                .ToList();

            var selectedTexts = ExtractStringList(valaszSzoveg, valaszJson)
                .Select(QuizAnswerNormalizer.NormalizeLooseText)
                .ToList();

            helyes = selectedTexts.SequenceEqual(expectedTexts);
        }

        return new QuizEvaluationResult
        {
            Helyes = helyes,
            Pontszam = helyes ? elerhetoPont : 0,
            HelyesValaszOsszegzes = string.Join(" → ", kerdes.ValaszOpcioK.Where(x => x.HelyesSorrend.HasValue).OrderBy(x => x.HelyesSorrend).Select(x => x.ValaszSzoveg))
        };
    }

    private static QuizEvaluationResult EvaluateMatching(Kerdes kerdes, int elerhetoPont, string? valaszSzoveg, JsonElement? valaszJson)
    {
        var expected = kerdes.Parok
            .OrderBy(x => x.Sorszam)
            .ToDictionary(
                x => QuizAnswerNormalizer.NormalizeLooseText(x.BalOldal),
                x => QuizAnswerNormalizer.NormalizeLooseText(x.JobbOldal));

        var submitted = ExtractPairs(valaszSzoveg, valaszJson);

        var helyes = expected.Count > 0 &&
                     expected.Count == submitted.Count &&
                     expected.All(kvp => submitted.TryGetValue(kvp.Key, out var value) && value == kvp.Value);

        return new QuizEvaluationResult
        {
            Helyes = helyes,
            Pontszam = helyes ? elerhetoPont : 0,
            HelyesValaszOsszegzes = string.Join("; ", kerdes.Parok.OrderBy(x => x.Sorszam).Select(x => $"{x.BalOldal} → {x.JobbOldal}"))
        };
    }

    private static List<int> ExtractIntList(string? valaszSzoveg, JsonElement? valaszJson)
    {
        var result = new List<int>();

        if (valaszJson.HasValue)
        {
            var element = valaszJson.Value;

            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var singleNumber))
            {
                result.Add(singleNumber);
                return result;
            }

            if (element.ValueKind == JsonValueKind.String)
            {
                if (int.TryParse(element.GetString(), out var singleFromString))
                {
                    result.Add(singleFromString);
                }

                return result;
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Number && item.TryGetInt32(out var number))
                    {
                        result.Add(number);
                    }
                    else if (item.ValueKind == JsonValueKind.String && int.TryParse(item.GetString(), out var strNumber))
                    {
                        result.Add(strNumber);
                    }
                }

                return result;
            }
        }

        if (!string.IsNullOrWhiteSpace(valaszSzoveg))
        {
            foreach (var chunk in valaszSzoveg.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (int.TryParse(chunk, out var number))
                {
                    result.Add(number);
                }
            }
        }

        return result;
    }

    private static List<string> ExtractStringList(string? valaszSzoveg, JsonElement? valaszJson)
    {
        var result = new List<string>();

        if (valaszJson.HasValue)
        {
            var element = valaszJson.Value;

            if (element.ValueKind == JsonValueKind.String)
            {
                result.Add(element.GetString() ?? string.Empty);
                return result;
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        result.Add(item.GetString() ?? string.Empty);
                    }
                }

                return result;
            }
        }

        if (!string.IsNullOrWhiteSpace(valaszSzoveg))
        {
            result.AddRange(valaszSzoveg.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }

        return result;
    }

    private static Dictionary<string, string> ExtractPairs(string? valaszSzoveg, JsonElement? valaszJson)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (valaszJson.HasValue)
        {
            var element = valaszJson.Value;

            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    var key = QuizAnswerNormalizer.NormalizeLooseText(property.Name);
                    var value = QuizAnswerNormalizer.NormalizeLooseText(property.Value.GetString());
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        result[key] = value;
                    }
                }

                return result;
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    string? bal = null;
                    string? jobb = null;

                    if (item.TryGetProperty("balOldal", out var balElement))
                    {
                        bal = balElement.GetString();
                    }

                    if (item.TryGetProperty("jobbOldal", out var jobbElement))
                    {
                        jobb = jobbElement.GetString();
                    }

                    if (item.TryGetProperty("left", out var leftElement))
                    {
                        bal ??= leftElement.GetString();
                    }

                    if (item.TryGetProperty("right", out var rightElement))
                    {
                        jobb ??= rightElement.GetString();
                    }

                    var normalizedBal = QuizAnswerNormalizer.NormalizeLooseText(bal);
                    if (!string.IsNullOrWhiteSpace(normalizedBal))
                    {
                        result[normalizedBal] = QuizAnswerNormalizer.NormalizeLooseText(jobb);
                    }
                }

                return result;
            }
        }

        if (!string.IsNullOrWhiteSpace(valaszSzoveg))
        {
            var pairs = valaszSzoveg.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var pair in pairs)
            {
                var parts = pair.Split("=>", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 2)
                {
                    var normalizedBal = QuizAnswerNormalizer.NormalizeLooseText(parts[0]);
                    if (!string.IsNullOrWhiteSpace(normalizedBal))
                    {
                        result[normalizedBal] = QuizAnswerNormalizer.NormalizeLooseText(parts[1]);
                    }
                }
            }
        }

        return result;
    }
}
