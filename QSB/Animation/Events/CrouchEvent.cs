using QSB.Events;
using QSB.Messaging;
using QSB.Player;

namespace QSB.Animation.Events
{
	public class CrouchEvent : QSBEvent<FloatMessage>
	{
		public override EventType Type => EventType.AnimTrigger;

		public override void SetupListener() => GlobalMessenger<float>.AddListener(EventNames.QSBCrouch, Handler);
		public override void CloseListener() => GlobalMessenger<float>.RemoveListener(EventNames.QSBCrouch, Handler);

		private void Handler(float value) => SendEvent(CreateMessage(value));

		private FloatMessage CreateMessage(float value) => new FloatMessage
		{
			AboutId = LocalPlayerId,
			Value = value
		};

		public override void OnReceiveRemote(bool server, FloatMessage message)
		{
			if (!QSBCore.HasWokenUp)
			{
				return;
			}
			var animationSync = QSBPlayerManager.GetSyncObject<AnimationSync>(message.AboutId);
			animationSync?.HandleCrouch(message.Value);
		}
	}
}