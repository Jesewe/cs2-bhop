using System;
using Bhop.Core;
using Bhop.Features;
using Bhop.Utils;

namespace Bhop
{
    class Program
    {
        private const string VERSION = "1.1.0";
        private static Memory cs2;

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║         CS2 Bhop Utility v{0}      ║", VERSION);
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.ResetColor();

            Updater.CheckForUpdates(VERSION);
            OffsetFetcher.SmartReinstallDumper();

            Console.WriteLine("\nRetrieving offsets...");
            
            if (Offsets.dwForceJump == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to obtain offsets!");
                Console.ResetColor();
                Console.WriteLine("Press ENTER to exit...");
                Console.ReadLine();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"dwForceJump: 0x{Offsets.dwForceJump:X}");
            Console.WriteLine($"dwLocalPlayerPawn: 0x{Offsets.dwLocalPlayerPawn:X}");
            Console.WriteLine($"m_fFlags: 0x{Offsets.m_fFlags:X}");
            Console.ResetColor();

            while (true)
            {
                Console.WriteLine("Searching for cs2.exe process...");
                cs2 = new Memory("cs2.exe");
                if (cs2.IsValid)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Found cs2.exe process.");
                    Console.ResetColor();

                    cs2.ClientBase = cs2.GetModuleBaseAddress("client.dll");
                    if (cs2.ClientBase == IntPtr.Zero)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Could not locate client.dll module!");
                        Console.ResetColor();
                        Console.WriteLine("Press ENTER to exit...");
                        Console.ReadLine();
                        return;
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"client.dll base address: 0x{cs2.ClientBase.ToInt64():X}");
                    Console.ResetColor();
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
                        return;
                    }
                    else if (keyInfo.Key == ConsoleKey.R)
                    {
                        Console.Clear();
                        Console.WriteLine("Retrying to locate cs2.exe process...");
                    }
                }
            }

            var bhopLogic = new BhopLogic(cs2);
            bhopLogic.Run();
        }
    }
}
