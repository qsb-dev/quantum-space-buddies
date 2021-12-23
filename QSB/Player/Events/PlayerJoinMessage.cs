using OWML.Common;
using QSB.Events;
using QSB.Messaging;
using QSB.Utility;
using QuantumUNET.Transport;

namespace QSB.Player.Events
{
	public class PlayerJoinMessage : QSBMessage
	{
		private string PlayerName;
		private string QSBVersion;
		private string GameVersion;
		private GamePlatform Platform;
		private bool DlcInstalled;

		public PlayerJoinMessage(string name)
		{
			PlayerName = name;
			QSBVersion = QSBCore.QSBVersion;
			GameVersion = QSBCore.GameVersion;
			Platform = QSBCore.Platform;
			DlcInstalled = QSBCore.DLCInstalled;
		}

		public PlayerJoinMessage() { }

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(PlayerName);
			writer.Write(QSBVersion);
			writer.Write(GameVersion);
			writer.Write((int)Platform);
			writer.Write(DlcInstalled);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			PlayerName = reader.ReadString();
			QSBVersion = reader.ReadString();
			GameVersion = reader.ReadString();
			Platform = (GamePlatform)reader.ReadInt32();
			DlcInstalled = reader.ReadBoolean();
		}

		public override void OnReceiveRemote()
		{
			if (QSBVersion != QSBCore.QSBVersion)
			{
				if (QSBCore.IsHost)
				{
					DebugLog.ToConsole($"Error - Client {PlayerName} connecting with wrong QSB version. (Client:{QSBVersion}, Server:{QSBCore.QSBVersion})", MessageType.Error);
					new PlayerKickMessage(From, KickReason.QSBVersionNotMatching).Send();
				}

				return;
			}

			if (GameVersion != QSBCore.GameVersion)
			{
				if (QSBCore.IsHost)
				{
					DebugLog.ToConsole($"Error - Client {PlayerName} connecting with wrong game version. (Client:{GameVersion}, Server:{QSBCore.GameVersion})", MessageType.Error);
					new PlayerKickMessage(From, KickReason.GameVersionNotMatching).Send();
				}

				return;
			}

			if (Platform != QSBCore.Platform)
			{
				if (QSBCore.IsHost)
				{
					DebugLog.ToConsole($"Error - Client {PlayerName} connecting with wrong game platform. (Client:{Platform}, Server:{QSBCore.Platform})", MessageType.Error);
					new PlayerKickMessage(From, KickReason.DLCNotMatching).Send();
				}
			}

			if (DlcInstalled != QSBCore.DLCInstalled)
			{
				if (QSBCore.IsHost)
				{
					DebugLog.ToConsole($"Error - Client {PlayerName} connecting with wrong DLC installation state. (Client:{DlcInstalled}, Server:{QSBCore.DLCInstalled})", MessageType.Error);
					new PlayerKickMessage(From, KickReason.GamePlatformNotMatching).Send();
				}
			}

			var player = QSBPlayerManager.GetPlayer(From);
			player.Name = PlayerName;
			DebugLog.ToAll($"{player.Name} joined!", MessageType.Info);
			DebugLog.DebugWrite($"{player.Name} joined. id:{player.PlayerId}, qsbVersion:{QSBVersion}, gameVersion:{GameVersion}, platform:{Platform}. dlcInstalled:{DlcInstalled}", MessageType.Info);
		}

		public override void OnReceiveLocal()
		{
			var player = QSBPlayerManager.GetPlayer(QSBPlayerManager.LocalPlayerId);
			player.Name = PlayerName;
			DebugLog.ToAll($"Connected to server as {player.Name}.", MessageType.Info);

			if (QSBSceneManager.IsInUniverse)
			{
				player.IsReady = true;
				new PlayerReadyMessage(true).Send();
			}
		}
	}
}
