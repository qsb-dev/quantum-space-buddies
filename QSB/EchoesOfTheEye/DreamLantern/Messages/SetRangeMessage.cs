using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.EchoesOfTheEye.DreamLantern.Messages;

internal class SetRangeMessage : QSBWorldObjectMessage<QSBDreamLantern, (float minRange, float maxRange)>
{
	public SetRangeMessage(float minRange, float maxRange) : base((minRange, maxRange)) { }

	public override void OnReceiveRemote()
		=> QSBPatch.RemoteCall(() => WorldObject.AttachedObject.SetRange(Data.minRange, Data.maxRange));
}
