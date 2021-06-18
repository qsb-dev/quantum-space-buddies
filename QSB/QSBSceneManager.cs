using OWML.Common;
using QSB.Utility;
using QSB.WorldSync;
using System;

namespace QSB
{
	public static class QSBSceneManager
	{
		public static OWScene CurrentScene => LoadManager.GetCurrentScene();

		public static bool IsInUniverse => InUniverse(CurrentScene);

		public static event Action<OWScene, bool> OnSceneLoaded;
		public static event Action<OWScene> OnUniverseSceneLoaded;

		static QSBSceneManager()
		{
			LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
			DebugLog.DebugWrite("Scene Manager ready.", MessageType.Success);
		}

		private static void OnCompleteSceneLoad(OWScene oldScene, OWScene newScene)
		{
			DebugLog.DebugWrite($"COMPLETE SCENE LOAD ({oldScene} -> {newScene})", MessageType.Info);
			var universe = InUniverse(newScene);
			if (QSBCore.IsInMultiplayer && universe)
			{
				// So objects have time to be deleted, made, whatever
				QSBCore.UnityEvents.FireOnNextUpdate(() => WorldObjectManager.Rebuild(newScene));
			}

			OnSceneLoaded?.SafeInvoke(newScene, universe);
			if (universe)
			{
				OnUniverseSceneLoaded?.SafeInvoke(newScene);
			}
		}

		private static bool InUniverse(OWScene scene) =>
			scene == OWScene.SolarSystem || scene == OWScene.EyeOfTheUniverse;
	}
}