using QSB.EyeOfTheUniverse.InstrumentSync.WorldObjects;
using QSB.Messaging;

namespace QSB.EyeOfTheUniverse.InstrumentSync.Messages;

public class GatherInstrumentMessage : QSBWorldObjectMessage<QSBQuantumInstrument>
{
	public override void OnReceiveRemote() => WorldObject.Gather();
}