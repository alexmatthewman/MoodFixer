using System.IO;
using System.Text.Json;

namespace AIRelief.Tests.Localization;

public class LocaleJsonValidationTests
{
    private static string GetLocalesPath()
    {
        // Walk up from the test bin directory to the solution root, then into the main project.
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "AIRelief.sln")))
            dir = dir.Parent;

        Assert.NotNull(dir);
        var localesPath = Path.Combine(dir!.FullName, "AIRelief", "LanguageResources", "Locales");
        Assert.True(Directory.Exists(localesPath), $"Locales directory not found at {localesPath}");
        return localesPath;
    }

    [Fact]
    public void AllLocaleFiles_ShouldBeValidJson()
    {
        var localesPath = GetLocalesPath();
        var files = Directory.GetFiles(localesPath, "*.json", SearchOption.AllDirectories);

        Assert.NotEmpty(files);

        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(localesPath, file);
            var json = File.ReadAllText(file);

            var ex = Record.Exception(() =>
            {
                using var doc = JsonDocument.Parse(json);
            });

            Assert.True(ex is null,
                $"Invalid JSON in '{relativePath}': {ex?.Message}");
        }
    }

    [Fact]
    public void AllLocaleFiles_ShouldDeserializeToFlatStringDictionary()
    {
        var localesPath = GetLocalesPath();
        var files = Directory.GetFiles(localesPath, "*.json", SearchOption.AllDirectories);

        Assert.NotEmpty(files);

        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(localesPath, file);
            var json = File.ReadAllText(file);

            var ex = Record.Exception(() =>
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                Assert.NotNull(dict);
            });

            Assert.True(ex is null,
                $"Failed to deserialize '{relativePath}' as Dictionary<string, string>: {ex?.Message}");
        }
    }
}
