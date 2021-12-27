using QSB.Messaging;
using QSB.ShipSync.WorldObjects;

namespace QSB.ShipSync.Messages.Hull
{
	internal class HullRepairTickMessage : QSBFloatWorldObjectMessage<QSBShipHull>
	{
		public HullRepairTickMessage(float repairFraction) => Value = repairFraction;

		public override void OnReceiveRemote() => WorldObject.RepairTick(Value);
	}
}
