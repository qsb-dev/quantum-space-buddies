using Mirror;
using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;
using System;
using System.Linq;

namespace QSB.QuantumSync.Messages
{
	internal class QuantumShuffleMessage : QSBWorldObjectMessage<QSBQuantumShuffleObject>
	{
		private int[] IndexArray;

		public QuantumShuffleMessage(int[] indexArray) => IndexArray = indexArray;

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			var temp = IndexArray.Select(x => (byte)x).ToArray();
			writer.WriteBytesAndSize(temp);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			IndexArray = Array.ConvertAll(reader.ReadBytesAndSize(), Convert.ToInt32);
		}

		public override void OnReceiveRemote() => WorldObject.ShuffleObjects(IndexArray);
	}
}
