# CS2 Bhop Script

This is a Bunny Hop (Bhop) script for Counter-Strike 2 (CS2) that automates jumping while holding the space bar, providing a smoother and more consistent bunny hopping experience.

## Features

- Automatically jumps when the space bar is held down
- Simple and lightweight script
- Real-time process and memory manipulation using the `pymem` library
- Fetches the latest game offsets from an online repository

## Requirements

- Python 3.x
- `pymem` library
- `requests` library

## Installation

1. **Clone the repository:**

    ```bash
    git clone https://github.com/Jesewe/cs2-bhop.git
    cd cs2-bhop
    ```

2. **Install the required Python libraries:**

    ```bash
    pip install pymem requests
    ```

## Usage

1. **Ensure that Counter-Strike 2 (cs2.exe) is running.**

2. **Run the script:**

    ```bash
    python main.py
    ```

3. **Hold down the space bar to activate the bunny hop.**

## Script Explanation

The script works by:
- Searching for the `cs2.exe` process.
- Fetching the memory offsets required for jumping from an online repository.
- Writing to the memory address responsible for the jump action when the space bar is pressed.

### Code Overview

- `fetch_offsets()`: Fetches the latest game offsets from an online repository.
- `get_cs2_process()`: Retrieves the `cs2.exe` process.
- `get_client_module(pm)`: Gets the `client.dll` module from the process.
- `bhop()`: Main function that handles the bunny hopping logic.

### Error Handling

The script includes error handling for various exceptions, including process not found, module not found, memory read/write errors, and more.

## Logging

The script uses the `logging` module to log important information and errors, making it easier to debug and monitor the script's execution.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Disclaimer

This script is for educational purposes only. Use it at your own risk. The author is not responsible for any consequences resulting from the use of this script, including but not limited to game bans or other punitive actions by game administrators.