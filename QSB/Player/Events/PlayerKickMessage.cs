using QSB.Menus;
using QSB.Messaging;
using QSB.Utility;
using QuantumUNET;
using System.Linq;

namespace QSB.Player.Events
{
	// sent by the server only
	internal class PlayerKickMessage : QSBEnumMessage<KickReason>
	{
		public uint PlayerId;

		public override void OnReceiveLocal()
		{
			if (!QSBCore.IsHost)
			{
				return;
			}

			QSBCore.UnityEvents.FireInNUpdates(KickPlayer, 10);
		}

		private void KickPlayer()
			=> QNetworkServer.connections.First(x => PlayerId == x.GetPlayerId()).Disconnect();

		public override void OnReceiveRemote()
		{
			if (PlayerId != QSBPlayerManager.LocalPlayerId)
			{
				if (QSBPlayerManager.PlayerExists(PlayerId))
				{
					DebugLog.ToAll($"{QSBPlayerManager.GetPlayer(PlayerId).Name} was kicked.");
					return;
				}

				DebugLog.ToAll($"Player id:{PlayerId} was kicked.");
				return;
			}

			DebugLog.ToAll($"Kicked from server. Reason : {Value}");
			MenuManager.Instance.OnKicked(Value);
		}
	}
}
