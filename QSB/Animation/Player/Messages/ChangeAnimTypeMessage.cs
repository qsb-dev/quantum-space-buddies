using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Animation.Player.Messages
{
	public class ChangeAnimTypeMessage : QSBMessage<AnimationType>
	{
		public ChangeAnimTypeMessage(AnimationType type) => Value = type;

		public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			if (!player.IsReady)
			{
				return;
			}

			player.AnimationSync.SetAnimationType(Value);
			player.InstrumentsManager.CheckInstrumentProps(Value);
		}
	}
}