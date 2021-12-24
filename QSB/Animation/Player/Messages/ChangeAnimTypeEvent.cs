using QSB.Events;
using QSB.Instruments;
using QSB.Messaging;
using QSB.Player;

namespace QSB.Animation.Player.Messages
{
	public class ChangeAnimTypeEvent : QSBEvent<EnumMessage<AnimationType>>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<uint, AnimationType>.AddListener(EventNames.QSBChangeAnimType, Handler);
		public override void CloseListener() => GlobalMessenger<uint, AnimationType>.RemoveListener(EventNames.QSBChangeAnimType, Handler);

		private void Handler(uint player, AnimationType type) => SendEvent(CreateMessage(player, type));

		private EnumMessage<AnimationType> CreateMessage(uint player, AnimationType type) => new()
		{
			AboutId = player,
			EnumValue = type
		};

		public override void OnReceiveRemote(bool server, EnumMessage<AnimationType> message)
		{
			if (!QSBPlayerManager.GetPlayer(message.AboutId).IsReady)
			{
				return;
			}

			QSBPlayerManager.GetPlayer(message.AboutId).AnimationSync.SetAnimationType(message.EnumValue);
			QSBPlayerManager.GetSyncObject<InstrumentsManager>(message.AboutId).CheckInstrumentProps(message.EnumValue);
		}
	}
}