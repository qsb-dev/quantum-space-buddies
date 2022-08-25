using HarmonyLib;
using Mirror;
using OWML.Common;
using QSB.Localization;
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
	// empty if no incompatible mods
	private string FirstIncompatibleMod;

	private int[] AddonHashes;

	public PlayerJoinMessage(string name)
	{
		PlayerName = name;
		QSBVersion = QSBCore.QSBVersion;
		GameVersion = QSBCore.GameVersion;
		DlcInstalled = QSBCore.DLCInstalled;

		var allEnabledMods = QSBCore.Helper.Interaction.GetMods();

		FirstIncompatibleMod = "";

		foreach (var mod in allEnabledMods)
		{
			if (QSBCore.IncompatibleMods.Contains(mod.ModHelper.Manifest.UniqueName))
			{
				FirstIncompatibleMod = mod.ModHelper.Manifest.UniqueName;
			}
		}

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
		writer.Write(FirstIncompatibleMod);

		writer.Write(AddonHashes);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		PlayerName = reader.ReadString();
		QSBVersion = reader.ReadString();
		GameVersion = reader.ReadString();
		DlcInstalled = reader.Read<bool>();
		FirstIncompatibleMod = reader.ReadString();

		AddonHashes = reader.Read<int[]>();
	}

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
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
				.Select(x => x.GetStableHashCode())
				.ToArray();
			if (!AddonHashes.SequenceEqual(addonHashes))
			{
				DebugLog.ToConsole($"Error - Client {PlayerName} connecting with addon mismatch. (Client:{AddonHashes.Join()}, Server:{addonHashes.Join()})", MessageType.Error);
				new PlayerKickMessage(From, string.Format(QSBLocalization.Current.AddonMismatch, AddonHashes.Length, addonHashes.Length)).Send();
				return;
			}

			if (FirstIncompatibleMod != "" && !QSBCore.IncompatibleModsAllowed)
			{
				DebugLog.ToConsole($"Error - Client {PlayerName} connecting with incompatible mod. (First mod found was {FirstIncompatibleMod})");
				new PlayerKickMessage(From, string.Format(QSBLocalization.Current.IncompatibleMod, FirstIncompatibleMod)).Send();
			}
		}

		var player = QSBPlayerManager.GetPlayer(From);
		player.Name = PlayerName;
		DebugLog.ToAll(string.Format(QSBLocalization.Current.PlayerJoinedTheGame, player.Name), MessageType.Info);
		DebugLog.DebugWrite($"{player} joined. qsbVersion:{QSBVersion}, gameVersion:{GameVersion}, dlcInstalled:{DlcInstalled}", MessageType.Info);
	}

	public override void OnReceiveLocal()
	{
		var player = QSBPlayerManager.GetPlayer(QSBPlayerManager.LocalPlayerId);
		player.Name = PlayerName;
	}
}
