using System;
using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	public class QSBNetworkMessage
	{
		public const int MaxMessageSize = 65535;
		public short MsgType;
		public QSBNetworkConnection Connection;
		public NetworkReader Reader;
		public int ChannelId;

		public TMsg ReadMessage<TMsg>() where TMsg : MessageBase, new()
		{
			var result = Activator.CreateInstance<TMsg>();
			result.Deserialize(Reader);
			return result;
		}

		public void ReadMessage<TMsg>(TMsg msg) where TMsg : MessageBase
		{
			msg.Deserialize(Reader);
		}
	}
}