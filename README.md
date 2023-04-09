<div align="center">
  <a href="https://jiayi.software/launcher"><img src="https://github.com/JiayiSoftware/JiayiLauncher/tree/master/.github/assets/logo.png" alt="Jiayi Launcher" width="700"></a>
</div>

### Minecraft for Windows mod manager (Bedrock Edition)

[![Build](https://github.com/JiayiSoftware/JiayiLauncher/actions/workflows/dotnet.yml/badge.svg)](https://github.com/JiayiSoftware/JiayiLauncher/actions/workflows/dotnet.yml)
[![Discord server](https://img.shields.io/badge/chat-on%20discord-7289da.svg)](https://jiayi.software/discord)

## Features
> * Simple, clean user interface
> * Manage and launch internal and external mods, both stored locally and from the Internet
> * A version manager that goes hand in hand with the mod manager
> * Switch between different game data folders with Profiles (resource packs, worlds, settings, etc.)
> * Extreme customizability, from the launcher's appearance down to the source code
> * Easily apply Renderdragon shaders and make your game look beautiful

## Installation
If you're a regular user, the launcher will be available in the [Releases](https://github.com/JiayiSoftware/JiayiLauncher/releases) page on May 1st.

If you're a developer, you can clone the repository and build Jiayi Launcher yourself with the instructions below.

## Building
### Requirements
* A computer running Windows 10 or later
* [Git](https://git-scm.com/)
* [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
* Optionally, an IDE that supports .NET 7 (e.g. [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [JetBrains Rider](https://www.jetbrains.com/rider/))

### Steps
1. Clone the repository
* ```git clone https://github.com/JiayiSoftware/JiayiLauncher.git```

**With an IDE:**

2. Open the solution file in the IDE
3. Build the solution

**Without an IDE:**

2. Navigate to the repository's root directory
3. Run ```dotnet build```

This will build the launcher in debug mode. You can append ```-c Release``` to build in release mode.

## Contributing
If you want to contribute to Jiayi Launcher, you can fork the repository and submit a pull request.

Jiayi is written in C# and uses HTML for the UI via Blazor Hybrid.

The code is admittedly a mess, hence why there is no code style to adhere to. However, please try to keep the code clean and readable. There will be a code style guide in the future.

## License
Jiayi Launcher is released under the GNU GPL-3.0 license, and you can read it [here](https://github.com/JiayiSoftware/JiayiLauncher/blob/master/LICENSE).

A quick rundown for those who don't want to read all that:
- You can modify, share and sell the code any way you please
- but you have to open source projects that do this under a compatible GPL license
- Any changes should be documented 
