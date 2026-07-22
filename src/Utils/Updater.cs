using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bhop.Utils
{
    public class Tag
    {
        public string name { get; set; }
    }

    [JsonSourceGenerationOptions(WriteIndented = false)]
    [JsonSerializable(typeof(Tag[]))]
    internal partial class SourceGenerationContext : JsonSerializerContext
    {
    }

    public static class Updater
    {
        public static void CheckForUpdates(string currentVersion)
        {
            try
            {
                Console.WriteLine("Checking for updates...");

                using (var wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent", "CS2-Bhop-Utility");
                    string json = wc.DownloadString("https://api.github.com/repos/Jesewe/cs2-bhop/tags");

                    var options = new JsonSerializerOptions
                    {
                        TypeInfoResolver = SourceGenerationContext.Default
                    };
                    var tags = JsonSerializer.Deserialize<Tag[]>(json, options);

                    if (tags != null && tags.Length > 0)
                    {
                        string latestVersion = tags[0].name;
                        if (latestVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                        {
                            latestVersion = latestVersion.Substring(1);
                        }

                        if (latestVersion != currentVersion)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Update available! Current version: {currentVersion}, Latest version: {latestVersion}");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("You are running the latest version.");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.WriteLine("No tags found in the repository.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Update check failed: " + ex.Message);
                Console.ResetColor();
            }
        }
    }
}
