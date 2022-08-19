using QSB.Messaging;
using QSB.Patches;
using QSB.WarpPadSync.WorldObjects;

namespace QSB.WarpPadSync.Messages;

public class CloseBlackHoleMessage : QSBWorldObjectMessage<QSBWarpPad>
{
	public override void OnReceiveRemote() =>
		QSBPatch.RemoteCall(() => WorldObject.AttachedObject.CloseBlackHole());
}
