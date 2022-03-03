using UnityEngine;

namespace QSB.TimeSync;

public class StopMeditation : MonoBehaviour
{
	public void Init()
	{
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