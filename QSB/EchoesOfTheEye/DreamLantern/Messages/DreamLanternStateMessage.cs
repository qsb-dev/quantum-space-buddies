using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.EchoesOfTheEye.DreamLantern.Messages;

internal class DreamLanternStateMessage : QSBMessage<DreamLanternActionType, bool, float>
{
	public DreamLanternStateMessage(DreamLanternActionType actionType, bool state = false, float floatValue = 0f)
	{
		Value1 = actionType;
		Value2 = state;
		Value3 = floatValue;
	}

	public override void OnReceiveRemote()
	{
		DebugLog.DebugWrite($"{From} Action:{Value1} Value:{Value2} FloatValue:{Value3}");

		var heldItem = QSBPlayerManager.GetPlayer(From).HeldItem;

		if (heldItem is not QSBDreamLanternItem lantern)
		{
			DebugLog.ToConsole($"Error - Got DreamLanternStateMessage from player {From}, but they are not holding a QSBDreamLanternItem!");
			return;
		}

		var controller = lantern.AttachedObject._lanternController;

		switch (Value1)
		{
			case DreamLanternActionType.CONCEAL:
				controller.SetConcealed(Value2);
				break;
			case DreamLanternActionType.FOCUS:
				controller.SetFocus(Value3);
				break;
		}
	}
}
