![logo](unknown.png)

![GitHub](https://img.shields.io/github/license/misternebula/quantum-space-buddies?style=flat-square)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/misternebula/quantum-space-buddies?style=flat-square)
![GitHub Release Date](https://img.shields.io/github/release-date/misternebula/quantum-space-buddies?label=last%20release&style=flat-square)
![GitHub all releases](https://img.shields.io/github/downloads/misternebula/quantum-space-buddies/total?style=flat-square)
![GitHub release (latest by date)](https://img.shields.io/github/downloads/misternebula/quantum-space-buddies/latest/total?style=flat-square)
![GitHub last commit (branch)](https://img.shields.io/github/last-commit/misternebula/quantum-space-buddies/dev?label=last%20commit%20to%20dev&style=flat-square)

Quantum Space Buddies (QSB) is a multiplayer mod for Outer Wilds. The mod uses the OWML mod loader and customized UNET code (internally referred to as QNet or QuantumUNET) for networking.

# Spoilers within!

## License

QNet code adapted in part from Unity Technologies' UNET.

Copyright (C) 2020 - 2021 : 
- Henry Pointer (_nebula or misternebula)
- Will Corby (JohnCorby)
- Aleksander Waage (AmazingAlek)
- Ricardo Lopes (Raicuparta)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.

## Installation

### Easy installation (recommended)

- [Install the Outer Wilds Mod Manager](https://github.com/Raicuparta/ow-mod-manager#how-do-i-use-this);
- Install Quantum Space Buddies from the mod list displayed in the application;
- If you can't get the mod manager to work, follow the instructions for manual installation.

### Manual installation

- [Install OWML](https://github.com/amazingalek/owml#installation);
- [Download the latest Quantum Space Buddies release](https://github.com/misternebula/quantum-space-buddies/releases/latest);
- Extract the `QSB` directory to the `OWML/Mods` directory;
- Run `OWML.Launcher.exe` to start the game.

## Playing as a client

- Run the game.
- On the title menu, select "CONNECT TO MULTIPLAYER".
- Enter the public IP address of the host.
- Hit connect, and pray.

## Playing as a host

- Open port `7777` on your router.
- Run the game.
- On the pause menu, select "OPEN TO MULTIPLAYER".
- Give your external IPv4 address to your clients ([like what you see here](http://whatismyip.host/)).

## Frequently Asked Questions

#### Requirements
- Latest version of OWML.
- Latest version of Mod Manager. (If using)
- Latest version of Outer Wilds. (Epic version preferred, as Steam version is untestable. **We cannot guarantee QSB, or OWML, will work on cracked/pirated versions of Outer Wilds. Do not come asking us for help when using pirated versions.**)
- Fast and stable internet connection, upload and download.
- Above minimum Outer Wilds system requirements.
- Knowledge on port forwarding and router/network configuration. We can be tech support for the mod, not your router or computer.

#### Compatibility with other mods
TL;DR - Don't use any mods with QSB that aren't marked as QSB compatible. 

QSB relies on exact orders of objects found using Resources.FindObjectsOfTypeAll to sync objects, so any mod that changes the hierarchy at all risks breaking QSB. Also, QSB relies on certain game events being called when things happen in-game. Any mod that makes these things happen without calling the correct events will break QSB. Some mods will work fine and have been tested, like CrouchMod. Others may only work partly, like EnableDebugMode and TAICheat.

#### Will you make this compatible with NomaiVR?

Short answer : No.

Long answer : Pay me enough money, and maybe I'll consider it.

#### Why can't a Steam game connect to an Epic game, and vice versa? Do you hate Steam/Epic?

QSB is incompatible between game vendors because of how it works at a base level. Not because I dislike Steam or Epic.

Technical explanation : QSB relies on the orders of lists returned by certain Unity methods to be the same on all clients. For Unity objects, these are (probably) ordered by AssetID or InstanceID. These IDs are different across different game builds. The Epic and Steam versions are different builds. Therefore, the lists are ordered differently and everything breaks.

#### Why can't I connect to a server?
##### For the host :
- Open port 7777 TCP and UDP on your router. If access the internet through multiple layers of routers, the port will need to be opened on every router.
- Open port 7777 TCP and UDP in and out on your firewall. Some AVs might block you editing firewall settings, so check with your specific software.
- Make sure you are giving your public IPv4 address to your clients.
##### For the client :
- Open port 7777 TCP and UDP in and out on your firewall. Some AVs might block you editing firewall settings, so check with your specific software.
- Sometimes, it has helped to change your network profile to "private".
- Make sure you are putting the right address into the address box.

If nothing here works, many people have got QSB working through programs such as Hamachi. Also make sure you are not running through a VPN while trying to connect.

Note - _nebula has no idea how Hamachi works and has never used it, so don't ask them for help setting it up! As said before, we are tech support for the mod. If you cannot connect to someone, or someone cannot connect to you, **that is not QSB's fault.**

#### Why do I keep getting thrown around the ship?
Boring boring physics stuff. The velocity of the ship is synced, as well as the angular velocity. However, this velocity is not also applied to the player. (Or it is sometimes. I don't 100% know.) This means the ship will accelerate, leaving the player "behind". Which makes you fly into the walls alot.
So really there's nothing we can do about this. I disabled damage by impact inside the ship, so if you die inside the ship while it is flying then that is a bug.

#### What's the difference between QSB and Outer Wilds Online?

QSB is a fully synced game. The other players are actually there in the world, and can affect things. The loop starts/ends at the same time for everyone, and you share ship logs / signal discoveries.

Outer Wilds Online is not multiplayer. The other players cannot affect your game, and do not contribute to anything in your save. The loop is entirely per-player.

#### Why would someone make this mod? Seems like a lot of effort for no reward.

Good question.

Let me know if you find an answer.

## Development Setup

- [Download the Outer Wilds Mod Manager](https://github.com/raicuparta/ow-mod-manager) and install it anywhere you like;
- Install OWML using the Mod Manager
- Clone QSB's source
- Open the file `DevEnv.targets` in your favorite text editor
- Edit the entry `<GameDir>` to point to the directory where Outer Wilds is installed
- Edit the entry `<OwmlDir>` to point to your OWML directory (it is installed inside the Mod Manager directory)
- Edit the entry `<UnityAssetsDir>` to point to the Assets folder of the QSB unity project
- Open the project solution file `QSB.sln` in Visual Studio
- If needed, right click `References` in the Solution Explorer > Manage NuGet Packages > Update OWML to fix missing references
  - Use [this](https://github.com/MrPurple6411/AssemblyPublicizer) to create `Assembly-CSharp_publicized.dll`, if you don't already have it
- Run this to stop tracking DevEnv.targets: ```git update-index --skip-worktree DevEnv.targets```

To fix the references, right click "References" in the Solution Explorer > "Add Reference", and add all the missing DLLs (references with yellow warning icon). You can find these DLLs in the game's directory (`OuterWilds\OuterWilds_Data\Managed`);

After doing this, the project references should be working.

The build pipeline (in post-build events):

- Build QuantumUNET.
  - Copy built `QuantumUNET.dll` to mod folder.
- Build QSB.
  - Copy `default-config.json` to mod folder.
  - Copy AssetBundles to mod folder.
  - Copy `manifest.json` to mod folder.
  - Copy built `QSB.dll` into mod folder.
- Build QSBTests.
  - Use `dotnet test` to run QSBTests on QSB project.

If Visual Studio isn't able to automatically copy the files, you'll have to copy the built dlls manually to OWML.

It is recommended to use the Epic version of Outer Wilds, as you cannot run multiple versions of the Steam version.

A powerful PC is needed for development, due to the high amount of RAM and CPU needed to run 2 or 3 instances of modded Outer Wilds.

It is also recommended to lower all graphics settings to minimum, be in windowed mode, and lower resolution to roughly a quarter of your monitor space. This lets you run multiple instances of Outer Wilds to quickly test QSB.

**Warning : Mod development can lead to unexpected errors in your computer system.** 
- **When editing the networking code, mistakes can lead to QSB overwhelming your network connection with excess packets**.
- **Too high RAM usage will lead to Outer Wilds sticking at ~31% loading, then crashing**.
- **There have been instances of graphics cards crashing, and needing to be disabled/re-enabled from Device Manager.**

## Authors and Special Thanks

### Authors

- [\_nebula](https://github.com/misternebula) - Developer of v0.3 onwards
- [JohnCorby](https://github.com/JohnCorby) - Co-developer of 0.13.0 onwards.
- [AmazingAlek](https://github.com/amazingalek) - Developer of v0.1.0 - v0.7.1.
- [Raicuparta](https://github.com/Raicuparta) - Developer of v0.1.0 - v0.2.0.

### Contributers

- [ShoosGun](https://github.com/ShoosGun)
- [Chris Yeninas](https://github.com/PhantomGamers)

### Special Thanks
- Thanks to Logan Ver Hoef for help with the game code.
- Thanks to all the people in the Outer Wilds Discord for helping in public tests.
- Special thanks (and apologies) to all the people in the #modding channel, which I (_nebula) have been using as a virtual [rubber duck.](https://en.wikipedia.org/wiki/Rubber_duck_debugging)

## Help / Discuss development / Whatever

[Join the Outer Wilds Modding Discord](https://discord.gg/9vE5aHxcF9), we have a nice `#mod-support` channel for any mod help, and a other channels to discuss modding!
