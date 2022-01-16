using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;

namespace QSB.Animation.Player.Messages
{
	public class PlayerSuitMessage : QSBBoolMessage
	{
		static PlayerSuitMessage()
		{
			GlobalMessenger.AddListener(OWEvents.SuitUp, () => Handle(true));
			GlobalMessenger.AddListener(OWEvents.RemoveSuit, () => Handle(false));
		}

		private static void Handle(bool on)
		{
			if (PlayerTransformSync.LocalInstance)
			{
				new PlayerSuitMessage(on).Send();
			}
		}

		private PlayerSuitMessage(bool on) => Value = on;

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			player.SuitedUp = Value;

			if (!player.IsReady)
			{
				return;
			}

			var animator = player.AnimationSync;
			var type = Value ? AnimationType.PlayerSuited : AnimationType.PlayerUnsuited;
			animator.SetAnimationType(type);
		}

		public override void OnReceiveLocal()
		{
			QSBPlayerManager.LocalPlayer.SuitedUp = Value;
			var animator = QSBPlayerManager.LocalPlayer.AnimationSync;
			var type = Value ? AnimationType.PlayerSuited : AnimationType.PlayerUnsuited;
			animator.CurrentType = type;
		}
	}
}