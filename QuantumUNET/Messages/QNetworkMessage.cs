using QuantumUNET.Transport;
using System;

namespace QuantumUNET.Messages
{
	public class QNetworkMessage
	{
		public short MsgType;
		public QNetworkConnection Connection;
		public QNetworkReader Reader;
		public int ChannelId;

		public TMsg ReadMessage<TMsg>() where TMsg : QMessageBase, new()
		{
			var result = Activator.CreateInstance<TMsg>();
			result.Deserialize(Reader);
			return result;
		}

		public void ReadMessage<TMsg>(TMsg msg) where TMsg : QMessageBase =>
			msg.Deserialize(Reader);
	}
}