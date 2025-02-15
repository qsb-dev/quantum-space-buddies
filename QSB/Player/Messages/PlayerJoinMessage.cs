using HarmonyLib;
using Mirror;
using OWML.Common;
using QSB.HUD;
using QSB.Localization;
using QSB.Messaging;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.Player.Messages;

public class PlayerJoinMessage : QSBMessage
{
	private string PlayerName;
	private string QSBVersion;
	private string GameVersion;
	private bool DlcInstalled;
	private bool NewHorizonsInstalled;

	private int[] AddonHashes;

	public PlayerJoinMessage(string name)
	{
		PlayerName = name;
		QSBVersion = QSBCore.QSBVersion;
		GameVersion = QSBCore.GameVersion;
		DlcInstalled = QSBCore.DLCInstalled;
		NewHorizonsInstalled = QSBCore.QSBNHAssembly != null;

		AddonHashes = QSBCore.Addons.Keys
			.Except(QSBCore.CosmeticAddons)
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
		writer.Write(NewHorizonsInstalled);

		writer.Write(AddonHashes);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		PlayerName = reader.ReadString();
		QSBVersion = reader.ReadString();
		GameVersion = reader.ReadString();
		DlcInstalled = reader.Read<bool>();
		NewHorizonsInstalled = reader.Read<bool>();

		AddonHashes = reader.Read<int[]>();
	}

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			if (QSBCore.DebugSettings.KickEveryone)
			{
				DebugLog.ToConsole($"Kicking {PlayerName} because of DebugSettings.KickEveryone", MessageType.Error);
				new PlayerKickMessage(From, "This server has DebugSettings.KickEveryone enabled.").Send();
				return;
			}

			if (QSBVersion != QSBCore.QSBVersion)
			{
				DebugLog.ToConsole($"Error - Client {PlayerName} connecting with wrong QSB version. (Client:{QSBVersion}, Server:{QSBCore.QSBVersion})", MessageType.Error);
				new PlayerKickMessage(From, string.Format(QSBLocalization.Current.QSBVersionMismatch, QSBVersion, QSBCore.QSBVersion)).Send();
				return;
			}

			if (GameVersion != QSBCore.GameVersion)
			{
				DebugLog.ToConsole($"Error - Client {PlayerName} connecting with wrong game version. (Client:{GameVersion}, Server:{QSBCore.GameVersion})", MessageType.Error);
				new PlayerKickMessage(From, string.Format(QSBLocalization.Current.OWVersionMismatch, GameVersion, QSBCore.GameVersion)).Send();
				return;
			}

			if (DlcInstalled != QSBCore.DLCInstalled)
			{
				DebugLog.ToConsole($"Error - Client {PlayerName} connecting with wrong DLC installation state. (Client:{DlcInstalled}, Server:{QSBCore.DLCInstalled})", MessageType.Error);
				new PlayerKickMessage(From, string.Format(QSBLocalization.Current.DLCMismatch, DlcInstalled, QSBCore.DLCInstalled)).Send();
				return;
			}

			if (QSBPlayerManager.PlayerList.Any(x => x.EyeState > EyeState.Observatory))
			{
				DebugLog.ToConsole($"Error - Client {PlayerName} connecting too late into eye scene.", MessageType.Error);
				new PlayerKickMessage(From, QSBLocalization.Current.GameProgressLimit).Send();
				return;
			}

			var addonHashes = QSBCore.Addons.Keys
				.Except(QSBCore.CosmeticAddons)
				.Select(x => x.GetStableHashCode())
				.ToArray();
			if (!AddonHashes.SequenceEqual(addonHashes))
			{
				DebugLog.ToConsole($"Error - Client {PlayerName} connecting with addon mismatch. (Client:{AddonHashes.Join()}, Server:{addonHashes.Join()})", MessageType.Error);
				new PlayerKickMessage(From, string.Format(QSBLocalization.Current.AddonMismatch, AddonHashes.Length, addonHashes.Length)).Send();
				return;
			}

			var nhInstalled = QSBCore.QSBNHAssembly != null;
			if (NewHorizonsInstalled != nhInstalled)
			{
				DebugLog.ToConsole($"Error - Client {PlayerName} connecting with NH mismatch. (Client:{NewHorizonsInstalled}, Server:{nhInstalled})", MessageType.Error);
				new PlayerKickMessage(From, string.Format(QSBLocalization.Current.ModMismatch, "New Horizons", NewHorizonsInstalled, nhInstalled)).Send();
				return;
			}
		}

		var player = QSBPlayerManager.GetPlayer(From);
		player.Name = PlayerName;
		MultiplayerHUDManager.Instance.WriteSystemMessage(string.Format(QSBLocalization.Current.PlayerJoinedTheGame, player.Name), Color.green);
		DebugLog.DebugWrite($"{player} joined. qsbVersion:{QSBVersion}, gameVersion:{GameVersion}, dlcInstalled:{DlcInstalled}", MessageType.Info);
	}

	public override void OnReceiveLocal()
	{
		var player = QSBPlayerManager.GetPlayer(QSBPlayerManager.LocalPlayerId);
		player.Name = PlayerName;
	}
}
