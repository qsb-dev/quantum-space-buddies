using QSB.EyeOfTheUniverse.InstrumentSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.EyeOfTheUniverse.InstrumentSync
{
	internal class QuantumInstrumentManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBQuantumInstrument, QuantumInstrument>();
	}
}
