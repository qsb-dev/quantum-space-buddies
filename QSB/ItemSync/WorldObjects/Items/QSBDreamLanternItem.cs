using QSB.EchoesOfTheEye.DreamLantern.Messages;
using QSB.Messaging;

namespace QSB.ItemSync.WorldObjects.Items;

public class QSBDreamLanternItem : QSBItem<DreamLanternItem>
{
	public override void SendInitialState(uint to)
	{
		base.SendInitialState(to);

		if (AttachedObject._lanternController != null)
		{
			this.SendMessage(new DreamLanternLitMessage(AttachedObject._lanternController.IsLit()) { To = to });
		}
	}
}
