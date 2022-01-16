﻿using QSB.ClientServerStateSync;
using QSB.Messaging;
using QSB.Player;
using QuantumUNET.Transport;

namespace QSB.RespawnSync.Messages
{
	internal class PlayerRespawnMessage : QSBMessage
	{
		private uint PlayerId;

		public PlayerRespawnMessage(uint playerId) => PlayerId = playerId;

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(PlayerId);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			PlayerId = reader.ReadUInt32();
		}

		public override void OnReceiveLocal() => OnReceiveRemote();

		public override void OnReceiveRemote()
		{
			if (PlayerId == QSBPlayerManager.LocalPlayerId)
			{
				RespawnManager.Instance.Respawn();
				ClientStateManager.Instance.OnRespawn();
			}

			RespawnManager.Instance.OnPlayerRespawn(QSBPlayerManager.GetPlayer(PlayerId));
		}
	}
}