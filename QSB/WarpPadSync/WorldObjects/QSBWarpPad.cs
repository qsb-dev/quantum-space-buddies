using QSB.Messaging;
using QSB.WarpPadSync.Messages;
using QSB.WorldSync;

namespace QSB.WarpPadSync.WorldObjects;

public class QSBWarpPad : WorldObject<NomaiWarpPlatform>
{
	public override void SendInitialState(uint to)
	{
		if (AttachedObject.IsBlackHoleOpen())
		{
			this.SendMessage(new OpenBlackHoleMessage(
				AttachedObject._linkedPlatform.GetWorldObject<QSBWarpPad>().ObjectId,
				AttachedObject._keepBlackHoleOpen
			));
		}
	}
}
