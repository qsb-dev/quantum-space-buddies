using QSB.Events;
using QSB.WorldSync;
using QSB.WorldSync.Events;
using QSB.ZeroGCaveSync.WorldObjects;

namespace QSB.ZeroGCaveSync.Events
{
	internal class SatelliteNodeRepaired : QSBEvent<WorldObjectMessage>
	{
		public override void SetupListener() => GlobalMessenger<SatelliteNode>.AddListener(EventNames.QSBSatelliteRepaired, Handler);
		public override void CloseListener() => GlobalMessenger<SatelliteNode>.RemoveListener(EventNames.QSBSatelliteRepaired, Handler);

		private void Handler(SatelliteNode node) => SendEvent(CreateMessage(node));

		private WorldObjectMessage CreateMessage(SatelliteNode node)
		{
			var worldObject = QSBWorldSync.GetWorldFromUnity<QSBSatelliteNode>(node);
			return new WorldObjectMessage
			{
				AboutId = LocalPlayerId,
				ObjectId = worldObject.ObjectId
			};
		}

		public override void OnReceiveRemote(bool server, WorldObjectMessage message)
		{
			var worldObject = QSBWorldSync.GetWorldFromId<QSBSatelliteNode>(message.ObjectId);
			worldObject.SetRepaired();
		}
	}
}
