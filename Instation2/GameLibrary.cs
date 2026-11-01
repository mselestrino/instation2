using System.IO;

namespace Instation2;

public sealed record GameEntry(string Name, string FilePath, long SizeBytes)
{
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
                        games.Add(new GameEntry(Path.GetFileNameWithoutExtension(file), info.FullName, info.Length));
                    }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        return games
            .OrderBy(game => game.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }
}