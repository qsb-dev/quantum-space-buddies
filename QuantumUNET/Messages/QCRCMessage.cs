using QuantumUNET.Transport;

namespace QuantumUNET.Messages
{
	internal class QCRCMessage : QMessageBase
	{
		public QCRCMessageEntry[] scripts;

		public override void Deserialize(QNetworkReader reader)
		{
			var num = (int)reader.ReadUInt16();
			scripts = new QCRCMessageEntry[num];
			for (var i = 0; i < scripts.Length; i++)
			{
				var crcmessageEntry = default(QCRCMessageEntry);
				crcmessageEntry.name = reader.ReadString();
				crcmessageEntry.channel = reader.ReadByte();
				scripts[i] = crcmessageEntry;
			}
		}

		public override void Serialize(QNetworkWriter writer)
		{
			writer.Write((ushort)scripts.Length);
			for (var i = 0; i < scripts.Length; i++)
			{
				writer.Write(scripts[i].name);
				writer.Write(scripts[i].channel);
			}
		}
	}
}