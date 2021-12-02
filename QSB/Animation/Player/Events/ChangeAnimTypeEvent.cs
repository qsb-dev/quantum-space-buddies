using QSB.Events;
using QSB.Instruments;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Animation.Player.Events
{
	public class ChangeAnimTypeEvent : QSBEvent<EnumMessage<AnimationType>>
	{
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
			if (!WorldObjectManager.AllReady || !QSBPlayerManager.GetPlayer(message.AboutId).IsReady)
			{
				return;
			}

			QSBPlayerManager.GetPlayer(message.AboutId).AnimationSync.SetAnimationType(message.EnumValue);
			QSBPlayerManager.GetSyncObject<InstrumentsManager>(message.AboutId).CheckInstrumentProps(message.EnumValue);
		}
	}
}