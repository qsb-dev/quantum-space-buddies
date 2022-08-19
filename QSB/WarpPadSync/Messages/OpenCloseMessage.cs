using QSB.Messaging;
using QSB.WarpPadSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.WarpPadSync.Messages;

public class OpenCloseMessage : QSBWorldObjectMessage<QSBWarpPad, (bool open, int linkedPlatform)>
{
	public OpenCloseMessage(bool open, NomaiWarpPlatform linkedPlatform) : base((
		open,
		linkedPlatform ? linkedPlatform.GetWorldObject<QSBWarpPad>().ObjectId : -1
	)) { }

	public override void OnReceiveRemote()
	{
		if (Data.open)
		{
			WorldObject.AttachedObject._blackHole.Create();
			if (Data.linkedPlatform != -1)
			{
				Data.linkedPlatform.GetWorldObject<QSBWarpPad>().AttachedObject._whiteHole.Create();
			}
		}
		else
		{
			WorldObject.AttachedObject._blackHole.Collapse();
			if (Data.linkedPlatform != -1)
			{
				Data.linkedPlatform.GetWorldObject<QSBWarpPad>().AttachedObject._whiteHole.Collapse();
			}
		}
	}
}
