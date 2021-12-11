using OWML.Common;
using QSB.Events;
using QSB.Messaging;
using QSB.Utility;
using QuantumUNET.Transport;

namespace QSB.Player.Events
{
	public class PlayerJoinMessage : QSBMessage
	{
		private string _playerName;
		private string _qsbVersion;
		private string _gameVersion;
		private GamePlatform _platform;

		public PlayerJoinMessage(string name)
		{
			_playerName = name;
			_qsbVersion = QSBCore.QSBVersion;
			_gameVersion = QSBCore.GameVersion;
			_platform = QSBCore.Platform;
		}

		public PlayerJoinMessage() { }

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(_playerName);
			writer.Write(_qsbVersion);
			writer.Write(_gameVersion);
			writer.Write((int)_platform);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			_playerName = reader.ReadString();
			_qsbVersion = reader.ReadString();
			_gameVersion = reader.ReadString();
			_platform = (GamePlatform)reader.ReadInt32();
		}

		public override void OnReceiveRemote(uint from)
		{
			if (_qsbVersion != QSBCore.QSBVersion)
			{
				if (QSBCore.IsHost)
				{
					DebugLog.ToConsole($"Error - Client {_playerName} connecting with wrong QSB version. (Client:{_qsbVersion}, Server:{QSBCore.QSBVersion})", MessageType.Error);
					QSBEventManager.FireEvent(EventNames.QSBPlayerKick, from, KickReason.QSBVersionNotMatching);
				}

				return;
			}

			if (_gameVersion != QSBCore.GameVersion)
			{
				if (QSBCore.IsHost)
				{
					DebugLog.ToConsole($"Error - Client {_playerName} connecting with wrong game version. (Client:{_gameVersion}, Server:{QSBCore.GameVersion})", MessageType.Error);
					QSBEventManager.FireEvent(EventNames.QSBPlayerKick, from, KickReason.GameVersionNotMatching);
				}

				return;
			}

			if (_platform != QSBCore.Platform)
			{
				if (QSBCore.IsHost)
				{
					DebugLog.ToConsole($"Error - Client {_playerName} connecting with wrong game platform. (Client:{_platform}, Server:{QSBCore.Platform})", MessageType.Error);
					QSBEventManager.FireEvent(EventNames.QSBPlayerKick, from, KickReason.GamePlatformNotMatching);
				}
			}

			var player = QSBPlayerManager.GetPlayer(from);
			player.Name = _playerName;
			DebugLog.ToAll($"{player.Name} joined!", MessageType.Info);
			DebugLog.DebugWrite($"{player.Name} joined. id:{player.PlayerId}, qsbVersion:{_qsbVersion}, gameVersion:{_gameVersion}, platform:{_platform}", MessageType.Info);
		}

		public override void OnReceiveLocal()
		{
			var player = QSBPlayerManager.GetPlayer(QSBPlayerManager.LocalPlayerId);
			player.Name = _playerName;
			var text = $"Connected to server as {player.Name}.";
			DebugLog.ToAll(text, MessageType.Info);

			if (QSBSceneManager.IsInUniverse)
			{
				QSBPlayerManager.LocalPlayer.IsReady = true;
				QSBEventManager.FireEvent(EventNames.QSBPlayerReady, true);
			}
		}
	}
}
