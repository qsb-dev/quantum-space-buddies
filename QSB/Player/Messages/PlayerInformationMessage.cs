using Mirror;
using OWML.Common;
using QSB.ClientServerStateSync;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.Player.Messages;

public class PlayerInformationMessage : QSBMessage
{
	private string PlayerName;
	private bool IsReady;
	private bool FlashlightActive;
	private bool SuitedUp;
	private bool LocalProbeLauncherEquipped;
	private bool SignalscopeEquipped;
	private bool TranslatorEquipped;
	private bool ProbeActive;
	private ClientState ClientState;
	private float FieldOfView;
	private bool IsInShip;

	public PlayerInformationMessage()
	{
		var player = QSBPlayerManager.LocalPlayer;
		PlayerName = player.Name;
		IsReady = player.IsReady;
		FlashlightActive = player.FlashlightActive;
		SuitedUp = player.SuitedUp;
		LocalProbeLauncherEquipped = player.LocalProbeLauncherEquipped;
		SignalscopeEquipped = player.SignalscopeEquipped;
		TranslatorEquipped = player.TranslatorEquipped;
		ProbeActive = player.ProbeActive;
		ClientState = player.State;
		FieldOfView = PlayerData.GetGraphicSettings().fieldOfView;
		IsInShip = player.IsInShip;
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(PlayerName);
		writer.Write(IsReady);
		writer.Write(FlashlightActive);
		writer.Write(SuitedUp);
		writer.Write(LocalProbeLauncherEquipped);
		writer.Write(SignalscopeEquipped);
		writer.Write(TranslatorEquipped);
		writer.Write(ProbeActive);
		writer.Write(ClientState);
		writer.Write(FieldOfView);
		writer.Write(IsInShip);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		PlayerName = reader.ReadString();
		IsReady = reader.Read<bool>();
		FlashlightActive = reader.Read<bool>();
		SuitedUp = reader.Read<bool>();
		LocalProbeLauncherEquipped = reader.Read<bool>();
		SignalscopeEquipped = reader.Read<bool>();
		TranslatorEquipped = reader.Read<bool>();
		ProbeActive = reader.Read<bool>();
		ClientState = reader.Read<ClientState>();
		FieldOfView = reader.ReadFloat();
		IsInShip = reader.ReadBool();
	}

	public override void OnReceiveRemote()
	{
		RequestStateResyncMessage._waitingForEvent = false;
		if (QSBPlayerManager.PlayerExists(From))
		{
			var player = QSBPlayerManager.GetPlayer(From);
			player.Name = PlayerName;
			player.IsReady = IsReady;
			player.FlashlightActive = FlashlightActive;
			player.SuitedUp = SuitedUp;
			player.LocalProbeLauncherEquipped = LocalProbeLauncherEquipped;
			player.SignalscopeEquipped = SignalscopeEquipped;
			player.TranslatorEquipped = TranslatorEquipped;
			player.ProbeActive = ProbeActive;
			player.IsInShip = IsInShip;
			if (QSBPlayerManager.LocalPlayer.IsReady && player.IsReady)
			{
				player.UpdateObjectsFromStates();
			}

			Delay.RunWhen(
				() => player.Camera != null,
				() => player.Camera.fieldOfView = FieldOfView);

			player.State = ClientState;
		}
		else
		{
			DebugLog.ToConsole($"Warning - got player information message about player that doesnt exist!", MessageType.Warning);
		}
	}
}