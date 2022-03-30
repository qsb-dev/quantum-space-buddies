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

	private int[] AddonHashes;

	public PlayerJoinMessage(string name)
	{
		PlayerName = name;
		QSBVersion = QSBCore.QSBVersion;
		GameVersion = QSBCore.GameVersion;
		DlcInstalled = QSBCore.DLCInstalled;

		AddonHashes = QSBCore.Addons.Keys
			.Select(x => x.GetStableHashCode())
			.ToArray();
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(PlayerName);
		writer.Write(QSBVersion);
		writer.Write(GameVersion);
		writer.Write(DlcInstalled);

		writer.Write(AddonHashes);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		PlayerName = reader.ReadString();
		QSBVersion = reader.ReadString();
		GameVersion = reader.ReadString();
		DlcInstalled = reader.Read<bool>();

		AddonHashes = reader.Read<int[]>();
	}

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			if (QSBVersion != QSBCore.QSBVersion)
			{
				DebugLog.ToConsole($"Error - Client {PlayerName} connecting with wrong QSB version. (Client:{QSBVersion}, Server:{QSBCore.QSBVersion})", MessageType.Error);
				new PlayerKickMessage(From, $"QSB version does not match. (Client:{QSBVersion}, Server:{QSBCore.QSBVersion})").Send();
				return;
			}

			if (GameVersion != QSBCore.GameVersion)
			{
				DebugLog.ToConsole($"Error - Client {PlayerName} connecting with wrong game version. (Client:{GameVersion}, Server:{QSBCore.GameVersion})", MessageType.Error);
				new PlayerKickMessage(From, $"Outer Wilds version does not match. (Client:{GameVersion}, Server:{QSBCore.GameVersion})").Send();
				return;
			}

			if (DlcInstalled != QSBCore.DLCInstalled)
			{
				DebugLog.ToConsole($"Error - Client {PlayerName} connecting with wrong DLC installation state. (Client:{DlcInstalled}, Server:{QSBCore.DLCInstalled})", MessageType.Error);
				new PlayerKickMessage(From, $"DLC installation state does not match. (Client:{DlcInstalled}, Server:{QSBCore.DLCInstalled})").Send();
				return;
			}

			if (QSBPlayerManager.PlayerList.Any(x => x.EyeState >= EyeState.Observatory))
			{
				DebugLog.ToConsole($"Error - Client {PlayerName} connecting too late into eye scene.", MessageType.Error);
				new PlayerKickMessage(From, "Game has progressed too far.").Send();
				return;
			}

			var addonHashes = QSBCore.Addons.Keys
				.Select(x => x.GetStableHashCode())
				.ToArray();
			if (!AddonHashes.SequenceEqual(addonHashes))
			{
				DebugLog.ToConsole($"Error - Client {PlayerName} connecting with addon mismatch. (Client:{AddonHashes}, Server:{addonHashes})", MessageType.Error);
				new PlayerKickMessage(From, $"Addon mismatch. (Client:{AddonHashes.Length} addons, Server:{addonHashes.Length} addons)").Send();
				return;
			}
		}

		var player = QSBPlayerManager.GetPlayer(From);
		player.Name = PlayerName;
		DebugLog.ToAll($"{player.Name} joined!", MessageType.Info);
		DebugLog.DebugWrite($"{player} joined. qsbVersion:{QSBVersion}, gameVersion:{GameVersion}, dlcInstalled:{DlcInstalled}", MessageType.Info);
	}

	public override void OnReceiveLocal()
	{
		var player = QSBPlayerManager.GetPlayer(QSBPlayerManager.LocalPlayerId);
		player.Name = PlayerName;
		DebugLog.ToAll($"Connected to server as {player.Name}.", MessageType.Info);
	}
}
