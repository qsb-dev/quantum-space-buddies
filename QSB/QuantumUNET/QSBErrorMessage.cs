using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	public class QSBErrorMessage : QSBMessageBase
	{
		public override void Deserialize(QSBNetworkReader reader)
		{
			this.errorCode = (int)reader.ReadUInt16();
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			writer.Write((ushort)this.errorCode);
		}

		public int errorCode;
	}
}
