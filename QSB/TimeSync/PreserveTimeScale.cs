using QSB.Utility;
using UnityEngine;

namespace QSB.TimeSync
{
	public class PreserveTimeScale : MonoBehaviour, IAddComponentOnStart
	{
		public void Init()
		{
			if (!QSBCore.IsHost)
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
}