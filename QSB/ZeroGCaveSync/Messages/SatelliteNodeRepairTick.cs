using QSB.Events;
using QSB.ShipSync.Messages;
using QSB.WorldSync;
using QSB.ZeroGCaveSync.WorldObjects;

namespace QSB.ZeroGCaveSync.Messages
{
	internal class SatelliteNodeRepairTick : QSBEvent<RepairTickMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<SatelliteNode, float>.AddListener(EventNames.QSBSatelliteRepairTick, Handler);
		public override void CloseListener() => GlobalMessenger<SatelliteNode, float>.RemoveListener(EventNames.QSBSatelliteRepairTick, Handler);

		private void Handler(SatelliteNode node, float repairFraction) => SendEvent(CreateMessage(node, repairFraction));

		private RepairTickMessage CreateMessage(SatelliteNode node, float repairFraction)
		{
			var worldObject = QSBWorldSync.GetWorldFromUnity<QSBSatelliteNode>(node);
			return new RepairTickMessage
			{
				AboutId = LocalPlayerId,
				ObjectId = worldObject.ObjectId,
				RepairNumber = repairFraction
			};
		}

		public override void OnReceiveRemote(bool server, RepairTickMessage message)
		{
			var worldObject = QSBWorldSync.GetWorldFromId<QSBSatelliteNode>(message.ObjectId);
			worldObject.RepairTick(message.RepairNumber);
		}
	}
}
