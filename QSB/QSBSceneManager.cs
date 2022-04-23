using OWML.Common;
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

	static QSBSceneManager()
	{
		LoadManager.OnStartSceneLoad += OnStartSceneLoad;
		LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
		DebugLog.DebugWrite("Scene Manager ready.", MessageType.Success);
	}

	private static void OnStartSceneLoad(OWScene oldScene, OWScene newScene)
	{
		DebugLog.DebugWrite($"START SCENE LOAD ({oldScene} -> {newScene})", MessageType.Info);
		QSBWorldSync.RemoveWorldObjects();
		DeterministicManager.OnStartSceneLoad();
	}

	private static void OnCompleteSceneLoad(OWScene oldScene, OWScene newScene)
	{
		DebugLog.DebugWrite($"COMPLETE SCENE LOAD ({oldScene} -> {newScene})", MessageType.Info);
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
