# Quantum Space Buddies - Outer Wilds Online Multiplayer Mod

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
