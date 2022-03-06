using Mirror;
using OWML.Common;
using QSB.Messaging;
using QSB.Utility;
using System.Linq;

namespace QSB.Player.Messages;

public class PlayerJoinMessage : QSBMessage
{
	private string PlayerName;
	private string QSBVersion;
	private string GameVersion;
	private bool DlcInstalled;

	public PlayerJoinMessage(string name)
	{
		PlayerName = name;
		QSBVersion = QSBCore.QSBVersion;
		GameVersion = QSBCore.GameVersion;
		DlcInstalled = QSBCore.DLCInstalled;
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(PlayerName);
		writer.Write(QSBVersion);
		writer.Write(GameVersion);
		writer.Write(DlcInstalled);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		PlayerName = reader.ReadString();
		QSBVersion = reader.ReadString();
		GameVersion = reader.ReadString();
		DlcInstalled = reader.Read<bool>();
	}

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			if (QSBVersion != QSBCore.QSBVersion)
			{
				DebugLog.ToConsole($"Error - Client {PlayerName} connecting with wrong QSB version. (Client:{QSBVersion}, Server:{QSBCore.QSBVersion})", MessageType.Error);
				new PlayerKickMessage(From, KickReason.QSBVersionNotMatching).Send();
				return;
			}

			if (GameVersion != QSBCore.GameVersion)
			{
				DebugLog.ToConsole($"Error - Client {PlayerName} connecting with wrong game version. (Client:{GameVersion}, Server:{QSBCore.GameVersion})", MessageType.Error);
				new PlayerKickMessage(From, KickReason.GameVersionNotMatching).Send();
				return;
			}

			if (DlcInstalled != QSBCore.DLCInstalled)
			{
				DebugLog.ToConsole($"Error - Client {PlayerName} connecting with wrong DLC installation state. (Client:{DlcInstalled}, Server:{QSBCore.DLCInstalled})", MessageType.Error);
				new PlayerKickMessage(From, KickReason.DLCNotMatching).Send();
				return;
			}

			if (QSBPlayerManager.PlayerList.Any(x => x.EyeState >= EyeState.Observatory))
			{
				DebugLog.ToConsole($"Error - Client {PlayerName} connecting too late into eye scene.", MessageType.Error);
				new PlayerKickMessage(From, KickReason.InEye).Send();
				return;
			}
		}

		var player = QSBPlayerManager.GetPlayer(From);
		player.Name = PlayerName;
		DebugLog.ToAll($"{player} joined!", MessageType.Info);
		DebugLog.DebugWrite($"{player} joined. qsbVersion:{QSBVersion}, gameVersion:{GameVersion}, dlcInstalled:{DlcInstalled}", MessageType.Info);
	}

	public override void OnReceiveLocal()
	{
		var player = QSBPlayerManager.GetPlayer(QSBPlayerManager.LocalPlayerId);
		player.Name = PlayerName;
		DebugLog.ToAll($"Connected to server as {player.Name}.", MessageType.Info);
	}
}