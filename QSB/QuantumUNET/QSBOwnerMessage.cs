using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	class QSBOwnerMessage : MessageBase
	{
		public override void Deserialize(NetworkReader reader)
		{
			netId = reader.ReadNetworkId();
			playerControllerId = (short)reader.ReadPackedUInt32();
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(netId);
			writer.WritePackedUInt32((uint)playerControllerId);
		}

		public NetworkInstanceId netId;

		public short playerControllerId;
	}
}
