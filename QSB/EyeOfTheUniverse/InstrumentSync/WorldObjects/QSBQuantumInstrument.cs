using QSB.EyeOfTheUniverse.MaskSync;
using QSB.WorldSync;
using System.Linq;

namespace QSB.EyeOfTheUniverse.InstrumentSync.WorldObjects
{
	internal class QSBQuantumInstrument : WorldObject<QuantumInstrument>
	{
		public void Gather()
		{
			var maskZoneController = QSBWorldSync.GetUnityObjects<MaskZoneController>().First();
			if (maskZoneController._maskInstrument == AttachedObject)
			{
				var shuttleController = QSBWorldSync.GetUnityObjects<EyeShuttleController>().First();

				foreach (var player in MaskManager.WentOnSolanumsWildRide)
				{
					player.DitheringAnimator.SetVisible(true, 0.5f);
				}

				maskZoneController._whiteSphere.SetActive(false);
				shuttleController._maskObject.SetActive(true);
			}

			AttachedObject.Gather();
		}
	}
}
