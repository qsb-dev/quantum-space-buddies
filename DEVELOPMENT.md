> :warning: Warning! :warning:  
Mod development needs a powerful PC!  
Unexpected errors and issues may occur when editing networking code.  
Running multiple instances of the game can be very taxing on your computer.  
We're not responsible if you push your PC too hard.

## Prerequisites
- Visual Studio 2022.
- Epic or Steam version of Outer Wilds.
- Keyboard with numpad for in-game debug actions.

We recommend using the Outer Wilds Mod Manager, but you can use OWML on its own if you want.

## Cloning and configuration
- Clone QSB's source code.
- Copy the file `DevEnv.template.targets` and rename it to `DevEnv.targets`.
- In `DevEnv.targets`, edit the entry for `<OwmlDir>` to point to your installation of OWML. This should be the folder named `OWML`. If using the manager, you can find this directory by :
	- Legacy Manager : Press the "Mods Directory" button and go up a folder.
	- New Manager : Press the "..." button at the top, and select "Show OWML Folder".
- `QSB.sln` should now be ready to open. ***This solution needs to be opened with Visual Studio 2022 or higher!***
 
## Steam
If using the Steam version of Outer Wilds, you will need to create a file to allow you to run multiple instances of the game.
- Navigate to your game install folder. You can find this by right-clicking on the game in Steam, and going `Manage > Browse local files`.
- Create a file named `steam_appid.txt`.
- In this file, write `753640` and save.
This file will override some Steam DRM features and allow the game to be ran multiple times at once.

## Building
Simply build the solution normally. (`Build > Build Solution` or CTRL-SHIFT-B)

The files will automatically be copied over to your OWML installation and be ready to play - no DLL copying needed.

For documentation reasons, here is the build flow :

- MirrorWeaver is built.
- EpicOnlineTransport is built.
- EpicRerouter is built.
- QSB is built.
- Any `.exe.config` files are removed from the build.
- QSB.dll is processed ("weaved") by MirrorWeaver. This injects all the boilerplate code that Mirror needs to function.
- If needed/possible, any `.dll` or `.exe` files are copied to the Unity project.

## Debugging
### Debug Actions :

Hold Q and press :

- Numpad 1 - Teleport to nearest player.
- Numpad 2 - If holding LeftShift, warp to the dreamworld Vault fire. If not, warp to the Endless Canyon.
- Numpad 3 - Unlock the Sealed Vault.
- Numpad 4 - Damage the ship's electrical system.
- Numpad 5 - Trigger the supernova.
- Numpad 6 - Set the flags for having met Solanum and the Prisoner.
- Numpad 7 - Warp to the Vessel.
- Numpad 8 - Insert the Advanced Warp Core into the Vessel.
- Numpad 9 - If holding LeftShift, load the SolarSystem scene. If not, load the EyeOfTheUniverse scene.
- Numpad 0 - Revive a random dead player.

### Debug Settings :

Create a file called `debugsettings.json` in the mod folder.
The template for this file is this :

```json
{
  "dumpWorldObjects": false,
  "instanceIdInLogs": false,
  "hookDebugLogs": false,
  "avoidTimeSync": false,
  "autoStart": false,
  "kickEveryone": false,
  "disableLoopDeath": false,
  "debugMode": false,
  "drawGui": false,
  "drawLines": false,
  "drawLabels": false,
  "drawQuantumVisibilityObjects": false,
  "drawGhostAI": false,
  "greySkybox": false
}
```

- dumpWorldObjects - Creates a file with information about the WorldObjects that were created.
- instanceIdInLogs - Appends the game instance id to every log message sent.
- hookDebugLogs - Print Unity logs and warnings.
- avoidTimeSync - Disables the syncing of time.
- autoStart - Host/connect automatically for faster testing.
- kickEveryone - Kick anyone who joins a game.
- disableLoopDeath - Make it so the loop doesn't end when everyone is dead.
- debugMode - Enables debug mode. If this is set to `false`, none of the following settings do anything.
- drawGui - Draws a GUI at the top of the screen that gives information on many things.
- drawLines - Draws gizmo-esque lines around things. Indicates reference sectors/transforms, triggers, etc. LAGGY.
- drawLabels - Draws GUI labels attached to some objects. LAGGY.
- drawQuantumVisibilityObjects - Indicates visibility objects with an orange shape.
- drawGhostAI - Draws debug lines and labels just for the ghosts.
- greySkybox - Turns the skybox grey. Useful in the Eye, where it's pretty dark.

