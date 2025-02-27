![logo](unknown.png)

![GitHub](https://img.shields.io/github/license/misternebula/quantum-space-buddies?style=flat-square)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/misternebula/quantum-space-buddies?style=flat-square)
![GitHub Release Date](https://img.shields.io/github/release-date/misternebula/quantum-space-buddies?label=last%20release&style=flat-square)
![GitHub all releases](https://img.shields.io/github/downloads/misternebula/quantum-space-buddies/total?style=flat-square)
![GitHub release (latest by date)](https://img.shields.io/github/downloads/misternebula/quantum-space-buddies/latest/total?style=flat-square)
![GitHub last commit (branch)](https://img.shields.io/github/last-commit/misternebula/quantum-space-buddies/dev?label=last%20commit%20to%20dev&style=flat-square)

[![Donate with PayPal](https://img.shields.io/badge/PayPal-Donate%20(nebula)-blue?style=for-the-badge&color=blue&logo=paypal)](https://www.paypal.com/paypalme/nebula2056/5)
[![Donate with PayPal](https://img.shields.io/badge/PayPal-Donate%20(johncorby)-blue?style=for-the-badge&color=blue&logo=paypal)](https://www.paypal.com/paypalme/johncorby/5)

Quantum Space Buddies (QSB) is a multiplayer mod for Outer Wilds. The mod uses the OWML mod loader and Mirror for networking.

Spoilers within!

## Installation

- [Install the Outer Wilds Mod Manager](https://outerwildsmods.com/mod-manager/)
- Install Quantum Space Buddies from the mod list displayed in the application

## Hosting / Connecting

#### Connecting to a server

- Make sure to have Steam open and logged in.
- On the title screen, click the option `CONNECT TO MULTIPLAYER`.
- Enter the Steam ID of the person you are trying to connect to.
    - If "Use KCP Transport" is enabled, enter the public IP address of the person instead.
- Enjoy!

#### Hosting a server

- Make sure to have Steam open and logged in.
- On the title screen, click the option `OPEN TO MULTIPLAYER`.
- Share your Steam ID with the people who want to connect.
    - If "Use KCP Transport" is enabled, share your public IP address instead. This can be found on websites like https://www.whatismyip.com/.
- Enjoy!

## Frequently Asked Questions

### I keep timing out when trying to connect!
Check the mod settings for "Use KCP Transport". You have to forward port 7777 as TCP/UDP (or a custom port number, which can be set in the mod settings), or use Hamachi. ***All players must either be using KCP, or not using KCP.***

### Why does SpaceWar show up in my Steam library?
This is for players who own the game on Epic or Xbox. Steam networking only works if you own the game on Steam, so we have to pretend to be SpaceWar (which every Steam user owns) so everyone can play.

### Requirements
- Latest version of OWML.
- Latest version of Mod Manager. (If using)
- Latest version of Outer Wilds. **We cannot guarantee QSB, or OWML, will work on cracked/pirated versions of Outer Wilds. Do not come asking us for help when using pirated versions.**
- Fast and stable internet connection, upload and download.
- Above minimum Outer Wilds system requirements.

### How complete is this mod? How far through the game can I play?

You can play the entire game, plus DLC!
There still might be one or two small mechanics that aren't synced - let us know if you find an obvious one that we've missed.
Also, you might encounter bugs that mean you can't progress in multiplayer. Again, let us know if you find one!

### Compatibility with other mods

QSB relies on object hierarchy to sync objects, so any mod that changes that risks breaking QSB.
QSB also relies on certain game events being called when things happen in-game, so any mod that makes these things happen without calling the correct events will break QSB.

Most small mods will work fine. The more complex and far reaching the mod, the less likely it will work completely.
Try as many mods as you like, but don't be surprised if things break.

#### NomaiVr

[Here](https://github.com/qsb-dev/quantum-space-buddies/issues?q=is%3Aissue+is%3Aopen+label%3ANomaiVR) are the known issues. You are welcome to add to this list by creating issues.

Most things seem to work _enough_. There are some visual bugs, and I believe a few softlocks, but the experience shouldn't be too bad.

We haven't done too much work to make them compatible, so the things that are broken are unlikely to be fixed.

#### New Horizons

[Here](https://github.com/qsb-dev/quantum-space-buddies/issues?q=is%3Aissue+is%3Aopen+label%3A%22New+Horizons%22) are the known issues. You are welcome to add to this list by creating issues.

We do our best to stay mostly compatible with base New Horizons, but the compatibility of each addon is mixed.
Most of them at least partially work. Most custom mechanics will not work until the addon developer explicitly adds QSB support.

### Why do I keep getting thrown around the ship?

Boring boring physics stuff. The velocity of the ship is synced, as well as the angular velocity. However, this velocity is not also applied to the player. (Or it is sometimes. I don't 100% know.) This means the ship will accelerate, leaving the player "behind". Which makes you fly into the walls alot.

To fix this, whilst in the ship you can attach yourself to it. Look at the top-left of your screen when inside the ship for the buttons to press.

### What's the difference between QSB and Outer Wilds Online?

TL;DR - QSB is multiplayer co-op, Outer Wilds Online is multiplayer not co-op.

QSB is a fully synced game. The other players are actually there in the world, and can affect things. The loop starts/ends at the same time for everyone, and you share ship logs / signal discoveries.

Outer Wilds Online is easier to set up, but much more basic in its features. The other players cannot affect your game, and do not contribute to anything in your save. The loop is entirely per-player.

## Translating

See [TRANSLATING.md](TRANSLATING.md)

## Development Setup / Contributing / Debugging

See the [QSB Developer Wiki](https://github.com/qsb-dev/quantum-space-buddies/wiki)

## Authors and Special Thanks

### Authors

- [\_nebula](https://github.com/misternebula) - Lead Dev *(v0.3.0 onwards.)*
- [JohnCorby](https://github.com/JohnCorby) - Lead Dev *(v0.13.0 onwards)*
- [AmazingAlek](https://github.com/amazingalek) - Ex-Developer *(v0.1.0 - v0.7.1)*
- [Raicuparta](https://github.com/Raicuparta) - Ex-Developer *(v0.1.0 - v0.2.0)*

### Contributers

- [xen](https://github.com/xen-42) - Help with syncing particle/sound effects, fixing lantern item bugs, and syncing addon data.
- [Moonstone](https://github.com/MoonstoneStudios) - Improvements to elevators and lifts.
- [Chris Yeninas](https://github.com/PhantomGamers) - Help with project files and GitHub workflows.
- [Locochoco](https://github.com/loco-choco) - Code improvements.

### Translators

- [Tlya](https://github.com/Tllya) - Russian translation.
- [xen](https://github.com/xen-42) and [MerlinConnected](https://github.com/MerlinConnected) - French translation.
- [Locochoco](https://github.com/loco-choco) - Portuguese translation.
- [DertolleDude](https://github.com/DertolleDude) - German translation.
- [SakuradaYuki](https://github.com/SakuradaYuki) and [isrecalpear](https://github.com/isrecalpear) - Chinese translation.
- [poleshe](https://github.com/poleshe) - Spanish translation.
- [Deniz](https://github.com/dumbdeniz) - Turkish translation.
- [Akasha0513](https://github.com/Akasha0513) - Korean translation.

### Special Thanks
- Thanks to Logan Ver Hoef for help with the game code, and for helping make the damn game in the first place.
- Thanks to all the people who helped in public tests.

### Dependencies

- [OWML](https://github.com/amazingalek/owml)
- [Mirror](https://mirror-networking.com/)
    - [kcp2k](https://github.com/vis2k/kcp2k)
    - [Telepathy](https://github.com/vis2k/Telepathy)
- [FizzySteamworks](https://github.com/Chykary/FizzySteamworks)
- [HarmonyX](https://github.com/BepInEx/HarmonyX)
- [UniTask](https://github.com/Cysharp/UniTask)
- Modified code from [Popcron's Gizmos](https://github.com/popcron/gizmos)

## Help / Discuss development / Whatever

[Join the Outer Wilds Modding Discord](https://discord.gg/9vE5aHxcF9), we have a nice `#qsb-bugs-and-questions` channel for support, and other channels to discuss modding!

## License and legal stuff

Copyright (C) 2020 - 2025 : 
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

This work is unofficial Fan Content created under permission from the Mobius Digital Fan Content Policy. It includes materials which are the property of Mobius Digital and it is neither approved nor endorsed by Mobius Digital.
