import pymem
import pymem.process
import time
import os
import ctypes
import logging
import sys
import requests
from colorama import init, Fore
from packaging import version

# Initialize colorama for colored console output
init(autoreset=True)

# Constants
KEY_SPACE = 0x20
FORCE_JUMP_ACTIVE = 65537
FORCE_JUMP_INACTIVE = 256

class Logger:
    """
    Handles logging setup for the application.
    Logs are written both to a file and the console.
    """
    LOG_DIRECTORY = os.path.expandvars(r'%LOCALAPPDATA%\Requests\ItsJesewe\crashes')
    LOG_FILE = os.path.join(LOG_DIRECTORY, 'bhop_logs.log')

    @staticmethod
    def setup_logging():
        """Set up logging configuration."""
        os.makedirs(Logger.LOG_DIRECTORY, exist_ok=True)
        # Clear the log file on each run
        with open(Logger.LOG_FILE, 'w'):
            pass

        logging.basicConfig(
            level=logging.INFO,
            format='%(levelname)s: %(message)s',
            handlers=[logging.FileHandler(Logger.LOG_FILE), logging.StreamHandler()]
        )

class Utility:
    """
    Contains utility functions such as setting the console title,
    fetching offsets, and checking for software updates.
    """
    
    @staticmethod
    def set_console_title(title):
        """Sets the console window title."""
        try:
            ctypes.windll.kernel32.SetConsoleTitleW(title)
        except Exception as e:
            logging.error(f"{Fore.RED}Failed to set console title: {e}")

    @staticmethod
    def fetch_offsets():
        """
        Fetches offsets from the GitHub repository.
        Returns the integer offset if found, otherwise None.
        """
        url = "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/buttons.hpp"
        try:
            response = requests.get(url, timeout=10)
            response.raise_for_status()
            offsets = response.text
            # Extract the dwForceJump offset from the line containing "jump"
            for line in offsets.splitlines():
                if "jump" in line:
                    # Extract the offset value after the last '=' symbol and remove any trailing ';'
                    offset_str = line.split('=')[-1].strip().rstrip(';')
                    try:
                        offset = int(offset_str, 16)
                        return offset
                    except ValueError as ve:
                        logging.error(f"{Fore.RED}Error converting offset '{offset_str}' to int: {ve}")
                        return None
            logging.error(f"{Fore.RED}Jump offset not found in the fetched data.")
        except requests.RequestException as re:
            logging.error(f"{Fore.RED}Failed to fetch offsets from {url}: {re}")
        except Exception as e:
            logging.error(f"{Fore.RED}Unexpected error while fetching offsets: {e}")
        return None

    @staticmethod
    def check_for_updates(current_version):
        """
        Checks for software updates on GitHub.
        Informs the user if a newer version is available.
        """
        url = "https://api.github.com/repos/Jesewe/cs2-bhop/tags"
        headers = {"User-Agent": "CS2-Bhop-Checker"}
        try:
            response = requests.get(url, headers=headers, timeout=10)
            response.raise_for_status()
            tags = response.json()
            if not tags:
                logging.info(f"{Fore.YELLOW}No tags found in the repository.")
                return
            latest_version = tags[0]["name"]
            # Remove 'v' prefix if present for proper version parsing
            current_ver = current_version.lstrip("v")
            latest_ver = latest_version.lstrip("v")
            if version.parse(latest_ver) > version.parse(current_ver):
                logging.warning(f"{Fore.YELLOW}New version available: {latest_version}. Please update.")
            elif version.parse(current_ver) > version.parse(latest_ver):
                logging.info(f"{Fore.YELLOW}Developer version: You are using a pre-release version.")
            else:
                logging.info(f"{Fore.GREEN}You are using the latest version.")
        except Exception as e:
            logging.error(f"{Fore.RED}Error checking for updates: {e}")

class Bhop:
    """
    Handles the bunnyhop functionality.
    Connects to the game process, locates the client module, and performs the bunnyhop.
    """
    VERSION = "v1.0.3"

    def __init__(self):
        """Initializes the Bhop instance and fetches necessary offsets."""
        self.pm = None
        self.dwForceJump = Utility.fetch_offsets()
        if self.dwForceJump is None:
            logging.error(f"{Fore.RED}Failed to fetch required offsets. Exiting...")
            sys.exit(1)
        self.client_base = None
        self.force_jump_address = None

    def initialize_pymem(self):
        """
        Attaches to the cs2.exe process using Pymem.
        Returns True on success, False on failure.
        """
        try:
            self.pm = pymem.Pymem("cs2.exe")
            logging.info(f"{Fore.GREEN}Successfully attached to cs2.exe process.")
            return True
        except pymem.exception.ProcessNotFound:
            logging.error(f"{Fore.RED}cs2.exe process not found. Ensure the game is running.")
        except pymem.exception.PymemError as pe:
            logging.error(f"{Fore.RED}Pymem error: {pe}")
        except Exception as e:
            logging.error(f"{Fore.RED}Unexpected error during Pymem initialization: {e}")
        return False

    def get_client_module(self):
        """
        Retrieves the base address of the client.dll module and calculates the force jump address.
        Returns True on success, False on failure.
        """
        try:
            client_module = pymem.process.module_from_name(self.pm.process_handle, "client.dll")
            if not client_module:
                raise pymem.exception.ModuleNotFoundError("client.dll not found")
            self.client_base = client_module.lpBaseOfDll
            self.force_jump_address = self.client_base + self.dwForceJump
            logging.info(f"{Fore.GREEN}client.dll module found. Force jump address set.")
            return True
        except pymem.exception.ModuleNotFoundError as mnfe:
            logging.error(f"{Fore.RED}Error: {mnfe}. Ensure client.dll is loaded.")
        except Exception as e:
            logging.error(f"{Fore.RED}Unexpected error retrieving client module: {e}")
        return False

    def start(self):
        """Starts the bunnyhop loop."""
        Utility.set_console_title(f"CS2 Bhop {self.VERSION}")
        logging.info(f"{Fore.CYAN}Checking for updates...")
        Utility.check_for_updates(self.VERSION)
        logging.info(f"{Fore.CYAN}Initializing cs2.exe process and client module...")

        if not self.initialize_pymem() or not self.get_client_module():
            logging.error(f"{Fore.RED}Initialization failed. Exiting...")
            input("Press Enter to exit...")
            return

        logging.info(f"{Fore.GREEN}Bunnyhop started. Hold Spacebar to jump.")
        is_jumping = False

        try:
            while True:
                # Check if the Spacebar is pressed (0x8000 indicates the key is down)
                if ctypes.windll.user32.GetAsyncKeyState(KEY_SPACE) & 0x8000:
                    if not is_jumping:
                        time.sleep(0.01)
                        self.pm.write_int(self.force_jump_address, FORCE_JUMP_ACTIVE)
                        is_jumping = True
                    else:
                        time.sleep(0.01)
                        self.pm.write_int(self.force_jump_address, FORCE_JUMP_INACTIVE)
                        is_jumping = False
                else:
                    # Brief sleep to reduce CPU usage when the key is not pressed
                    time.sleep(0.005)
        except KeyboardInterrupt:
            logging.info(f"{Fore.YELLOW}Bunnyhop terminated by user.")
        except Exception as e:
            logging.error(f"{Fore.RED}Unexpected error in bunnyhop loop: {e}")
            logging.error(f"{Fore.RED}Report this issue at: https://github.com/Jesewe/cs2-triggerbot/issues")
            input("Press Enter to exit...")

if __name__ == "__main__":
    Logger.setup_logging()
    bhop = Bhop()
    bhop.start()