using Mirror;

namespace QSB.TimeSync;

public class PreserveTimeScale : NetworkBehaviour
{
	public void Init()
	{
		if (!isServer)
		{
			var campfires = FindObjectsOfType<Campfire>();
			foreach (var campfire in campfires)
			{
				campfire._canSleepHere = false;
			}
		}

		var menuManager = Locator.GetSceneMenuManager();

		if (menuManager == null)
		{
			return;
		}

		if (menuManager._pauseMenu == null || menuManager.pauseMenu._skipToNextLoopButton == null)
		{
			return;
		}

		menuManager.pauseMenu._skipToNextLoopButton.SetActive(false);
	}
}