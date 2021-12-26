﻿using QSB.Messaging;
using QSB.ZeroGCaveSync.WorldObjects;

namespace QSB.ZeroGCaveSync.Messages
{
	internal class SatelliteNodeRepairTickMessage : QSBFloatWorldObjectMessage<QSBSatelliteNode>
	{
		public SatelliteNodeRepairTickMessage(float repairFraction) => Value = repairFraction;

		public SatelliteNodeRepairTickMessage() { }

		public override void OnReceiveRemote() => WorldObject.RepairTick(Value);
	}
}