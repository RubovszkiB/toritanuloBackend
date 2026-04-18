using System.Text;
using System.Text.RegularExpressions;

namespace toritanulo.Helpers;

public static class QuizAnswerNormalizer
{
    public static string NormalizeYear(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return new string(value.Where(char.IsDigit).ToArray());
    }

    public static string NormalizeLooseText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var lowered = value.Trim().ToLowerInvariant();

        lowered = lowered
            .Replace("kr. e.", string.Empty)
            .Replace("kr.e.", string.Empty)
            .Replace("kr e ", string.Empty)
            .Replace("kr. u.", string.Empty)
            .Replace("kr.u.", string.Empty)
            .Replace("kr u ", string.Empty);

        var builder = new StringBuilder(lowered.Length);
        foreach (var ch in lowered)
        {
            if (char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch))
            {
                builder.Append(ch);
            }
        }

        return Regex.Replace(builder.ToString(), @"\s+", " ").Trim();
    }
}
