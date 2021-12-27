using OWML.Utils;
using QuantumUNET;

namespace QSB.TimeSync
{
	public class PreserveTimeScale : QNetworkBehaviour
	{
		public void Start()
		{
			// BUG : Get this working for the new menu system. Can't use OWML's anymore.
			//QSBCore.Helper.Menus.PauseMenu.GetTitleButton("Button-EndCurrentLoop").Hide(); // Remove the meditation button

			// Allow server to sleep at campfires
			if (IsServer)
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