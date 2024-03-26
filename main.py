import pymem, pymem.process, time, ctypes
from colorama import Fore, init

init(autoreset=True)

ctypes.windll.kernel32.SetConsoleTitleW("CS2 Bhop Script | ItsJesewe")

def bhop():
    try:
        print(Fore.YELLOW + "[*] Searching for cs2.exe process...\n")
        pm = pymem.Pymem("cs2.exe")
        client_module = pymem.process.module_from_name(pm.process_handle, "client.dll").lpBaseOfDll
        dwForceJump = 0x1730530 # Offset are being updated: https://github.com/a2x/cs2-dumper/blob/main/generated/offsets.py
        force_jump_address = client_module + dwForceJump
        jump = False
    except pymem.exception.ProcessNotFound:
        print(Fore.RED + '[*] cs2.exe process is not running!')
    except pymem.exception.ProcessError:
        print(Fore.RED + '[*] Error accessing process cs2.exe')
    except pymem.exception.ModuleNotFound:
        print(Fore.RED + '[*] module not found')
    except pymem.exception.MemoryReadError:
        print(Fore.RED + '[*] Error reading memory')
    except pymem.exception.MemoryWriteError:
        print(Fore.RED + '[*] Error writing memory')
    except AttributeError:
        print(Fore.RED + '[*] Byte pattern not found')
    else:
        print(Fore.GREEN + "[*] cs2.exe is running.\n\n[*] Client loaded...")
        while True:
            if ctypes.windll.user32.GetAsyncKeyState(0x20):
                if not jump:
                    time.sleep(0.01)
                    pm.write_int(force_jump_address, 65537)
                    jump = True
                elif jump:
                    time.sleep(0.01)
                    pm.write_int(force_jump_address, 256)
                    jump = False
    finally:
        input('\n[*] Press Enter to exit...')

if __name__ == "__main__":
    bhop()