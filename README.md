<div align="center">
  <img src="src/img/icon.png" alt="CS2 Bhop" width="200" height="200">
  <h1>CS2 Bhop</h1>
  <p>A lightweight tool designed to enhance movement in Counter-Strike 2 through bunny hopping by automatically triggering jumps.</p>

[![Downloads](https://img.shields.io/github/downloads/jesewe/cs2-bhop/total?style=for-the-badge&logo=github&color=8E44AD)](https://github.com/Jesewe/cs2-bhop/releases)
[![Latest Release](https://img.shields.io/github/v/release/jesewe/cs2-bhop?style=for-the-badge&logo=github&color=8E44AD)](https://github.com/Jesewe/cs2-bhop/releases/latest/)
[![Ko-fi](https://img.shields.io/badge/ko--fi-support-8E44AD?style=for-the-badge&logo=ko-fi)](https://ko-fi.com/E1E51PAHB3)
[![License](https://img.shields.io/github/license/jesewe/cs2-bhop?style=for-the-badge&color=8E44AD)](LICENSE)

<a href="#installation"><strong>Installation</strong></a> •
<a href="#usage"><strong>Usage</strong></a> •
<a href="#troubleshooting"><strong>Troubleshooting</strong></a> •
<a href="#contributing"><strong>Contributing</strong></a>

</div>

---

# Overview

**CS2 Bhop** is a lightweight C# utility for Counter-Strike 2 that facilitates bunny hopping by automatically triggering jump commands when the spacebar is pressed. It works by dynamically finding the running `cs2.exe` process, retrieving the base address for the game module (`client.dll`), and writing to process memory to simulate jump commands with precise timing control.

# Installation

## Prerequisites

- **Windows Operating System:**  
  The tool is built using Windows-specific APIs, such as `user32.dll` for key state detection and window management, and `kernel32.dll` for memory operations.

- **.NET Framework / .NET Core:**  
  You will need Visual Studio 2019 (or later) or a compatible .NET development environment to build the project. The project uses .NET 7+ features including JSON source generators.

## Building from Source

1. **Clone the Repository:**

   ```bash
   git clone https://github.com/Jesewe/cs2-bhop.git
   cd cs2-bhop
   ```

2. **Open the Project:**

   Open the solution file (`.sln`) or the project file (`.csproj`) in Visual Studio.

3. **Build the Solution:**

   Build the project in Visual Studio. Ensure you have administrator privileges if required by your system's security settings since the tool performs process memory operations.

4. **Run the Executable:**

   Execute the generated `.exe` file. The console application will display the version (e.g., **v1.0.5**) and perform update checks on startup.

## Pre-Built Executable

You can also download the ready-to-use executable from the [Releases](https://github.com/Jesewe/cs2-bhop/releases) page and run it directly.

# Usage

1. **Launch Counter-Strike 2:**  
   Make sure the game is running before you start the utility.

2. **Start CS2 Bhop:**  
   Run the CS2 Bhop executable (preferably as administrator). The console will search for `cs2.exe` and attempt to locate `client.dll` within the process.

3. **Initiate Bunny Hop:**  
   Once the application confirms that the game process is valid, press and hold the **SPACE** key while playing. The tool will automatically manage jump states with precise timing to perform bunny hops.

4. **Window Focus Behavior:**  
   The tool only works when CS2 is your active window. If you alt-tab or switch to another application, the bhop will automatically pause and resume when you return to the game.

5. **Exit:**  
   Close the console window to stop the utility. The tool will automatically clean up and reset the jump state.

# Troubleshooting

- **Failed to Fetch Offsets:**

  - Verify your internet connection.
  - Ensure that the remote GitHub source (https://raw.githubusercontent.com/a2x/cs2-dumper) is accessible.
  - Check if your firewall or antivirus is blocking the application's network access.

- **Process Not Found:**

  - Confirm that Counter-Strike 2 (`cs2.exe`) is running.
  - Run the tool with administrator privileges (right-click > Run as administrator).
  - Try pressing 'R' to retry the process search.

- **Module `client.dll` Not Detected:**

  - Ensure that the game has fully loaded into the main menu or a match.
  - Restart the game or the utility if needed.
  - Verify that your CS2 installation is not corrupted.

- **Bhop Not Working:**

  - Make sure CS2 is the active window (the tool only works when CS2 is in focus).
  - Verify that the jump offset was successfully retrieved (check console output).
  - Ensure you're holding the spacebar, not just tapping it.

- **JSON Serialization Errors:**

  - This version uses source generators which require .NET 7+. Ensure you're using the correct .NET version.
  - If building from source, make sure the project targets the correct framework version.

- **Unexpected Errors or Crashes:**
  - Check the console output for detailed error messages.
  - Verify that the CS2 process hasn't been terminated.
  - Try running as administrator if permission errors occur.

# Contributing

Contributions are welcome! Feel free to open an issue or submit a pull request on the [GitHub repository](https://github.com/Jesewe/cs2-bhop) if you would like to improve the tool.

# Disclaimer

This project is provided for educational purposes only. Use of cheats or hacks in online games may violate the terms of service, leading to bans or other penalties. Use this tool at your own risk.

# License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
