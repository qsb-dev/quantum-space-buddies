using QSB.ClientServerStateSync;
using QSB.Messaging;
using QSB.Player;
using QSB.RespawnSync;
using QSB.Utility;
using QuantumUNET.Transport;

namespace QSB.DeathSync.Messages
{
	public class PlayerDeathMessage : QSBEnumMessage<DeathType>
	{
		private int NecronomiconIndex;

		public PlayerDeathMessage(DeathType type)
		{
			Value = type;
			NecronomiconIndex = Necronomicon.GetRandomIndex(type);
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(NecronomiconIndex);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			NecronomiconIndex = reader.ReadInt32();
		}

		public override void OnReceiveLocal()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			RespawnManager.Instance.OnPlayerDeath(player);
			ClientStateManager.Instance.OnDeath();
		}

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			var playerName = player.Name;
			var deathMessage = Necronomicon.GetPhrase(Value, NecronomiconIndex);
			if (deathMessage != null)
			{
				DebugLog.ToAll(string.Format(deathMessage, playerName));
			}

			RespawnManager.Instance.OnPlayerDeath(player);
		}
	}
}