using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	public class QSBNetworkMessage
	{
		public static string Dump(byte[] payload, int sz)
		{
			var text = "[";
			for (var i = 0; i < sz; i++)
			{
				text = text + payload[i] + " ";
			}
			return text + "]";
		}

		public TMsg ReadMessage<TMsg>() where TMsg : MessageBase, new()
		{
			var result = Activator.CreateInstance<TMsg>();
			result.Deserialize(reader);
			return result;
		}

		public void ReadMessage<TMsg>(TMsg msg) where TMsg : MessageBase
		{
			msg.Deserialize(reader);
		}

		public const int MaxMessageSize = 65535;

		public short msgType;

		public QSBNetworkConnection conn;

		public NetworkReader reader;

		public int channelId;
	}
}
