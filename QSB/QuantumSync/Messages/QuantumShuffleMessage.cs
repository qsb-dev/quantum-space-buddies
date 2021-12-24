using System;
using System.Linq;
using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.QuantumSync.Messages
{
	public class QuantumShuffleMessage : WorldObjectMessage
	{
		public int[] IndexArray { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			IndexArray = Array.ConvertAll(reader.ReadBytesAndSize(), Convert.ToInt32);
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			var temp = IndexArray.Select(x => (byte)x).ToArray();
			writer.WriteBytesAndSize(temp, temp.Length);
		}
	}
}
