using QSB.Messaging;
using QSB.Patches;
using QSB.WarpPadSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.WarpPadSync.Messages;

public class OpenBlackHoleMessage : QSBWorldObjectMessage<QSBWarpPad, (int linkedPlatform, bool stayOpen)>
{
	public OpenBlackHoleMessage(int linkedPlatform, bool stayOpen) : base((linkedPlatform, stayOpen)) { }

	public override void OnReceiveRemote() =>
		QSBPatch.RemoteCall(() => WorldObject.AttachedObject.OpenBlackHole(
			Data.linkedPlatform.GetWorldObject<QSBWarpPad>().AttachedObject,
			Data.stayOpen
		));
}
