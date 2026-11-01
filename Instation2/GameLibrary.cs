using System.IO;

namespace Instation2;

public sealed class GameMetadata
{
    public string Title { get; set; } = "";
    public string[] Aliases { get; set; } = [];
    public string? Serial { get; set; }
    public string? Region { get; set; }
    public string? Description { get; set; }
    public string? CoverUrl { get; set; }
}

public sealed record GameEntry(string Name, string FilePath, long SizeBytes, string? CoverPath, GameMetadata? Metadata = null)
{
    public string DisplayName => Metadata?.Title ?? Name;
    public string SizeLabel => SizeBytes >= 1024L * 1024 * 1024
        ? $"{SizeBytes / (1024d * 1024 * 1024):0.0} GB"
        : $"{SizeBytes / (1024d * 1024):0} MB";
}

public static class GameLibrary
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".iso", ".chd", ".cso", ".bin", ".gz", ".elf"
    };

    private static readonly string[] CoverExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public static IReadOnlyList<GameEntry> Scan(IEnumerable<string> folders)
    {
        var games = new List<GameEntry>();
        foreach (var folder in folders.Where(Directory.Exists).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
                {
                    if (!SupportedExtensions.Contains(Path.GetExtension(file)))
                        continue;

                    try
                    {
                        var info = new FileInfo(file);
                        games.Add(new GameEntry(
                            Path.GetFileNameWithoutExtension(file),
                            info.FullName,
                            info.Length,
                            FindCover(info)));
                    }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        return games.OrderBy(game => game.Name, StringComparer.CurrentCultureIgnoreCase).ToArray();
    }

    private static string? FindCover(FileInfo gameFile)
    {
        foreach (var extension in CoverExtensions)
        {
            var sibling = Path.ChangeExtension(gameFile.FullName, extension);
            if (File.Exists(sibling))
                return sibling;
        }

        var coverFolder = Path.Combine(gameFile.DirectoryName!, "covers");
        if (Directory.Exists(coverFolder))
        {
            foreach (var extension in CoverExtensions)
            {
                var cover = Path.Combine(coverFolder, gameFile.Name + extension);
                if (File.Exists(cover))
                    return cover;

                cover = Path.Combine(coverFolder, Path.GetFileNameWithoutExtension(gameFile.Name) + extension);
                if (File.Exists(cover))
                    return cover;
            }
        }

        return null;
    }
}