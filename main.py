import pymem, pymem.process
import time
import ctypes
import logging
from requests import get

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

ctypes.windll.kernel32.SetConsoleTitleW("CS2 Bhop Script | ItsJesewe")

SPACE_KEY = 0x20
FORCE_JUMP_ON = 65537
FORCE_JUMP_OFF = 256
SLEEP_INTERVAL = 0.01

def fetch_offsets():
    try:
        buttons = get("https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/buttons.json").json()
        return buttons
    except Exception as e:
        logger.error(f"Failed to fetch offsets: {e}")
        return None

def get_cs2_process():
    try:
        return pymem.Pymem("cs2.exe")
    except pymem.exception.ProcessNotFound:
        logger.error('cs2.exe process is not running!')
    except pymem.exception.ProcessError:
        logger.error('Error accessing process cs2.exe')
    return None

def get_client_module(pm):
    try:
        return pymem.process.module_from_name(pm.process_handle, "client.dll").lpBaseOfDll
    except pymem.exception.ModuleNotFound:
        logger.error('client.dll module not found')
    return None

def bhop():
    logger.info("Searching for cs2.exe process...")
    pm = get_cs2_process()
    if not pm:
        return

    client_module = get_client_module(pm)
    if not client_module:
        return

    buttons = fetch_offsets()
    if not buttons:
        return

    global ForceJump
    ForceJump = buttons["client.dll"]["jump"]
    force_jump_address = client_module + ForceJump

    jump = False
    try:
        while True:
            if ctypes.windll.user32.GetAsyncKeyState(SPACE_KEY):
                time.sleep(SLEEP_INTERVAL)
                pm.write_int(force_jump_address, FORCE_JUMP_ON if not jump else FORCE_JUMP_OFF)
                jump = not jump
    except pymem.exception.MemoryWriteError:
        logger.error('Error writing memory')
    except KeyboardInterrupt:
        logger.warning('Script interrupted by user')

if __name__ == "__main__":
    bhop()