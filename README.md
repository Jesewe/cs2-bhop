<div align="center">
  <img src="src/img/icon.png" alt="CS2 Bhop" width="200" height="200">
  <h1>CS2 Bhop</h1>
  <p>A tool designed to enhance movement in Counter-Strike 2 through bunny hopping by automatically triggering jumps.</p>

![Downloads](https://img.shields.io/github/downloads/jesewe/cs2-bhop/total?style=for-the-badge&logo=github&color=D5006D)
![Platforms](https://img.shields.io/badge/platform-Windows-blue?style=for-the-badge&color=D5006D)
![License](https://img.shields.io/github/license/jesewe/cs2-bhop?style=for-the-badge&color=D5006D)

<a href="#features"><strong>Features</strong></a> •
<a href="#installation"><strong>Installation</strong></a> •
<a href="#usage"><strong>Usage</strong></a> •
<a href="#technical-overview"><strong>Technical Overview</strong></a> •
<a href="#troubleshooting"><strong>Troubleshooting</strong></a> •
<a href="#contributing"><strong>Contributing</strong></a>

</div>

---

# Overview

**CS2 Bhop** is a lightweight C# utility for Counter-Strike 2 that facilitates bunny hopping by automatically triggering jump commands when the spacebar is pressed. It works by dynamically finding the running `cs2.exe` process, retrieving the base address for the game module (`client.dll`), and writing to process memory to simulate jump commands.

# Features

- **Automatic Bunny Hop:**  
  The tool detects spacebar input and alternates between jump and reset states by writing specific values into the game’s memory.

- **Dynamic Offset Retrieval:**  
  Instead of hardcoding memory offsets, the jump offset is fetched dynamically from a remote file hosted on GitHub, ensuring compatibility with game updates.

- **Update Checker:**  
  Upon startup, the tool checks the GitHub repository for newer releases. If an update is available, a message is displayed to inform the user.

- **User Feedback and Logging:**  
  The application provides real-time feedback via console messages, including process detection, module base address retrieval, and offset validation.

- **Minimal Performance Impact:**  
  Designed to run in the background with low overhead, the utility integrates seamlessly while playing.

# Installation

## Prerequisites

- **Windows Operating System:**  
  The tool is built using Windows-specific APIs, such as `user32.dll` for key state detection and `kernel32.dll` for memory operations.

- **.NET Framework / .NET Core:**  
  You will need Visual Studio 2019 (or later) or a compatible .NET development environment to build the project.

## Building from Source

1. **Clone the Repository:**

   ```bash
   git clone https://github.com/Jesewe/cs2-bhop.git
   cd cs2-bhop
   ```

2. **Open the Project:**

   Open the solution file (`.sln`) or the project file (`.csproj`) in Visual Studio.

3. **Build the Solution:**

   Build the project in Visual Studio. Ensure you have administrator privileges if required by your system’s security settings since the tool performs process memory operations.

4. **Run the Executable:**

   Execute the generated `.exe` file. The console application will display the version (e.g., **v1.0.4**) and perform update checks on startup.

## Pre-Built Executable

You can also download the ready-to-use executable from the [Releases](https://github.com/Jesewe/cs2-bhop/releases) page and run it directly.

# Usage

1. **Launch Counter-Strike 2:**  
   Make sure the game is running before you start the utility.

2. **Start CS2 Bhop:**  
   Run the CS2 Bhop executable. The console will search for `cs2.exe` and attempt to locate `client.dll` within the process.

3. **Initiate Bunny Hop:**  
   Once the application confirms that the game process is valid, press the **SPACE** key. The tool will alternate between jump (writing the value `65537`) and reset (writing the value `256`) with short delays to perform a bunny hop.

# Technical Overview

- **Process and Memory Handling:**  
  The tool scans for the `cs2.exe` process and obtains a handle to it. It further retrieves the base address of `client.dll` using Windows API calls for module enumeration.
- **Dynamic Offset Acquisition:**  
  The jump offset (`dwForceJump`) is dynamically retrieved from a remote C++ header file hosted on GitHub:

  ```cpp
  // Example line from remote file:
  constexpr std::ptrdiff_t jump = 0x186CD60;
  ```

  A regular expression is used to extract the hexadecimal offset value during the static initialization of the `Offsets` class.

- **Memory Writing:**  
  Using the `WriteProcessMemory` function from `kernel32.dll`, the tool writes to the game’s memory space to simulate jump inputs. This direct memory manipulation approach is key to achieving the automatic bunny hop functionality.

- **Key Detection:**  
  The spacebar input is detected using the `GetAsyncKeyState` function from `user32.dll`, ensuring responsive trigger detection even while the tool runs in the background.

- **Update and Error Handling:**  
  The application checks for new versions using the GitHub API and displays appropriate messages if an update is available. It also provides clear feedback if the game process or required modules are not detected.

# Troubleshooting

- **Failed to Fetch Offsets:**
  - Verify your internet connection.
  - Ensure that the remote GitHub source (used to fetch the offset) is accessible.
- **Process Not Found:**

  - Confirm that Counter-Strike 2 (`cs2.exe`) is running.
  - Run the tool with the necessary permissions (consider running as administrator).

- **Module `client.dll` Not Detected:**

  - Ensure that the game has fully loaded.
  - Restart the game or the utility if needed.

- **Unexpected Errors:**
  - Check the console output for detailed error messages.
  - Refer to the log file (if logging is enabled) for additional insights.

# Contributing

Contributions are welcome! Feel free to open an issue or submit a pull request on the [GitHub repository](https://github.com/Jesewe/cs2-bhop) if you would like to improve the tool.

# Disclaimer

This project is provided for educational purposes only. Use of cheats or hacks in online games may violate the terms of service, leading to bans or other penalties. Use this tool at your own risk.

# License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
