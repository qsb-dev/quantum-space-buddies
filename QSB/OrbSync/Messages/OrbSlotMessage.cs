using Mirror;
using QSB.Messaging;
using QSB.OrbSync.WorldObjects;

namespace QSB.OrbSync.Messages
{
	public class OrbSlotMessage : QSBWorldObjectMessage<QSBOrb, int>
	{
		private bool _playAudio;

		public OrbSlotMessage(int slotIndex, bool playAudio)
		{
			Data = slotIndex;
			_playAudio = playAudio;
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(_playAudio);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			_playAudio = reader.Read<bool>();
		}

		public override void OnReceiveRemote() => WorldObject.SetSlot(Data, _playAudio);
	}
}