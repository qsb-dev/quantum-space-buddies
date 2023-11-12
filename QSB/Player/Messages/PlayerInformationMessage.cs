using Mirror;
using OWML.Common;
using QSB.ClientServerStateSync;
using QSB.HUD;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.Player.Messages;

public class PlayerInformationMessage : QSBMessage
{
	private string PlayerName;
	private bool IsReady;
	private bool FlashlightActive;
	private bool SuitedUp;
	private bool HelmetOn;
	private bool LocalProbeLauncherEquipped;
	private bool SignalscopeEquipped;
	private bool TranslatorEquipped;
	private bool ProbeActive;
	private ClientState ClientState;
	private float FieldOfView;
	private bool IsInShip;
	private HUDIcon HUDIcon;
	private string SkinType;
	private string JetpackType;

	public PlayerInformationMessage()
	{
		var player = QSBPlayerManager.LocalPlayer;
		PlayerName = player.Name;
		IsReady = player.IsReady;
		FlashlightActive = player.FlashlightActive;
		SuitedUp = player.SuitedUp;
		HelmetOn = Locator.GetPlayerSuit() != null && Locator.GetPlayerSuit().IsWearingHelmet();
		LocalProbeLauncherEquipped = player.LocalProbeLauncherEquipped;
		SignalscopeEquipped = player.SignalscopeEquipped;
		TranslatorEquipped = player.TranslatorEquipped;
		ProbeActive = player.ProbeActive;
		ClientState = player.State;
		FieldOfView = PlayerData.GetGraphicSettings().fieldOfView;
		IsInShip = player.IsInShip;
		HUDIcon = player.HUDBox == null ? HUDIcon.UNKNOWN : player.HUDBox.PlanetIcon;
		SkinType = QSBCore.SkinVariation;
		JetpackType = QSBCore.JetpackVariation;
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(PlayerName);
		writer.Write(IsReady);
		writer.Write(FlashlightActive);
		writer.Write(SuitedUp);
		writer.Write(HelmetOn);
		writer.Write(LocalProbeLauncherEquipped);
		writer.Write(SignalscopeEquipped);
		writer.Write(TranslatorEquipped);
		writer.Write(ProbeActive);
		writer.Write(ClientState);
		writer.Write(FieldOfView);
		writer.Write(IsInShip);
		writer.Write(HUDIcon);
		writer.Write(SkinType);
		writer.Write(JetpackType);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		PlayerName = reader.ReadString();
		IsReady = reader.Read<bool>();
		FlashlightActive = reader.Read<bool>();
		SuitedUp = reader.Read<bool>();
		HelmetOn = reader.Read<bool>();
		LocalProbeLauncherEquipped = reader.Read<bool>();
		SignalscopeEquipped = reader.Read<bool>();
		TranslatorEquipped = reader.Read<bool>();
		ProbeActive = reader.Read<bool>();
		ClientState = reader.Read<ClientState>();
		FieldOfView = reader.ReadFloat();
		IsInShip = reader.ReadBool();
		HUDIcon = reader.Read<HUDIcon>();
		SkinType = reader.ReadString();
		JetpackType = reader.ReadString();
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

			Delay.RunWhen(() => player.IsReady && QSBPlayerManager.LocalPlayer.IsReady, () =>
			{
				player.UpdateObjectsFromStates();
				player.HelmetAnimator.SetHelmetInstant(HelmetOn);

				var REMOTE_Traveller_HEA_Player_v2 = player.Body.transform.Find("REMOTE_Traveller_HEA_Player_v2");
				BodyCustomization.BodyCustomizer.Instance.CustomizeRemoteBody(REMOTE_Traveller_HEA_Player_v2.gameObject, player.HelmetAnimator.FakeHead.gameObject, SkinType, JetpackType);

				player.Camera.fieldOfView = FieldOfView;
			});

			player.State = ClientState;

			Delay.RunWhen(() => player.HUDBox != null, () =>
			{
				player.HUDBox.PlayerName.text = PlayerName.ToUpper();
				player.HUDBox.UpdateIcon(HUDIcon);
			});
		}
		else
		{
			DebugLog.ToConsole($"Warning - got player information message about player that doesnt exist!", MessageType.Warning);
		}
	}
}