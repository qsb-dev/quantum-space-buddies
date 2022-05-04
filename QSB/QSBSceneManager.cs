using HarmonyLib;
using OWML.Common;
using QSB.Patches;
using QSB.Utility;
using System;

namespace QSB;

public static class QSBSceneManager
{
	public static OWScene CurrentScene => LoadManager.GetCurrentScene();

	public static bool IsInUniverse => CurrentScene.IsUniverseScene();

	[Obsolete("TODO: remove after migration")]
	public static event Action<OWScene, OWScene, bool> OnSceneLoaded;
	[Obsolete("TODO: remove after migration")]
	public static event Action<OWScene, OWScene> OnUniverseSceneLoaded;

	/// <summary>
	/// runs before the scene is changed.
	/// happens before OnDestroy.
	/// </summary>
	public static event LoadManager.SceneLoadEvent OnPreSceneLoad;
	/// <summary>
	/// runs after the scene is changed.
	/// happens after Awake, but before Start.
	/// </summary>
	public static event LoadManager.SceneLoadEvent OnPostSceneLoad;

	static QSBSceneManager()
	{
		LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;

		LoadManager.OnStartSceneLoad += (originalScene, loadScene) =>
		{
			DebugLog.DebugWrite($"PRE SCENE LOAD ({originalScene} -> {loadScene})", MessageType.Info);
			OnPreSceneLoad?.SafeInvoke(originalScene, loadScene);
		};
		LoadManager.OnCompleteSceneLoad += (originalScene, loadScene) =>
		{
			DebugLog.DebugWrite($"POST SCENE LOAD ({originalScene} -> {loadScene})", MessageType.Info);
			OnPostSceneLoad?.SafeInvoke(originalScene, loadScene);
		};

		DebugLog.DebugWrite("Scene Manager ready.", MessageType.Success);
	}

	[Obsolete("TODO: remove after migration")]
	private static void OnCompleteSceneLoad(OWScene oldScene, OWScene newScene)
	{
		var universe = newScene.IsUniverseScene();

		OnSceneLoaded?.SafeInvoke(oldScene, newScene, universe);
		if (universe)
		{
			OnUniverseSceneLoaded?.SafeInvoke(oldScene, newScene);
		}
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
