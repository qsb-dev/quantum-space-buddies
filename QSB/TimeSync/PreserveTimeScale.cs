using Mirror;

namespace QSB.TimeSync
{
	public class PreserveTimeScale : NetworkBehaviour
	{
		public void Start()
		{
			// disable meditation button;
			Locator.GetSceneMenuManager().pauseMenu._skipToNextLoopButton.SetActive(false);

			// Allow server to sleep at campfires
			if (isServer)
			{
				return;
			}

			var campfires = FindObjectsOfType<Campfire>();
			foreach (var campfire in campfires)
			{
				campfire._canSleepHere = false; // Stop players from sleeping at campfires
			}
		}
	}
}