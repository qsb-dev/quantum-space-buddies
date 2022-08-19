using QSB.Messaging;
using QSB.WarpPadSync.Messages;
using QSB.WorldSync;

namespace QSB.WarpPadSync.WorldObjects;

public class QSBWarpPad : WorldObject<NomaiWarpPlatform>
{
	public override void SendInitialState(uint to) =>
		this.SendMessage(new OpenCloseMessage(AttachedObject.IsBlackHoleOpen(), AttachedObject._linkedPlatform) { To = to });
}
