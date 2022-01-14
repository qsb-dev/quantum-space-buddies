using QSB.Player;
using QSB.WorldSync;
using System.Linq;

namespace QSB.EyeOfTheUniverse.InstrumentSync.WorldObjects
{
	internal class QSBQuantumInstrument : WorldObject<QuantumInstrument>
	{
		public void Gather()
		{
			AttachedObject.Gather();

			var maskZoneController = QSBWorldSync.GetUnityObjects<MaskZoneController>().First();
			if (maskZoneController._maskInstrument == AttachedObject)
			{
				// remote gathering solanum mask - make all players visible
				QSBPlayerManager.ShowAllPlayers();
			}
		}
	}
}
