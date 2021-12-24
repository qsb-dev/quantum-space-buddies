using QSB.EyeOfTheUniverse.InstrumentSync.WorldObjects;
using QSB.Messaging;

namespace QSB.EyeOfTheUniverse.InstrumentSync.Messages
{
	internal class GatherInstrumentMessage : QSBWorldObjectMessage<QSBQuantumInstrument>
	{
		public override void OnReceiveRemote() => WorldObject.AttachedObject.Gather();
	}
}
