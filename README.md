<div align="center">
   <img src="src/img/icon.png" alt="CS2 Bhop" width="200" height="200"> 
   <h1>CS2 Bhop</h1> 
   <p>A tool designed to enhance movement in Counter-Strike 2 through bunny hopping.</p> 
   <a href="#features"><strong>Features</strong></a> •
   <a href="#installation"><strong>Installation</strong></a> •
   <a href="#usage"><strong>Usage</strong></a> •
   <a href="#troubleshooting"><strong>Troubleshooting</strong></a> •
   <a href="#contributing"><strong>Contributing</strong></a>
</div>

---

# Overview
CS2 Bhop is an automated tool for Counter-Strike 2 that facilitates bunny hopping, allowing players to move faster and more fluidly by automatically triggering jumps at the right moments. The tool runs in the background and works seamlessly with the game.

## Features
- **Automatic Bunny Hop**: Automatically jumps for you while holding the configured trigger key.
- **Dynamic Offset Updates**: Automatically fetches the latest offsets required for functionality from a GitHub repository.
- **Logging**: Detailed logs are saved in `%LOCALAPPDATA%\Requests\ItsJesewe\crashes\bhop_logs.log`.
- **Update Checker**: Automatically checks for updates from the GitHub repository.
- **Lightweight**: Minimal impact on game performance.

## Installation

You can install CS2 Bhop by cloning the repository or by downloading a pre-built executable file from the releases.

### Option 1: Clone the Repository

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/Jesewe/cs2-bhop.git
   cd cs2-bhop
   ```

2. **Install Dependencies:**
   ```bash
   pip install -r requirements.txt
   ```

3. **Run the Script:**
   ```bash
   python main.py
   ```

### Option 2: Download Pre-Built Executable

Alternatively, you can download the ready-to-use executable from the [Releases](https://github.com/Jesewe/cs2-bhop/releases) page. Simply download the latest version and run the executable directly.

## Usage
- Launch Counter-Strike 2.
- Run the Bhop tool using the command mentioned above or by launching the GUI version if available.
- The tool will automatically start functioning when the game is active and will respond to the configured trigger key.

## Troubleshooting
- **Failed to Fetch Offsets:** Ensure you have an active internet connection and that the source URLs are accessible.
- **Could Not Open `cs2.exe`:** Make sure the game is running and that you have the necessary permissions.
- **Unexpected Errors:** Check the log file located in the log directory for more details.

## Contributing
Contributions are welcome! Please open an issue or submit a pull request on the [GitHub repository](https://github.com/Jesewe/cs2-bhop).

## Disclaimer
This script is for educational purposes only. Using cheats or hacks in online games is against the terms of service of most games and can result in bans or other penalties. Use this script at your own risk.

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.