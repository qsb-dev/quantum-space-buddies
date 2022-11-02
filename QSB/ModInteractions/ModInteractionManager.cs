using QSB.ModInteractions.NewHorizons;
using QSB.Utility;
using UnityEngine.SceneManagement;

namespace QSB.ModInteractions;

internal static class ModInteractionManager
{
	public static bool IsNHInstalled { get; private set; }
	public static bool IsNHReady { get; private set; }

	private static bool _init = false;

	private static INewHorizons _newHorizons;

	public static void Init()
	{
		if (_init) return;

		_init = true;

		_newHorizons = QSBCore.Helper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
		IsNHInstalled = _newHorizons != null;

		if (IsNHInstalled)
		{
			_newHorizons.GetStarSystemLoadedEvent().AddListener(OnStarSystemLoaded);
			SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
		}

		DebugLog.DebugWrite($"New Horizons is {(IsNHInstalled ? "installed" : "not installed")}");
	}

	private static void OnStarSystemLoaded(string _) => IsNHReady = true;
	private static void SceneManager_sceneUnloaded(Scene _) => IsNHReady = false;
}
