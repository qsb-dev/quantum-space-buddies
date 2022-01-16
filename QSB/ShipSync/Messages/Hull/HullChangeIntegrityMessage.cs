using QSB.Messaging;
using QSB.ShipSync.WorldObjects;

namespace QSB.ShipSync.Messages.Hull
{
	internal class HullChangeIntegrityMessage : QSBFloatWorldObjectMessage<QSBShipHull>
	{
		public HullChangeIntegrityMessage(float integrity) => Value = integrity;

		public override void OnReceiveRemote() => WorldObject.ChangeIntegrity(Value);
	}
}
