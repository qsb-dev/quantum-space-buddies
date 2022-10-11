using QSB.EchoesOfTheEye.VisionTorch.Messages;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.VisionTorch.WorldObjects;

public class QSBVisionTorchItem : QSBItem<VisionTorchItem>
{
	public override void SendInitialState(uint to)
	{
		this.SendMessage(new VisionTorchProjectMessage(AttachedObject._isProjecting) { To = to });
	}
}
