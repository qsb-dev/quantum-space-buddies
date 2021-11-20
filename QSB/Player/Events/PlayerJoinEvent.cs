using OWML.Common;
using QSB.Events;
using QSB.Utility;

namespace QSB.Player.Events
{
	public class PlayerJoinEvent : QSBEvent<PlayerJoinMessage>
	{
		public override EventType Type => EventType.PlayerJoin;

		public override void SetupListener() => GlobalMessenger<string>.AddListener(EventNames.QSBPlayerJoin, Handler);
		public override void CloseListener() => GlobalMessenger<string>.RemoveListener(EventNames.QSBPlayerJoin, Handler);

		private void Handler(string name) => SendEvent(CreateMessage(name));

		private PlayerJoinMessage CreateMessage(string name) => new PlayerJoinMessage
		{
			AboutId = LocalPlayerId,
			PlayerName = name,
			QSBVersion = QSBCore.QSBVersion,
			GameVersion = QSBCore.GameVersion,
			Platform = QSBCore.Platform
		};

		public override void OnReceiveRemote(bool server, PlayerJoinMessage message)
		{
			if (message.QSBVersion != QSBCore.QSBVersion)
			{
				if (server)
				{
					DebugLog.ToConsole($"Error - Client {message.PlayerName} connecting with wrong QSB version. (Client:{message.QSBVersion}, Server:{QSBCore.QSBVersion})", MessageType.Error);
					QSBEventManager.FireEvent(EventNames.QSBPlayerKick, message.AboutId, KickReason.QSBVersionNotMatching);
				}

				return;
			}

			if (message.GameVersion != QSBCore.GameVersion)
			{
				if (server)
				{
					DebugLog.ToConsole($"Error - Client {message.PlayerName} connecting with wrong game version. (Client:{message.GameVersion}, Server:{QSBCore.GameVersion})", MessageType.Error);
					QSBEventManager.FireEvent(EventNames.QSBPlayerKick, message.AboutId, KickReason.GameVersionNotMatching);
				}

				return;
			}

			if (message.Platform != QSBCore.Platform)
			{
				if (server)
				{
					DebugLog.ToConsole($"Error - Client {message.PlayerName} connecting with wrong game platform. (Client:{message.Platform}, Server:{QSBCore.Platform})", MessageType.Error);
					QSBEventManager.FireEvent(EventNames.QSBPlayerKick, message.AboutId, KickReason.GamePlatformNotMatching);
				}
			}

			var player = QSBPlayerManager.GetPlayer(message.AboutId);
			player.Name = message.PlayerName;
			DebugLog.ToAll($"{player.Name} joined!", MessageType.Info);
			DebugLog.DebugWrite($"{player.Name} joined. id:{player.PlayerId}, qsbVersion:{message.QSBVersion}, gameVersion:{message.GameVersion}, platform:{message.Platform}", MessageType.Info);
		}

		public override void OnReceiveLocal(bool server, PlayerJoinMessage message)
		{
			var player = QSBPlayerManager.GetPlayer(QSBPlayerManager.LocalPlayerId);
			player.Name = message.PlayerName;
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