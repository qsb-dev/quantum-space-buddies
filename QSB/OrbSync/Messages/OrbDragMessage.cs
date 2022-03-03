using QSB.Messaging;
using QSB.OrbSync.WorldObjects;

namespace QSB.OrbSync.Messages
{
	public class OrbDragMessage : QSBWorldObjectMessage<QSBOrb, bool>
	{
		public OrbDragMessage(bool isDragging) => Data = isDragging;

		public override void OnReceiveRemote() => WorldObject.SetDragging(Data);
	}
}