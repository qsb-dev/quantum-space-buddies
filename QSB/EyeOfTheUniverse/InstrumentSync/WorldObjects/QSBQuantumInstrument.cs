using QSB.EyeOfTheUniverse.MaskSync;
using QSB.WorldSync;

namespace QSB.EyeOfTheUniverse.InstrumentSync.WorldObjects;

public class QSBQuantumInstrument : WorldObject<QuantumInstrument>
{
	public void Gather()
	{
		var maskZoneController = QSBWorldSync.GetUnityObject<MaskZoneController>();
		if (maskZoneController._maskInstrument == AttachedObject)
		{
			var shuttleController = QSBWorldSync.GetUnityObject<EyeShuttleController>();

			foreach (var player in MaskManager.WentOnSolanumsWildRide)
			{
				player.SetVisible(true, 2);
				player.OnSolanumsWildRide = false;
			}

			maskZoneController._whiteSphere.SetActive(false);
			shuttleController._maskObject.SetActive(true);
		}

		AttachedObject.Gather();
	}
}
