namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Moderation;

public record LocalFilterResult(string MatchedCategory, string MatchedTerm);

public interface ILocalWordListFilter
{
    LocalFilterResult? Check(string message);
}

public class LocalWordListFilter : ILocalWordListFilter
{
    private static readonly Dictionary<string, string[]> WordLists;

    static LocalWordListFilter()
    {
        var assembly = typeof(LocalWordListFilter).Assembly;
        var resourceName = "XtremeIdiots.Portal.Events.Ingest.App.V1.Moderation.wordlist.txt";

        WordLists = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            return;

        using var reader = new StreamReader(stream);
        var currentCategory = "General";

        while (reader.ReadLine() is { } line)
        {
            line = line.Trim();

            if (string.IsNullOrEmpty(line) || line.StartsWith('#'))
                continue;

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                currentCategory = line[1..^1];
                if (!WordLists.ContainsKey(currentCategory))
                    WordLists[currentCategory] = [];
                continue;
            }

            if (!WordLists.TryGetValue(currentCategory, out var existing))
            {
                existing = [];
                WordLists[currentCategory] = existing;
            }

            WordLists[currentCategory] = [.. existing, line.ToLowerInvariant()];
        }
    }

    public LocalFilterResult? Check(string message)
    {
        var normalised = message.ToLowerInvariant();

        foreach (var (category, terms) in WordLists)
        {
            foreach (var term in terms)
            {
                if (ContainsWord(normalised, term))
                    return new LocalFilterResult(category, term);
            }
        }

        return null;
    }

    private static bool ContainsWord(string text, string word)
    {
        var index = text.IndexOf(word, StringComparison.Ordinal);
        if (index < 0) return false;

        var before = index == 0 || !char.IsLetterOrDigit(text[index - 1]);
        var after = index + word.Length >= text.Length
            || !char.IsLetterOrDigit(text[index + word.Length]);

        return before && after;
    }
}
