import pymem
import pymem.process
import time
import os
import ctypes
import logging
from requests import get
from colorama import init, Fore
from packaging import version

# Initialize colorama for colored console output
init(autoreset=True)

class Logger:
    """Handles logging setup for the application."""

    LOG_DIRECTORY = os.path.expandvars(r'%LOCALAPPDATA%\Requests\ItsJesewe\crashes')
    LOG_FILE = os.path.join(LOG_DIRECTORY, 'bhop_logs.log')

    @staticmethod
    def setup_logging():
        """Set up the logging configuration with the default log level INFO."""
        os.makedirs(Logger.LOG_DIRECTORY, exist_ok=True)
        with open(Logger.LOG_FILE, 'w') as f:
            pass

        logging.basicConfig(
            level=logging.INFO,  # Default to INFO level logging
            format='%(levelname)s: %(message)s',
            handlers=[logging.FileHandler(Logger.LOG_FILE), logging.StreamHandler()]
        )

class Utility:
    """Contains utility functions for the application."""

    @staticmethod
    def set_console_title(title):
        ctypes.windll.kernel32.SetConsoleTitleW(title)

    @staticmethod
    def fetch_offsets():
        """Fetches offsets from the GitHub repository."""
        try:
            response = get("https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/buttons.hpp")
            if response.status_code == 200:
                offsets = response.text
                # Extract dwForceJump offset from the line containing "jump"
                for line in offsets.splitlines():
                    if "jump" in line:
                        # Extract the offset value after the last '=' symbol, and remove any trailing characters
                        offset_str = line.split('=')[-1].strip().rstrip(';')
                        offset = int(offset_str, 16)
                        return offset
            else:
                logging.error(f"{Fore.RED}Failed to fetch offsets from server.")
        except Exception as e:
            logging.error(f"{Fore.RED}Failed to fetch offsets: {e}")
        return None


    @staticmethod
    def check_for_updates(current_version):
        """Checks for software updates on GitHub."""
        try:
            response = get("https://api.github.com/repos/Jesewe/cs2-bhop/tags")
            response.raise_for_status()
            latest_version = response.json()[0]["name"]
            if version.parse(latest_version) > version.parse(current_version):
                logging.warning(f"{Fore.YELLOW}New version available: {latest_version}. Please update for the latest fixes and features.")
            elif version.parse(current_version) > version.parse(latest_version):
                logging.info(f"{Fore.YELLOW}Developer version: You are using a pre-release or developer version.")
            else:
                logging.info(f"{Fore.GREEN}You are using the latest version.")
        except Exception as e:
            logging.error(f"{Fore.RED}Error checking for updates: {e}")

class Bhop:
    """Handles the bunnyhop functionality."""

    VERSION = "v1.0.2"

    def __init__(self):
        """Initializes the Bhop instance."""
        self.pm = None
        self.dwForceJump = Utility.fetch_offsets()
        self.client_base = None
        self.force_jump_address = None

    def initialize_pymem(self):
        """Initializes Pymem and attaches to the game process."""
        try:
            self.pm = pymem.Pymem("cs2.exe")
            logging.info(f"{Fore.GREEN}Successfully attached to cs2.exe process.")
        except pymem.exception.ProcessNotFound:
            logging.error(f"{Fore.RED}Could not find cs2.exe process. Please make sure the game is running.")
            return False
        except pymem.exception.PymemError as e:
            logging.error(f"{Fore.RED}Pymem encountered an error: {e}")
            return False
        except Exception as e:
            logging.error(f"{Fore.RED}Unexpected error during Pymem initialization: {e}")
            return False
        return True 

    def get_client_module(self):
        """Retrieves the base address of the client.dll module."""
        try:
            if self.client_base is None:
                client_module = pymem.process.module_from_name(self.pm.process_handle, "client.dll")
                if not client_module:
                    raise pymem.exception.ModuleNotFoundError("client.dll not found")
                self.client_base = client_module.lpBaseOfDll
                self.force_jump_address = self.client_base + self.dwForceJump  # Set the force jump address
        except pymem.exception.ModuleNotFoundError as e:
            logging.error(f"{Fore.RED}Error: {e}. Ensure client.dll is loaded.")
            return False
        except Exception as e:
            logging.error(f"{Fore.RED}Unexpected error retrieving client module: {e}")
            return False
        return True  # Return True if the client module was successfully retrieved

    def start(self):
        """Starts the bunnyhop loop."""

        Utility.set_console_title(f"CS2 Bhop {self.VERSION}")

        logging.info(f"{Fore.CYAN}Checking for updates...")
        Utility.check_for_updates(self.VERSION)

        logging.info(f"{Fore.CYAN}Fetching offsets and client data...")

        logging.info(f"{Fore.CYAN}Searching for cs2.exe process...")
        if not self.initialize_pymem():
            input(f"{Fore.RED}Press Enter to exit...")
            return

        if not self.get_client_module():
            input(f"{Fore.RED}Press Enter to exit...")
            return

        jump = False
        logging.info(f"{Fore.GREEN}Bunnyhop started.")


        while True:
            try:
                if ctypes.windll.user32.GetAsyncKeyState(0x20):  # Spacebar
                    if not jump:
                        time.sleep(0.01)
                        self.pm.write_int(self.force_jump_address, 65537)
                        jump = True
                    else:
                        time.sleep(0.01)
                        self.pm.write_int(self.force_jump_address, 256)
                        jump = False
            except Exception as e:
                logging.error(f"{Fore.RED}Unexpected error: {e}")
                logging.error(f"{Fore.RED}Please report this issue on the GitHub repository: https://github.com/Jesewe/cs2-triggerbot/issues")
                input(f"{Fore.RED}Press Enter to exit...")

if __name__ == "__main__":
    Logger.setup_logging()
    bhop = Bhop()
    bhop.start()