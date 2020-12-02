using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace QSB
{
	class QSBObjectDestroyMessage : MessageBase
	{
		public override void Deserialize(NetworkReader reader)
		{
			netId = reader.ReadNetworkId();
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(netId);
		}

		public NetworkInstanceId netId;
	}
}
