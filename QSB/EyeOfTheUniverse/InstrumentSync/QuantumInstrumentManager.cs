using QSB.EyeOfTheUniverse.InstrumentSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.EyeOfTheUniverse.InstrumentSync
{
	internal class QuantumInstrumentManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.Eye;

		public override void BuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBQuantumInstrument, QuantumInstrument>();
	}
}
