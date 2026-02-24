# Jiayi Launcher
Minecraft for Windows mod manager (Bedrock Edition)

**This project has been marked as legacy software, and no major features will be added. We are shifting our focus towards a complete remake of Jiayi Launcher, hoping to achieve feature parity with the legacy mod manager as well as add new features. We will still accept pull requests. Join our Discord server in the badge below for more info on development.**

[![Build](https://github.com/JiayiSoftware/JiayiLauncher/actions/workflows/dotnet.yml/badge.svg)](https://github.com/JiayiSoftware/JiayiLauncher/actions/workflows/dotnet.yml)
[![Discord server](https://img.shields.io/badge/chat-on%20discord-7289da.svg)](https://jiayisoftware.github.io/discord)

## Features
* Simple, clean, and customizable user interface
* Manage and launch internal and external mods, both stored locally and from the internet
* A version manager that integrates with the mod manager
* Back up and switch between different game data folders with Profiles (resource packs, worlds, settings, etc.)
* Easily apply Renderdragon shaders and make your game look beautiful

## Installation
### Using the installer
Most users would prefer a program that handles the installation of Jiayi Launcher. Our installer can be downloaded [here](https://phased.tech/download/JiayiInstaller.exe), and it's about 6 MB in size. The installer automatically downloads and installs the mod manager, installs required dependencies, and integrates Jiayi Launcher with the shell. The source code for the installer is also contained within this repository.

### Manual/portable install
Our current and past releases of Jiayi Launcher can be found in the [Releases](https://github.com/JiayiSoftware/JiayiLauncher/releases) page. All releases are zipped and can be ran immediately after extracting, no installer required.

## Building
If you'd like to, you can clone this repository and build Jiayi Launcher yourself with the instructions below.

### Requirements
* A computer running Windows 10 or 11
* [Git](https://git-scm.com/)
* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* Optionally, an IDE that supports .NET 9 (e.g. [Visual Studio](https://visualstudio.microsoft.com/vs/) or [JetBrains Rider](https://www.jetbrains.com/rider/))

### Steps
1. Clone the repository, and all of its dependencies
* ```git clone --recursive https://github.com/JiayiSoftware/JiayiLauncher.git```

**With an IDE:**

2. Open the solution file in the IDE
3. Build the solution

**Without an IDE:**

2. Navigate to the repository's root directory
3. Run ```dotnet build```
> This will build the mod manager in debug mode. You can append ```-c Release``` to build in release mode.

## Contributing
If you want to contribute to Jiayi Launcher, you can fork the repository and submit a pull request. Jiayi Launcher is written in C# and uses HTML for the UI via Blazor Hybrid.

The code is admittedly a mess, hence why there is no code style to adhere to. However, please try to keep the code clean and readable.

## License
Jiayi Launcher is released under the GNU General Public License, version 3.0. You can read the license [here](https://github.com/JiayiSoftware/JiayiLauncher/blob/master/LICENSE).

This is a summary of the license:
- You can modify, share and sell the code any way you please
- ...but you have to open source projects that do this under a compatible GPL license
- Any changes should be documented

If you bought this program, you were scammed!
