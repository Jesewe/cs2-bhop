using System;
using System.IO;
using System.Text.Json;
using Bhop.Utils;

namespace Bhop.Core
{
    public static class Offsets
    {
        public static int dwForceJump { get; private set; }
        public static int dwLocalPlayerPawn { get; private set; }
        public static int m_fFlags { get; private set; }

        private static readonly string DumperOutputDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Requests",
            "ItsJesewe",
            "dumper_output"
        );

        static Offsets()
        {
            try
            {
                if (!OffsetFetcher.EnsureBinary())
                {
                    throw new Exception("Failed to download or find cs2-dumper.");
                }

                Directory.CreateDirectory(DumperOutputDir);

                if (!OffsetFetcher.RunCs2Dumper(DumperOutputDir))
                {
                    throw new Exception("Failed to run cs2-dumper.");
                }

                string buttonsPath = Path.Combine(DumperOutputDir, "buttons.json");
                if (File.Exists(buttonsPath))
                {
                    using (JsonDocument doc = JsonDocument.Parse(File.ReadAllText(buttonsPath)))
                    {
                        dwForceJump = doc.RootElement.GetProperty("client.dll").GetProperty("jump").GetInt32();
                    }
                }
                else
                {
                    throw new Exception("buttons.json not found.");
                }

                string offsetsPath = Path.Combine(DumperOutputDir, "offsets.json");
                if (File.Exists(offsetsPath))
                {
                    using (JsonDocument doc = JsonDocument.Parse(File.ReadAllText(offsetsPath)))
                    {
                        dwLocalPlayerPawn = doc.RootElement.GetProperty("client.dll").GetProperty("dwLocalPlayerPawn").GetInt32();
                    }
                }
                else
                {
                    throw new Exception("offsets.json not found.");
                }

                string clientDllPath = Path.Combine(DumperOutputDir, "client_dll.json");
                if (File.Exists(clientDllPath))
                {
                    using (JsonDocument doc = JsonDocument.Parse(File.ReadAllText(clientDllPath)))
                    {
                        m_fFlags = doc.RootElement.GetProperty("client.dll")
                            .GetProperty("classes")
                            .GetProperty("C_BaseEntity")
                            .GetProperty("fields")
                            .GetProperty("m_fFlags")
                            .GetInt32();
                    }
                }
                else
                {
                    throw new Exception("client_dll.json not found.");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error retrieving offset: " + ex.Message);
                Console.ResetColor();
                dwForceJump = 0;
            }
        }
    }
}
