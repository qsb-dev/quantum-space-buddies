# Quantum Space Buddies - Outer Wilds Online Multiplayer Mod

<!-- TOC -->

- [Installation](#installation)
  - [Easy installation (recommended)](#easy-installation-recommended)
  - [Manual installation](#manual-installation)
- [Playing as a client](#playing-as-a-client)
- [Playing as a host](#playing-as-a-host)
- [Development Setup](#development-setup)
- [Authors](#authors)
- [Special thanks](#special-thanks)
- [Help / Discuss development / Whatever](#help--discuss-development--whatever)

<!-- /TOC -->

## Installation

### Easy installation (recommended)

- [Install the Outer Wilds Mod Manager](https://github.com/Raicuparta/ow-mod-manager#how-do-i-use-this);
- Install Quantum Space Buddies from the mod list displayed in the application;
- If you can't get the mod manager to work, follow the instructions for manual installation.

### Manual installation

- [Install OWML](https://github.com/amazingalek/owml#installation);
- [Download the latest Quantum Space Buddies release](https://github.com/Raicuparta/quantum-space-buddies/releases/latest);
- Extract the `QSB` directory to the `OWML/Mods` directory;
- Run `OWML.Launcher.exe` to start the game.

## Playing as a client

- Run `OWML.Launcher.exe` to start the game;
- You'll see some new buttons on the top left;
- Replace `localhost` with the server's IP address;
- Press "LAN Client(C)";
- If you see "Stop (X)", you are connected.

## Playing as a host

- Open port `7777` on your router;
- Run `OWML.Launcher.exe` to start the game;
- You'll see some new buttons on the top left;
- Don't start the game (expedition) before starting the server;
- Press "LAN Host(H)";
- If you now see the "Stop (X)" button, you are serving;
- Give your external IPv4 address to your clients ([like what you see here](http://whatismyip.host/)).

## Development Setup

- [Download the Outer Wilds Mod Manager](https://github.com/Raicuparta/ow*mod*manager) and install it anywhere you like;
- Install OWML using the Mod Manager;
- Clone QSB's source;
- Open the file `QSB/QSB.csproj.user` in your favorite text editor;
- Edit the entry `<GameDir>` to point to the directory where Outer Wilds is installed;
- Edit the entry `<OwmlDir>` to point to your OWML directory (it is installed inside the Mod Manager directory);
- Open the project solution file `QSB.sln` in Visual Studio;
- If needed, right click `References` in the Solution Explorer > Manage NuGet Packages > Update OWML to fix missing references;
- Run this to stop tracking QSB.csproj.user: ```git update-index --skip-worktree QSB/QSB.csproj.user```

After doing this, the project references should be working. When you build the solution, the dll and json files will be copied to `[Mod Manager directory]/OWML/QSB`. If this process is successful, you should see the mod show up in the Mod Manager.

If for some reason none of this is working, you might have to set everything manually:

- To fix the references, right*click "References" in the Solution Explorer > "Add Reference", and add all the missing DLLs (references with yellow warning icon). You can find these DLLs in the game's directory (`OuterWilds\OuterWilds_Data\Managed`);
- If Visual Studio isn't able to automatically copy the files, you'll have to copy the built dlls manually to OWML.

## Authors

- [Mister_Nebula](https://github.com/misternebula) - Current lead
- [AmazingAlek](https://github.com/amazingalek)
- [Raicuparta](https://github.com/Raicuparta)

## Help / Discuss development / Whatever

[Join the unofficial Outer Wilds Discord](https://discord.gg/Sftcc9Z), we have a nice `#modding` channel where you can discuss all types of things.
