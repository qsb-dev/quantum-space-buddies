using QSB.Events;
using QSB.Messaging;
using QSB.Utility;
using QuantumUNET;
using System.Linq;

namespace QSB.Player.Events
{
	internal class PlayerKickEvent : QSBEvent<EnumMessage<KickReason>>
	{
		public override EventType Type => EventType.PlayerKick;

		public override void SetupListener() => GlobalMessenger<uint, KickReason>.AddListener(EventNames.QSBPlayerKick, Handler);
		public override void CloseListener() => GlobalMessenger<uint, KickReason>.RemoveListener(EventNames.QSBPlayerKick, Handler);

		private void Handler(uint player, KickReason reason) => SendEvent(CreateMessage(player, reason));

		private EnumMessage<KickReason> CreateMessage(uint player, KickReason reason) => new EnumMessage<KickReason>
		{
			AboutId = player,
			EnumValue = reason
		};

		public override void OnReceiveLocal(bool server, EnumMessage<KickReason> message)
		{
			if (!server)
			{
				return;
			}

			QSBCore.UnityEvents.FireInNUpdates(() => KickPlayer(message.AboutId), 10);
		}

		private void KickPlayer(uint id)
			=> QNetworkServer.connections.First(x => x.GetPlayerId() == id).Disconnect();

		public override void OnReceiveRemote(bool server, EnumMessage<KickReason> message)
		{
			if (message.AboutId != QSBPlayerManager.LocalPlayerId)
			{
				if (QSBPlayerManager.PlayerExists(message.AboutId))
				{
					DebugLog.ToAll($"{QSBPlayerManager.GetPlayer(message.AboutId).Name} was kicked.");
					return;
				}

				DebugLog.ToAll($"Player id:{message.AboutId} was kicked.");
				return;
			}

			DebugLog.ToAll($"Kicked from server. Reason : {message.EnumValue}");
		}
	}
}
