using QSB.Instruments;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using QuantumUNET.Transport;

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

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(PlayerId);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			PlayerId = reader.ReadUInt32();
		}

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(PlayerId);
			if (!player.IsReady)
			{
				return;
			}

			player.AnimationSync.SetAnimationType(Value);
			QSBPlayerManager.GetSyncObject<InstrumentsManager>(PlayerId).CheckInstrumentProps(Value);
		}
	}
}