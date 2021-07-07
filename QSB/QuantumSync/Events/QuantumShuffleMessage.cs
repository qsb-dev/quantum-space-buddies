using QSB.WorldSync.Events;
using QuantumUNET.Transport;
using System;
using System.Linq;

namespace QSB.QuantumSync.Events
{
	public class QuantumShuffleMessage : WorldObjectMessage
	{
		public int[] IndexArray { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			IndexArray = Array.ConvertAll(reader.ReadByteArray(), Convert.ToInt32);
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			var temp = IndexArray.Select(x => (byte)x).ToArray();
			writer.WriteBytesAndSize(temp, temp.Length);
		}
	}
}
