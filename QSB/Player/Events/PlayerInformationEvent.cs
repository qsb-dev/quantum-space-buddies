using OWML.Common;
using QSB.Events;
using QSB.Utility;

namespace QSB.Player.Events
{
	public class PlayerInformationEvent : QSBEvent<PlayerInformationMessage>
	{
		public override bool RequireWorldObjectsReady => false;

		public override void SetupListener() => GlobalMessenger.AddListener(EventNames.QSBPlayerInformation, Handler);
		public override void CloseListener() => GlobalMessenger.RemoveListener(EventNames.QSBPlayerInformation, Handler);

		private void Handler() => SendEvent(CreateMessage(QSBPlayerManager.LocalPlayer));

		private PlayerInformationMessage CreateMessage(PlayerInfo player) => new()
		{
			AboutId = player.PlayerId,
			PlayerName = player.Name,
			IsReady = player.IsReady,
			FlashlightActive = player.FlashlightActive,
			SuitedUp = player.SuitedUp,
			ProbeLauncherEquipped = player.ProbeLauncherEquipped,
			SignalscopeEquipped = player.SignalscopeEquipped,
			TranslatorEquipped = player.TranslatorEquipped,
			ProbeActive = player.ProbeActive,
			ClientState = player.State
		};

		public override void OnReceiveRemote(bool server, PlayerInformationMessage message)
		{
			RequestStateResyncEvent._waitingForEvent = false;
			if (QSBPlayerManager.PlayerExists(message.AboutId))
			{
				QSBPlayerManager.HandleFullStateMessage(message);
			}
			else
			{
				DebugLog.ToConsole($"Warning - got player information message about player that doesnt exist!", MessageType.Warning);
			}
		}
	}
}
