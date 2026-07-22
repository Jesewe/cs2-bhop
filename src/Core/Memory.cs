using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bhop.Core
{
    public class Memory
    {
        public IntPtr ProcessHandle { get; private set; }
        public IntPtr ClientBase { get; set; }
        public Process Process { get; private set; }

        public bool IsValid { get; private set; }

        public Memory(string processName)
        {
            Process[] processes = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(processName));
            if (processes.Length > 0)
            {
                Process = processes[0];
                ProcessHandle = Process.Handle;
                IsValid = true;
            }
            else
            {
                IsValid = false;
            }
        }

        public IntPtr GetModuleBaseAddress(string moduleName)
        {
            foreach (ProcessModule module in Process.Modules)
            {
                if (module.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    return module.BaseAddress;
                }
            }
            return IntPtr.Zero;
        }

        public void WriteMemory<T>(IntPtr address, T value)
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, false);
            Marshal.Copy(ptr, buffer, 0, size);
            Marshal.FreeHGlobal(ptr);

            int bytesWritten;
            WriteProcessMemory(ProcessHandle, address, buffer, buffer.Length, out bytesWritten);
        }

        public T ReadMemory<T>(IntPtr address) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];

            int bytesRead;
            ReadProcessMemory(ProcessHandle, address, buffer, buffer.Length, out bytesRead);

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                T obj = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                return obj;
            }
            finally
            {
                handle.Free();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int dwSize,
            out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead);
    }
}
