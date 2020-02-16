# QSB Helper Unity Project

This Unity project is mainly used for generating asset bundles to be imported by QSB at runtime. It is a Unity 2017.4 project, since that's the version used by Outer Wilds.

There's a "Build Assetbundles" option that generates the assetbundles in `UnityProject/Assets/AssetBundles`.

Detailed instructions:

* Create a new GameObject on the scene;
* Change the name of the GameObject on the inspector panel;
* Add models, behaviours, etc to that GameObject;
* Create a prefab from that GameObject by dragging it from the scene hierarchy to the assets panel (usually at the bottom);
* Select the newly created prefab;
* On the bottom right, select the AssetBundle for that prefab (either from an existing one, or create one);
* Save the scene (File -> Save Scenes);
* Select "Assets" -> "Build Assetbundles" from the top menu;
* The new asset bundles will be generated in `QSB/AssetBundles`;
* When you build QSB in Visual Studio, the asset bundle files will be copied to the mod folder.
