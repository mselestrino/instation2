using System.Net.Http;
using System.Text.Json;

namespace Instation2;

public static class GameCatalog
{
    private const string CatalogUrl = "https://raw.githubusercontent.com/mselestrino/instation2/main/catalog/games.json";
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(8) };
    private static readonly string CacheFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Instation2", "cache");
    private static readonly string CacheFile = Path.Combine(CacheFolder, "games.json");
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task<(IReadOnlyList<GameEntry> Games, string Message)> EnrichAsync(
        IReadOnlyList<GameEntry> games, CancellationToken cancellationToken = default)
    {
        List<GameMetadata> entries;
        var downloaded = false;

        try
        {
            var json = await Http.GetStringAsync(CatalogUrl, cancellationToken);
            entries = JsonSerializer.Deserialize<List<GameMetadata>>(json, JsonOptions) ?? [];
            Directory.CreateDirectory(CacheFolder);
            await File.WriteAllTextAsync(CacheFile, json, cancellationToken);
            downloaded = true;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException or JsonException)
        {
            try
            {
                if (!File.Exists(CacheFile))
                    return (games, "Catálogo online indisponível; biblioteca local mantida.");
                var json = await File.ReadAllTextAsync(CacheFile, cancellationToken);
                entries = JsonSerializer.Deserialize<List<GameMetadata>>(json, JsonOptions) ?? [];
            }
            catch (Exception cacheEx) when (cacheEx is IOException or JsonException or TaskCanceledException)
            {
                return (games, "Não foi possível acessar o catálogo; biblioteca local mantida.");
            }
        }

        var lookup = new Dictionary<string, GameMetadata>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in entries.Where(item => !string.IsNullOrWhiteSpace(item.Title)))
        {
            foreach (var name in entry.Aliases.Append(entry.Title))
            {
                var key = Normalize(name);
                if (key.Length > 0 && !lookup.ContainsKey(key))
                    lookup[key] = entry;
            }
        }

        var result = new List<GameEntry>(games.Count);
        foreach (var game in games)
        {
            var key = Normalize(Path.GetFileNameWithoutExtension(game.FilePath));
            if (!lookup.TryGetValue(key, out var metadata))
            {
                result.Add(game);
                continue;
            }

            var coverPath = game.CoverPath;
            if (string.IsNullOrWhiteSpace(coverPath) && !string.IsNullOrWhiteSpace(metadata.CoverUrl))
                coverPath = await DownloadCoverAsync(metadata, cancellationToken);

            result.Add(game with { Metadata = metadata, CoverPath = coverPath });
        }

        var matched = result.Count(game => game.Metadata is not null);
        return (result, downloaded
            ? $"Catálogo atualizado: {matched} jogo(s) identificado(s)."
            : $"Catálogo em cache: {matched} jogo(s) identificado(s).");
    }

    private static async Task<string?> DownloadCoverAsync(GameMetadata metadata, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(metadata.CoverUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            return null;

        var extension = Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            extension = ".jpg";

        var fileName = Normalize(metadata.Title);
        if (fileName.Length == 0)
            return null;

        try
        {
            Directory.CreateDirectory(CacheFolder);
            var destination = Path.Combine(CacheFolder, fileName + extension);
            if (File.Exists(destination))
                return destination;

            var bytes = await Http.GetByteArrayAsync(uri, cancellationToken);
            if (bytes.Length == 0 || bytes.Length > 12 * 1024 * 1024)
                return null;

            await File.WriteAllBytesAsync(destination, bytes, cancellationToken);
            return destination;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException)
        {
            return null;
        }
    }

    private static string Normalize(string value) =>
        new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
}