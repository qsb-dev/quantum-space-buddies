using Mirror;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Animation.Player.Messages
{
	public class ChangeAnimTypeMessage : QSBEnumMessage<AnimationType>
	{
		private uint PlayerId;

		public ChangeAnimTypeMessage(uint playerId, AnimationType type)
		{
			PlayerId = playerId;
			Value = type;
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(PlayerId);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			PlayerId = reader.Read<uint>();
		}

		public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(PlayerId);
			if (!player.IsReady)
			{
				return;
			}

			player.AnimationSync.SetAnimationType(Value);
			player.InstrumentsManager.CheckInstrumentProps(Value);
		}
	}
}