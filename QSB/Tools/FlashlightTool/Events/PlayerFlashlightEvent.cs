using QSB.Events;
using QSB.Messaging;
using QSB.Player;

namespace QSB.Tools.FlashlightTool.Events
{
	public class PlayerFlashlightEvent : QSBEvent<ToggleMessage>
	{
		public override bool RequireWorldObjectsReady() => false;

		public override void SetupListener()
		{
			GlobalMessenger.AddListener(EventNames.TurnOnFlashlight, HandleTurnOn);
			GlobalMessenger.AddListener(EventNames.TurnOffFlashlight, HandleTurnOff);
		}

		public override void CloseListener()
		{
			GlobalMessenger.RemoveListener(EventNames.TurnOnFlashlight, HandleTurnOn);
			GlobalMessenger.RemoveListener(EventNames.TurnOffFlashlight, HandleTurnOff);
		}

		private void HandleTurnOn() => SendEvent(CreateMessage(true));
		private void HandleTurnOff() => SendEvent(CreateMessage(false));

		private ToggleMessage CreateMessage(bool value) => new()
		{
			AboutId = LocalPlayerId,
			ToggleValue = value
		};

		public override void OnReceiveRemote(bool server, ToggleMessage message)
		{
			var player = QSBPlayerManager.GetPlayer(message.AboutId);
			player.FlashlightActive = message.ToggleValue;
			player.FlashLight?.UpdateState(message.ToggleValue);
		}

		public override void OnReceiveLocal(bool server, ToggleMessage message) =>
			QSBPlayerManager.LocalPlayer.FlashlightActive = message.ToggleValue;
	}
}