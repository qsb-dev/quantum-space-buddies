using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;
using QuantumUNET.Transport;
using System;
using System.Linq;

namespace QSB.QuantumSync.Messages
{
	internal class QuantumShuffleMessage : QSBWorldObjectMessage<QSBQuantumShuffleObject>
	{
		private int[] IndexArray;

		public QuantumShuffleMessage(int[] indexArray) => IndexArray = indexArray;

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			var temp = IndexArray.Select(x => (byte)x).ToArray();
			writer.WriteBytesAndSize(temp, temp.Length);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			IndexArray = Array.ConvertAll(reader.ReadBytesAndSize(), Convert.ToInt32);
		}

		public override void OnReceiveRemote() => WorldObject.ShuffleObjects(IndexArray);
	}
}
