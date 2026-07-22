using System;
using System.Runtime.InteropServices;
using System.Threading;
using Bhop.Core;

namespace Bhop.Features
{
    public class BhopLogic
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        private static extern uint timeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
        private static extern uint timeEndPeriod(uint uMilliseconds);

        private const int VK_SPACE = 0x20;
        private const int FORCE_JUMP_ACTIVE = 65537;
        private const int FORCE_JUMP_INACTIVE = 256;
        private const int MAIN_LOOP_SLEEP = 1;

        private bool wasJumping = false;
        private readonly Memory cs2;

        public BhopLogic(Memory memory)
        {
            this.cs2 = memory;
        }

        public void Run()
        {
            // Request 1ms timer resolution from Windows OS to prevent Thread.Sleep(1) from taking 15.6ms
            timeBeginPeriod(1);

            Console.WriteLine("Press SPACE to perform bhop...");
            Console.WriteLine("Bhop active - will only work when CS2 is the active window.");

            IntPtr forceJumpAddress = cs2.ClientBase + Offsets.dwForceJump;

            try
            {
                while (true)
                {
                    try
                    {
                        if (!IsCS2Active())
                        {
                            if (wasJumping)
                            {
                                cs2.WriteMemory(forceJumpAddress, FORCE_JUMP_INACTIVE);
                                wasJumping = false;
                            }
                            Thread.Sleep(MAIN_LOOP_SLEEP);
                            continue;
                        }

                        bool keyPressed = (GetAsyncKeyState(VK_SPACE) & 0x8000) != 0;

                        if (keyPressed)
                        {
                            long localPawn = cs2.ReadMemory<long>(cs2.ClientBase + Offsets.dwLocalPlayerPawn);
                            if (localPawn != 0)
                            {
                                int flags = cs2.ReadMemory<int>((IntPtr)(localPawn + Offsets.m_fFlags));
                                bool onGround = (flags & 1) != 0;

                                if (onGround)
                                {
                                    cs2.WriteMemory(forceJumpAddress, FORCE_JUMP_ACTIVE);
                                }
                                else
                                {
                                    cs2.WriteMemory(forceJumpAddress, FORCE_JUMP_INACTIVE);
                                }
                            }

                            wasJumping = true;
                            Thread.Sleep(1);
                        }
                        else
                        {
                            if (wasJumping)
                            {
                                cs2.WriteMemory(forceJumpAddress, FORCE_JUMP_INACTIVE);
                                wasJumping = false;
                            }
                            Thread.Sleep(MAIN_LOOP_SLEEP);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error in main loop: {ex.Message}");
                        Console.ResetColor();

                        if (cs2.Process.HasExited)
                        {
                            Console.WriteLine("CS2 process has exited. Press ENTER to close.");
                            Console.ReadLine();
                            return;
                        }

                        Thread.Sleep(MAIN_LOOP_SLEEP);
                    }
                }
            }
            finally
            {
                timeEndPeriod(1);
            }
        }

        private bool IsCS2Active()
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                    return false;

                int processId;
                GetWindowThreadProcessId(foregroundWindow, out processId);

                return processId == cs2.Process.Id;
            }
            catch
            {
                return false;
            }
        }
    }
}
