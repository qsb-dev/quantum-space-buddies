using QSB.EyeOfTheUniverse.MaskSync;
using QSB.WorldSync;
using System.Linq;

namespace QSB.EyeOfTheUniverse.InstrumentSync.WorldObjects
{
	internal class QSBQuantumInstrument : WorldObject<QuantumInstrument>
	{
		public override void SendResyncInfo(uint to)
		{
			// not needed since mid-game join is impossible here
		}

		public void Gather()
		{
			var maskZoneController = QSBWorldSync.GetUnityObjects<MaskZoneController>().First();
			if (maskZoneController._maskInstrument == AttachedObject)
			{
				var shuttleController = QSBWorldSync.GetUnityObjects<EyeShuttleController>().First();

				foreach (var player in MaskManager.WentOnSolanumsWildRide)
				{
					player.SetVisible(true, 2);
				}

				maskZoneController._whiteSphere.SetActive(false);
				shuttleController._maskObject.SetActive(true);
			}

			AttachedObject.Gather();
		}
	}
}
