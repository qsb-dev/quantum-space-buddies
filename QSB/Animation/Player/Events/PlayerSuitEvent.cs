using QSB.Events;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Animation.Player.Events
{
	public class PlayerSuitEvent : QSBEvent<ToggleMessage>
	{
		public override bool RequireWorldObjectsReady() => true;

		public override void SetupListener()
		{
			GlobalMessenger.AddListener(EventNames.SuitUp, HandleSuitUp);
			GlobalMessenger.AddListener(EventNames.RemoveSuit, HandleSuitDown);
		}

		public override void CloseListener()
		{
			GlobalMessenger.RemoveListener(EventNames.SuitUp, HandleSuitUp);
			GlobalMessenger.RemoveListener(EventNames.RemoveSuit, HandleSuitDown);
		}

		private void HandleSuitUp() => SendEvent(CreateMessage(true));
		private void HandleSuitDown() => SendEvent(CreateMessage(false));

		private ToggleMessage CreateMessage(bool value) => new()
		{
			AboutId = LocalPlayerId,
			ToggleValue = value
		};

		public override void OnReceiveRemote(bool server, ToggleMessage message)
		{
			var player = QSBPlayerManager.GetPlayer(message.AboutId);
			player.SuitedUp = message.ToggleValue;

			if (!player.IsReady)
			{
				return;
			}

			var animator = player.AnimationSync;
			var type = message.ToggleValue ? AnimationType.PlayerSuited : AnimationType.PlayerUnsuited;
			animator.SetAnimationType(type);
		}

		public override void OnReceiveLocal(bool server, ToggleMessage message)
		{
			QSBPlayerManager.LocalPlayer.SuitedUp = message.ToggleValue;
			var animator = QSBPlayerManager.LocalPlayer.AnimationSync;
			var type = message.ToggleValue ? AnimationType.PlayerSuited : AnimationType.PlayerUnsuited;
			animator.CurrentType = type;
		}
	}
}