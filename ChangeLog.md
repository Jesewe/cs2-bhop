### Summary

- Importing `GetForegroundWindow` and `GetWindowThreadProcessId` to determine the active window.
- The main bhop loop is refactored to check for the active window before processing jumps.
- The jump logic is overhauled from a simple toggle to a timed state-based system for more reliable and controlled jumps, using a configurable delay.
- Updated JSON deserialization for offsets by introducing a dedicated `OffsetsData` class, making the code more robust and readable.

[![Downloads](https://img.shields.io/github/downloads/Jesewe/cs2-bhop/v1.0.5/total?style=for-the-badge&logo=github&color=8E44AD)](https://github.com/Jesewe/cs2-bhop/releases/tag/v1.0.5) [![Platforms](https://img.shields.io/badge/platform-Windows-blue?style=for-the-badge&color=8E44AD)](https://github.com/Jesewe/cs2-bhop/releases/download/v1.0.5/cs2-bhop.exe) [![Ko-fi](https://img.shields.io/badge/ko--fi-support-8E44AD?style=for-the-badge&logo=ko-fi)](https://ko-fi.com/E1E51PAHB3) [![License](https://img.shields.io/github/license/jesewe/cs2-triggerbot?style=for-the-badge&color=8E44AD)](https://github.com/Jesewe/VioletWing/blob/main/LICENSE)
