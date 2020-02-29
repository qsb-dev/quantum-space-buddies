# Quantum Space Buddies - Outer Wilds Online Multiplayer Mod

## Installation

* [Download OWML](https://github.com/amazingalek/owml/releases);
* [Follow OWML's instalation instructions](https://github.com/amazingalek/owml#installation);
* [Download the latest QSB release](https://github.com/Raicuparta/quantum-space-buddies/releases/latest);
* Extract the `QSB` directory to the `OWML/Mods` directory;
* Run `OWML.Launcher.exe` to start the game.

## Playing as a client

* Run `OWML.Launcher.exe` to start the game;
* You'll see some new buttons on the top left;
* Replace `localhost` with the server's IP address;
* Press "LAN Client(C)";
* If you see "Stop (X)", you are connected.

## Playing as a host

* Open port `7777` on your router;
* Run `OWML.Launcher.exe` to start the game;
* You'll see some new buttons on the top left;
* Don't start the game (expedition) before starting the server;
* Press "LAN Host(H)";
* If you now see the "Stop (X)" button, you are serving;
* Give your external IPv4 address to your clients ([like what you see here](http://whatismyip.host/)).

## Development Setup

To get the project to run on Visual Studio and build correctly, you need to set this all up so the project can find its dependencies:

* [Install OWML](https://github.com/amazingalek/owml#installation) in the game's directory (should be something like `C:\Program Files\Epic Games\OuterWilds\OWML`);
* If you already have QSB installed, remove it from the `OWML/Mods` directory;
* Clone QSB's source;
* Open the project solution file `QSB.sln` in Visual Studio;
* On the Solution Explorer (usually the right side panel), under the project-name (NomaiVR), double click "Properties";
* Go to "Debug" and change "Working Directory" to **OWML's directory**;
* Do the same thing for all the other projects in the QSB solution;
* If needed, right click `References` in the Solution Explorer > Manage NuGet Packages > Update OWML to fix missing references;
* In the top menu go to "Project" > "Unload Project", and then "Project" > "Reload Project".

After doing this, the project references should be working. When you build the solution, the dll and json files will be copied to `OWML/Mods/QSB`, so you can start the game through OWML and test right away.

If for some reason none of this is working, you might have to set everything manually:

* To fix the build paths and automatically copy the files to OWML, edit the "Build Events" in the properties menu.
* To fix the references, right-click "References" in the Solution Explorer > "Add Reference", and add all the missing DLLs (references with yellow warning icon). You can find these DLLs in the game's directory (`OuterWilds\OuterWilds_Data\Managed`).

## Authors

* [AmazingAlek](https://github.com/amazingalek)
* [Raicuparta](https://github.com/Raicuparta)

## Special thanks

* [Mister_Nebula](https://github.com/misternebula), for research

## Help / Discuss development / Whatever

[Join the unofficial Outer Wilds Discord](https://discord.gg/Sftcc9Z), we have a nice `#modding` channel where you can discuss all types of things.
