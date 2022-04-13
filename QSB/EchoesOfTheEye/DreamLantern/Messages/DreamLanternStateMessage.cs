using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.EchoesOfTheEye.DreamLantern.Messages;

internal class DreamLanternStateMessage : QSBMessage<(DreamLanternActionType Type, bool BoolValue, float FloatValue)>
{
	public DreamLanternStateMessage(DreamLanternActionType actionType, bool boolValue = false, float floatValue = 0f)
		: base((actionType, boolValue, floatValue)) { }

	public override void OnReceiveRemote()
	{
		var heldItem = QSBPlayerManager.GetPlayer(From).HeldItem;

		if (heldItem is not QSBDreamLanternItem lantern)
		{
			DebugLog.ToConsole($"Error - Got DreamLanternStateMessage from player {From}, but they are not holding a QSBDreamLanternItem!", OWML.Common.MessageType.Error);
			return;
		}

		var controller = lantern.AttachedObject._lanternController;

		switch (Data.Type)
		{
			case DreamLanternActionType.CONCEAL:
				DebugLog.DebugWrite($"CONCEAL {lantern.AttachedObject.name}");
				controller.SetConcealed(Data.BoolValue);
				break;
			case DreamLanternActionType.FOCUS:
				controller.SetFocus(Data.FloatValue);
				break;
		}
	}
}
