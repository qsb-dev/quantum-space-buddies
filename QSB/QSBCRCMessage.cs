using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace QSB
{
	class QSBCRCMessage : MessageBase
	{
		public override void Deserialize(NetworkReader reader)
		{
			int num = (int)reader.ReadUInt16();
			this.scripts = new QSBCRCMessageEntry[num];
			for (int i = 0; i < this.scripts.Length; i++)
			{
				QSBCRCMessageEntry crcmessageEntry = default(QSBCRCMessageEntry);
				crcmessageEntry.name = reader.ReadString();
				crcmessageEntry.channel = reader.ReadByte();
				this.scripts[i] = crcmessageEntry;
			}
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write((ushort)this.scripts.Length);
			for (int i = 0; i < this.scripts.Length; i++)
			{
				writer.Write(this.scripts[i].name);
				writer.Write(this.scripts[i].channel);
			}
		}

		public QSBCRCMessageEntry[] scripts;
	}
}
