using HarmonyLib;
using OWML.Common;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using System;

namespace QSB;

public static class QSBSceneManager
{
	public static OWScene CurrentScene => LoadManager.GetCurrentScene();

	public static bool IsInUniverse => InUniverse(CurrentScene);

	public static event Action<OWScene, OWScene, bool> OnSceneLoaded;
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
		LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
		DebugLog.DebugWrite("Scene Manager ready.", MessageType.Success);

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
	}

	private static void OnCompleteSceneLoad(OWScene oldScene, OWScene newScene)
	{
		DebugLog.DebugWrite($"COMPLETE SCENE LOAD ({oldScene} -> {newScene})", MessageType.Info);
		QSBWorldSync.RemoveWorldObjects();
		var universe = InUniverse(newScene);
		if (QSBCore.IsInMultiplayer && universe)
		{
			// So objects have time to be deleted, made, whatever
			Delay.RunNextFrame(() => QSBWorldSync.BuildWorldObjects(newScene).Forget());
		}

		OnSceneLoaded?.SafeInvoke(oldScene, newScene, universe);
		if (universe)
		{
			OnUniverseSceneLoaded?.SafeInvoke(oldScene, newScene);
		}

		if (newScene == OWScene.TitleScreen && QSBCore.IsInMultiplayer)
		{
			QSBNetworkManager.singleton.StopHost();
		}
	}

	private static bool InUniverse(OWScene scene) =>
		scene is OWScene.SolarSystem or OWScene.EyeOfTheUniverse;
}

[HarmonyPatch(typeof(GhostBrain))]
internal class Patch : QSBPatch
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
