using QSB.Utility;
using UnityEngine;

namespace QSB.TimeSync;

[UsedInUnityProject]
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