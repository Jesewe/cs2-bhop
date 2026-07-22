using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bhop.Utils
{
    public static class OffsetFetcher
    {
        private const string GITHUB_RELEASES_URL = "https://api.github.com/repos/{0}/releases/latest";
        private const string CS2_DUMPER_REPO = "a2x/cs2-dumper";
        private const string CS2_DUMPER_EXE_NAME = "cs2-dumper.exe";
        private const int SUBPROCESS_TIMEOUT_MS = 120000;

        private static readonly string ConfigDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Requests",
            "ItsJesewe"
        );

        private static readonly string Cs2DumperExePath = Path.Combine(ConfigDirectory, CS2_DUMPER_EXE_NAME);
        private static readonly string CacheFile = Path.Combine(ConfigDirectory, "cs2_patch_cache.txt");

        private static readonly HttpClient httpClient;

        static OffsetFetcher()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "VioletWing-App");
        }

        public static async Task FetchCs2LatestPatchAsync()
        {
            string url = "https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=730&count=1&maxlength=1&format=json";

            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    
                    string jsonContent = await response.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                    {
                        var root = doc.RootElement;
                        if (root.TryGetProperty("appnews", out var appNews) && 
                            appNews.TryGetProperty("newsitems", out var newsItems) && 
                            newsItems.GetArrayLength() > 0)
                        {
                            var firstItem = newsItems[0];
                            if (firstItem.TryGetProperty("date", out var dateElement))
                            {
                                long unixDate = dateElement.GetInt64();
                                DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixDate).LocalDateTime;
                                string dateStr = date.ToString("MM/dd/yyyy");

                                Directory.CreateDirectory(ConfigDirectory);
                                File.WriteAllText(CacheFile, dateStr);

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"CS2 Patch Date: {dateStr}");
                                Console.ResetColor();
                                return;
                            }
                        }
                    }
                    throw new Exception("No news items in Steam API response");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed to fetch CS2 patch date (attempt {attempt + 1}): {ex.Message}");
                    Console.ResetColor();
                    if (attempt < 2)
                    {
                        await Task.Delay(5000);
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error fetching CS2 patch date.");
            Console.ResetColor();
        }

        public static void SmartReinstallDumper()
        {
            if (!File.Exists(CacheFile) || !File.Exists(Cs2DumperExePath))
            {
                return;
            }

            try
            {
                string dateStr = File.ReadAllText(CacheFile).Trim();
                if (DateTime.TryParseExact(dateStr, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime patchDate))
                {
                    DateTime dumperDate = File.GetLastWriteTime(Cs2DumperExePath).Date;
                    if (patchDate > dumperDate)
                    {
                        Console.WriteLine($"CS2 patch ({patchDate:d}) is newer than cs2-dumper.exe ({dumperDate:d}). Forcing re-download.");
                        File.Delete(Cs2DumperExePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Failed to evaluate smart reinstall for cs2-dumper: {ex.Message}");
                Console.ResetColor();
            }
        }

        public static bool EnsureBinary()
        {
            if (File.Exists(Cs2DumperExePath))
            {
                return true;
            }
            return DownloadCs2Dumper();
        }

        private static bool DownloadCs2Dumper()
        {
            string apiUrl = string.Format(GITHUB_RELEASES_URL, CS2_DUMPER_REPO);
            try
            {
                Console.WriteLine($"Fetching cs2-dumper release info from {apiUrl}");
                
                using (var wc = new System.Net.WebClient())
                {
                    wc.Headers.Add("User-Agent", "VioletWing-App");
                    wc.Headers.Add("Accept", "application/vnd.github+json");
                    string json = wc.DownloadString(apiUrl);
                    
                    using (JsonDocument doc = JsonDocument.Parse(json))
                    {
                        var root = doc.RootElement;
                        string downloadUrl = null;
                        
                        if (root.TryGetProperty("assets", out var assets))
                        {
                            foreach (var asset in assets.EnumerateArray())
                            {
                                if (asset.TryGetProperty("name", out var nameElement) && 
                                    nameElement.GetString()?.Equals(CS2_DUMPER_EXE_NAME, StringComparison.OrdinalIgnoreCase) == true)
                                {
                                    if (asset.TryGetProperty("browser_download_url", out var urlElement))
                                    {
                                        downloadUrl = urlElement.GetString();
                                        break;
                                    }
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(downloadUrl))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Could not find cs2-dumper.exe asset in the latest release.");
                            Console.ResetColor();
                            return false;
                        }

                        Console.WriteLine($"Downloading cs2-dumper from {downloadUrl}");
                        byte[] binary = wc.DownloadData(downloadUrl);
                        Directory.CreateDirectory(ConfigDirectory);
                        File.WriteAllBytes(Cs2DumperExePath, binary);
                        
                        Console.WriteLine($"cs2-dumper saved to {Cs2DumperExePath} ({binary.Length} bytes).");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error downloading cs2-dumper: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        public static bool RunCs2Dumper(string outputDir)
        {
            Console.WriteLine($"Running cs2-dumper: {Cs2DumperExePath} -o \"{outputDir}\" -f json");

            try
            {
                // ArgumentList populates argv[] entries directly, so paths with spaces are safe.
                var processInfo = new ProcessStartInfo
                {
                    FileName = Cs2DumperExePath,
                    WorkingDirectory = outputDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                processInfo.ArgumentList.Add("-o");
                processInfo.ArgumentList.Add(outputDir);
                processInfo.ArgumentList.Add("-f");
                processInfo.ArgumentList.Add("json");

                using (var process = Process.Start(processInfo))
                {
                    if (process == null)
                    {
                        Console.WriteLine("Failed to start cs2-dumper process.");
                        return false;
                    }

                    string stdout = process.StandardOutput.ReadToEnd();
                    string stderr = process.StandardError.ReadToEnd();
                    
                    bool exited = process.WaitForExit(SUBPROCESS_TIMEOUT_MS);
                    if (!exited)
                    {
                        process.Kill();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("cs2-dumper timed out.");
                        Console.ResetColor();
                        return false;
                    }
                    
                    if (process.ExitCode != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"cs2-dumper exited with code {process.ExitCode}");
                        if (!string.IsNullOrEmpty(stderr))
                        {
                            Console.WriteLine(stderr);
                        }
                        Console.ResetColor();
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error executing cs2-dumper: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }
    }
}
