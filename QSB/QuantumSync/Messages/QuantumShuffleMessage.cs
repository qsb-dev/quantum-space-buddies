using Mirror;
using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;

namespace QSB.QuantumSync.Messages
{
	internal class QuantumShuffleMessage : QSBWorldObjectMessage<QSBQuantumShuffleObject>
	{
		private int[] IndexArray;

		public QuantumShuffleMessage(int[] indexArray) => IndexArray = indexArray;

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.WriteArray(IndexArray);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			IndexArray = reader.ReadArray<int>();
		}

		public override void OnReceiveRemote() => WorldObject.ShuffleObjects(IndexArray);
	}
}
