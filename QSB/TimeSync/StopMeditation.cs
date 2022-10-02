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
			Delay.RunWhen(() => Locator.GetSceneMenuManager() != null, Init);
			return;
		}

		if (menuManager._pauseMenu == null)
		{
			Delay.RunWhen(() => Locator.GetSceneMenuManager().pauseMenu != null, Init);
			return;
		}

		if (menuManager.pauseMenu._skipToNextLoopButton == null)
		{
			Delay.RunWhen(() => Locator.GetSceneMenuManager().pauseMenu._skipToNextLoopButton != null, Init);
			return;
		}

		menuManager.pauseMenu._skipToNextLoopButton.SetActive(false);
	}
}