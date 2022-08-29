using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.DreamLantern.Messages;

internal class SetRangeMessage : QSBWorldObjectMessage<QSBDreamLantern, (float minRange, float maxRange)>
{
	public SetRangeMessage(float minRange, float maxRange) : base((minRange, maxRange)) { }

	public override void OnReceiveRemote()
		=> WorldObject.AttachedObject.SetRange(Data.minRange, Data.maxRange);
}
