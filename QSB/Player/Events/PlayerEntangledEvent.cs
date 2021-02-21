using QSB.Events;
using QSB.QuantumSync;
using QSB.WorldSync.Events;

namespace QSB.Player.Events
{
	internal class PlayerEntangledEvent : QSBEvent<WorldObjectMessage>
	{
		public override EventType Type => EventType.PlayerEntangle;

		public override void SetupListener() => GlobalMessenger<int>.AddListener(EventNames.QSBPlayerEntangle, Handler);
		public override void CloseListener() => GlobalMessenger<int>.RemoveListener(EventNames.QSBPlayerEntangle, Handler);

		private void Handler(int id) => SendEvent(CreateMessage(id));

		private WorldObjectMessage CreateMessage(int id) => new WorldObjectMessage
		{
			AboutId = LocalPlayerId,
			ObjectId = id
		};

		public override void OnReceiveRemote(bool server, WorldObjectMessage message)
		{
			var quantumObject = QuantumManager.GetObject(message.ObjectId);
			var player = QSBPlayerManager.GetPlayer(message.AboutId);
			player.EntangledObject = quantumObject;
		}
	}
}
