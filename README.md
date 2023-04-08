# Jiayi Launcher
Minecraft for Windows mod manager (Bedrock Edition)

Project website @ https://jiayi.software/launcher

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
If you're a regular user, you can download the latest release from the [Releases](https://github.com/JiayiSoftware/JiayiLauncher/releases) page.

If you're a developer, you can clone the repository and build Jiayi Launcher yourself with the instructions below.

## Building
### Requirements
* A computer running Windows 10
* [Git](https://git-scm.com/)
* [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
* An IDE that supports .NET 7 (e.g. [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [JetBrains Rider](https://www.jetbrains.com/rider/))
  * *VS Code can work but it's not recommended*

### Steps
1. Clone the repository
* ```git clone https://github.com/JiayiSoftware/JiayiLauncher.git```
2. Open the solution file in your IDE
3. Build the solution for x64

## Contributing
If you want to contribute to Jiayi Launcher, you can fork the repository and submit a pull request.

Jiayi is written in C# and uses HTML for the UI via Blazor Hybrid.

The code is admittedly a mess, hence why there is no code style to adhere to. However, please try to keep the code clean and readable. There will be a code style guide in the future.
