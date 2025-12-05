using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace Bhop
{
    class Bhop
    {
        // Import GetAsyncKeyState from user32.dll to detect key presses.
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        // Import GetForegroundWindow to check active window
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        private const int VK_SPACE = 0x20;

        // Constants for bunnyhop
        private const int FORCE_JUMP_ACTIVE = 65537;
        private const int FORCE_JUMP_INACTIVE = 256;

        // Main loop sleep time for reduced CPU usage
        private const int MAIN_LOOP_SLEEP = 1; // 1ms for better timing precision

        // Jump state tracking
        private static bool jumpActive = false;
        private static DateTime lastActionTime = DateTime.MinValue;

        // Configuration
        private static int jumpDelayMs = 10; // Default jump delay in milliseconds

        // Memory object for the cs2.exe process.
        private static Memory cs2;

        // Current version constant.
        private const string VERSION = "1.0.5";

        static void Main(string[] args)
        {
            // Display colorful startup header.
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║         CS2 Bhop Utility v{0}      ║", VERSION);
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.ResetColor();

            // Check for updates via the GitHub API.
            CheckForUpdates();

            Console.WriteLine("\nRetrieving jump offset...");
            // The available offset is now automatically loaded via the static constructor of Offsets.
            if (Offsets.dwForceJump == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to obtain dwForceJump offset!");
                Console.ResetColor();
                // Wait for user to press enter before exiting.
                Console.WriteLine("Press ENTER to exit...");
                Console.ReadLine();
                return;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"dwForceJump: 0x{Offsets.dwForceJump:X}");
            Console.ResetColor();

            // Loop until the cs2.exe process is found or user decides to exit.
            while (true)
            {
                Console.WriteLine("Searching for cs2.exe process...");
                cs2 = new Memory("cs2.exe");
                if (cs2.IsValid)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Found cs2.exe process.");
                    Console.ResetColor();

                    // Retrieve the base address of the client.dll module.
                    cs2.ClientBase = cs2.GetModuleBaseAddress("client.dll");
                    if (cs2.ClientBase == IntPtr.Zero)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Could not locate client.dll module!");
                        Console.ResetColor();
                        // Exit or retry here if needed. For now, we exit after a key press.
                        Console.WriteLine("Press ENTER to exit...");
                        Console.ReadLine();
                        return;
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"client.dll base address: 0x{cs2.ClientBase.ToInt64():X}");
                    Console.ResetColor();

                    // If everything is set, break out of the loop.
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Could not find cs2.exe process!");
                    Console.ResetColor();
                    Console.WriteLine("Press ENTER to exit or R to retry.");
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        return;  // Exit the application.
                    }
                    else if (keyInfo.Key == ConsoleKey.R)
                    {
                        Console.Clear();
                        Console.WriteLine("Retrying to locate cs2.exe process...");
                    }
                }
            }

            Console.WriteLine("Press SPACE to perform bhop...");
            Console.WriteLine("Bhop active - will only work when CS2 is the active window.");

            IntPtr forceJumpAddress = cs2.ClientBase + Offsets.dwForceJump;

            // Main bhop loop with improved timing and state management
            while (true)
            {
                try
                {
                    // Check if CS2 is the active window
                    if (!IsCS2Active())
                    {
                        // Reset jump state when game is not active
                        if (jumpActive)
                        {
                            cs2.WriteMemory(forceJumpAddress, FORCE_JUMP_INACTIVE);
                            jumpActive = false;
                        }
                        Thread.Sleep(MAIN_LOOP_SLEEP);
                        continue;
                    }

                    DateTime currentTime = DateTime.Now;
                    bool keyPressed = (GetAsyncKeyState(VK_SPACE) & 0x8000) != 0;

                    if (keyPressed)
                    {
                        // Key is pressed - handle jump timing
                        TimeSpan timeSinceLastAction = currentTime - lastActionTime;

                        if (timeSinceLastAction.TotalMilliseconds >= jumpDelayMs)
                        {
                            if (!jumpActive)
                            {
                                // Activate jump
                                cs2.WriteMemory(forceJumpAddress, FORCE_JUMP_ACTIVE);
                                jumpActive = true;
                                lastActionTime = currentTime;
                            }
                            else
                            {
                                // Deactivate jump
                                cs2.WriteMemory(forceJumpAddress, FORCE_JUMP_INACTIVE);
                                jumpActive = false;
                                lastActionTime = currentTime;
                            }
                        }
                    }
                    else
                    {
                        // Key not pressed - ensure jump is inactive
                        if (jumpActive)
                        {
                            cs2.WriteMemory(forceJumpAddress, FORCE_JUMP_INACTIVE);
                            jumpActive = false;
                        }
                    }

                    Thread.Sleep(MAIN_LOOP_SLEEP);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error in main loop: {ex.Message}");
                    Console.ResetColor();

                    // Check if process is still valid
                    if (cs2.process.HasExited)
                    {
                        Console.WriteLine("CS2 process has exited. Press ENTER to close.");
                        Console.ReadLine();
                        return;
                    }

                    Thread.Sleep(MAIN_LOOP_SLEEP);
                }
            }
        }

        // Check if CS2 is the currently active window
        private static bool IsCS2Active()
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                    return false;

                int processId;
                GetWindowThreadProcessId(foregroundWindow, out processId);

                return processId == cs2.process.Id;
            }
            catch
            {
                return false;
            }
        }

        // Checks for updates using the GitHub API.
        private static void CheckForUpdates()
        {
            try
            {
                Console.WriteLine("Checking for updates...");

                using (var wc = new WebClient())
                {
                    // GitHub requires a User-Agent header.
                    wc.Headers.Add("User-Agent", "CS2-Bhop-Utility");
                    string json = wc.DownloadString("https://api.github.com/repos/Jesewe/cs2-bhop/tags");

                    // Parse JSON using source generator context
                    var options = new JsonSerializerOptions
                    {
                        TypeInfoResolver = SourceGenerationContext.Default
                    };
                    var tags = JsonSerializer.Deserialize<Tag[]>(json, options);

                    if (tags != null && tags.Length > 0)
                    {
                        // Assume the first tag is the latest.
                        string latestVersion = tags[0].name;
                        if (latestVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                        {
                            latestVersion = latestVersion.Substring(1);
                        }

                        if (latestVersion != VERSION)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Update available! Current version: {VERSION}, Latest version: {latestVersion}");
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

        // Class mapping JSON response for GitHub tags.
        public class Tag
        {
            public string name { get; set; }
        }
    }

    // JSON Source Generator Context
    [JsonSourceGenerationOptions(WriteIndented = false)]
    [JsonSerializable(typeof(Bhop.Tag[]))]
    internal partial class SourceGenerationContext : JsonSerializerContext
    {
    }

    /// <summary>
    /// Static class Offsets.
    /// The jump offset is loaded automatically from a remote file.
    /// </summary>
    static class Offsets
    {
        public static int dwForceJump { get; private set; }

        // Static constructor runs once upon first access to the class.
        static Offsets()
        {
            try
            {
                using (var wc = new WebClient())
                {
                    string url = "https://raw.githubusercontent.com/a2x/cs2-dumper/refs/heads/main/output/buttons.hpp";
                    string content = wc.DownloadString(url);
                    // Look for the jump offset line, e.g.:
                    //    constexpr std::ptrdiff_t jump = 0x186CD60;
                    Match match = Regex.Match(content, @"constexpr\s+std::ptrdiff_t\s+jump\s*=\s*(0x[0-9A-Fa-f]+);");
                    if (match.Success)
                    {
                        string hexValue = match.Groups[1].Value;
                        dwForceJump = Convert.ToInt32(hexValue, 16);
                    }
                    else
                    {
                        throw new Exception("Failed to find jump offset in the file.");
                    }
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

    /// <summary>
    /// Memory class for process memory operations.
    /// </summary>
    class Memory
    {
        public IntPtr ProcessHandle { get; private set; }
        public IntPtr ClientBase { get; set; }
        public Process process { get; private set; }

        public bool IsValid { get; private set; }

        // Constructor attempts to find the process by name (e.g., "cs2.exe").
        public Memory(string processName)
        {
            Process[] processes = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(processName));
            if (processes.Length > 0)
            {
                process = processes[0];
                ProcessHandle = process.Handle;
                IsValid = true;
            }
            else
            {
                IsValid = false;
            }
        }

        // Retrieves the base address of the specified module.
        public IntPtr GetModuleBaseAddress(string moduleName)
        {
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    return module.BaseAddress;
                }
            }
            return IntPtr.Zero;
        }

        // Generic method to write a value of type T into process memory at a given address.
        public void WriteMemory<T>(IntPtr address, T value)
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];

            // Convert the value to a byte array.
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, false);
            Marshal.Copy(ptr, buffer, 0, size);
            Marshal.FreeHGlobal(ptr);

            int bytesWritten;
            WriteProcessMemory(ProcessHandle, address, buffer, buffer.Length, out bytesWritten);
        }

        // Import WriteProcessMemory from kernel32.dll to write into another process's memory.
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int dwSize,
            out int lpNumberOfBytesWritten);
    }
}