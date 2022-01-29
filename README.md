![logo](unknown.png)

![GitHub](https://img.shields.io/github/license/misternebula/quantum-space-buddies?style=flat-square)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/misternebula/quantum-space-buddies?style=flat-square)
![GitHub Release Date](https://img.shields.io/github/release-date/misternebula/quantum-space-buddies?label=last%20release&style=flat-square)
![GitHub all releases](https://img.shields.io/github/downloads/misternebula/quantum-space-buddies/total?style=flat-square)
![GitHub release (latest by date)](https://img.shields.io/github/downloads/misternebula/quantum-space-buddies/latest/total?style=flat-square)
![GitHub last commit (branch)](https://img.shields.io/github/last-commit/misternebula/quantum-space-buddies/dev?label=last%20commit%20to%20dev&style=flat-square)

Quantum Space Buddies (QSB) is a multiplayer mod for Outer Wilds. The mod uses the OWML mod loader and Mirror for networking.

Spoilers within!

## License

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

## Hosting / Connecting

### If you own Outer Wilds on Steam

Make sure you are logged into Steam before hosting/connecting.

#### Connecting to a server

- On the title screen, click the option `CONNECT TO MULTIPLAYER`.
- Enter the SteamID of the person you are trying to connect to.
- Enjoy!

#### Hosting a server

- Enter a game. This can be a new expedition or an existing save file.
- On the pause screen, click the option `OPEN TO MULTIPLAYER`.
- Share your SteamID with the people who want to connect.
- Enjoy!

### If you do not own Outer Wilds on Steam

QSB uses Steamworks to simplify connecting and hosting. If you do not own Outer Wilds on steam, you will not be able to use this.

There are several ways around this :
- Change the "appIdOverride" option in `debugsettings.json` to an AppID that you own on Steam. The most common id to use is 480, as Spacewar is an app everyone owns by default. You will then be able to connect and host as detailed in the above section.
- If that doesn't work, enable the "useKcpTransport" option in `debugsettings.json`. This will force QSB to use the KCP transport, which means you will have to port forward and all that fun stuff. To connect/host, follow the below instructions.

#### Connecting to a server with KCP transport

- On the title screen, click the option `CONNECT TO MULTIPLAYER`.
- Enter the public IP address of the person you are trying to connect to.
- Enjoy!

#### Hosting a server with KCP transport

- Port forward port 7777 with UDP/TCP.
- Make sure your firewall isn't blocking the connections, you've port forwarded the entire route to your NAT (if using multiple routers), etc. There are many guides on port forwarding online, so check those if you need help.
- Enter a game. This can be a new expedition or an existing save file.
- On the pause screen, click the option `OPEN TO MULTIPLAYER`.
- Share your public IP address with the people who want to connect.
- Enjoy!

## Frequently Asked Questions

### Requirements
- Steam account.
- Latest version of OWML.
- Latest version of Mod Manager. (If using)
- Latest version of Outer Wilds. **We cannot guarantee QSB, or OWML, will work on cracked/pirated versions of Outer Wilds. Do not come asking us for help when using pirated versions.**
- Fast and stable internet connection, upload and download.
- Above minimum Outer Wilds system requirements.

### How complete is this mod? How far through the game can I play?

| Area of the game  | Working |
| ------------- | ------------- |
| Base game  | :heavy_check_mark:  |
| Echoes of the Eye  | :x:  |

### Compatibility with other mods
TL;DR - Don't use any mods with QSB that aren't marked as QSB compatible. 

QSB relies on exact orders of objects found using Resources.FindObjectsOfTypeAll to sync objects, so any mod that changes the hierarchy at all risks breaking QSB. Also, QSB relies on certain game events being called when things happen in-game. Any mod that makes these things happen without calling the correct events will break QSB. Some mods will work fine and have been tested, like CrouchMod. Others may only work partly, like EnableDebugMode and TAICheat.

### Will you make this compatible with NomaiVR?

Short answer : No.

Long answer : Pay me enough money, and maybe I'll consider it.

### Why can't a Steam game connect to an Epic game, and vice versa? Do you hate Steam/Epic?

QSB is incompatible between game vendors because of how it works at a base level. Not because I dislike Steam or Epic.

Technical explanation : QSB relies on the orders of lists returned by certain Unity methods to be the same on all clients. For Unity objects, these are (probably) ordered by AssetID or InstanceID. These IDs are different across different game builds. The Epic and Steam versions are different builds. Therefore, the lists are ordered differently and everything breaks.

### Why do I keep getting thrown around the ship?

Boring boring physics stuff. The velocity of the ship is synced, as well as the angular velocity. However, this velocity is not also applied to the player. (Or it is sometimes. I don't 100% know.) This means the ship will accelerate, leaving the player "behind". Which makes you fly into the walls alot.
So really there's nothing we can do about this. I disabled damage by impact inside the ship, so if you die inside the ship while it is flying then that is a bug.

### What's the difference between QSB and Outer Wilds Online?

TL;DR - QSB is multiplayer co-op, Outer Wilds Online is multiplayer not not co-op.

QSB is a fully synced game. The other players are actually there in the world, and can affect things. The loop starts/ends at the same time for everyone, and you share ship logs / signal discoveries.

Outer Wilds Online is easier to set up, but much more basic in its features. The other players cannot affect your game, and do not contribute to anything in your save. The loop is entirely per-player.

### Why would someone make this mod? Seems like a lot of effort for no reward.

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
- [JohnCorby](https://github.com/JohnCorby) - Co-developer of v0.13.0 onwards.
- [AmazingAlek](https://github.com/amazingalek) - Developer of v0.1.0 - v0.7.1.
- [Raicuparta](https://github.com/Raicuparta) - Developer of v0.1.0 - v0.2.0.

### Contributers

- [ShoosGun](https://github.com/ShoosGun)
- [Chris Yeninas](https://github.com/PhantomGamers)

### Special Thanks
- Thanks to Logan Ver Hoef for help with the game code, and for helping make the damn game in the first place.
- Thanks to all the people who helped in public tests.

### Dependencies

- [OWML](https://github.com/amazingalek/owml)
- [Mirror](https://mirror-networking.com/)
- [FizzyFacepunch](https://github.com/Chykary/FizzyFacepunch)
- [HarmonyX](https://github.com/BepInEx/HarmonyX)
- [Mono.Cecil](https://github.com/jbevain/cecil)
- [UniTask](https://github.com/Cysharp/UniTask)

## Help / Discuss development / Whatever

[Join the Outer Wilds Modding Discord](https://discord.gg/9vE5aHxcF9), we have a nice `#qsb-bugs-and-questions` channel for support, and other channels to discuss modding!
