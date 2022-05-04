using HarmonyLib;
using OWML.Common;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using System;
using UnityEngine;

namespace QSB;

public static class QSBSceneManager
{
	public static OWScene CurrentScene => LoadManager.GetCurrentScene();

	public static bool IsInUniverse => CurrentScene.IsUniverseScene();

	[Obsolete]
	public static event Action<OWScene, OWScene, bool> OnSceneLoaded;
	[Obsolete]
	public static event Action<OWScene, OWScene> OnUniverseSceneLoaded;

	/// <summary>
	/// runs before objects are destroyed
	/// </summary>
	public static event LoadManager.SceneLoadEvent OnPreSceneLoad;
	/// <summary>
	/// runs after objects are awakened and started
	/// </summary>
	public static event LoadManager.SceneLoadEvent OnPostSceneLoad;

	static QSBSceneManager()
	{
		LoadManager.OnStartSceneLoad += (originalScene, loadScene) =>
		{
			DebugLog.DebugWrite($"PRE SCENE LOAD ({originalScene} -> {loadScene})", MessageType.Info);
			OnPreSceneLoad?.SafeInvoke(originalScene, loadScene);
		};
		LoadManager.OnCompleteSceneLoad += (originalScene, loadScene) =>
			Delay.RunNextFrame(() =>
			{
				DebugLog.DebugWrite($"POST SCENE LOAD ({originalScene} -> {loadScene})", MessageType.Info);
				OnPostSceneLoad?.SafeInvoke(originalScene, loadScene);
			});

		OnPreSceneLoad += (_, _) =>
			QSBWorldSync.RemoveWorldObjects();
		OnPostSceneLoad += (_, loadScene) =>
		{
			if (QSBCore.IsInMultiplayer)
			{
				if (loadScene.IsUniverseScene())
				{
					QSBWorldSync.BuildWorldObjects(loadScene).Forget();
				}

				if (loadScene == OWScene.TitleScreen)
				{
					QSBNetworkManager.singleton.StopHost();
				}
			}
		};

		DebugLog.DebugWrite("Scene Manager ready.", MessageType.Success);
	}

	public static bool IsUniverseScene(this OWScene scene) =>
		scene is OWScene.SolarSystem or OWScene.EyeOfTheUniverse;
}

[HarmonyPatch(typeof(GhostBrain))]
internal class TestPatch : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.Awake))]
	private static void Awake() => DebugLog.DebugWrite("GhostBrain.Awake");

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.Start))]
	private static void Start() => DebugLog.DebugWrite("GhostBrain.Start");

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.OnDestroy))]
	private static void OnDestroy() => DebugLog.DebugWrite("GhostBrain.OnDestroy");
}
