using QuantumUNET.Messages;
using QuantumUNET.Transport;

namespace QuantumUNET
{
	internal class QULocalConnectionToClient : QNetworkConnection
	{
		public QLocalClient LocalClient { get; }

		public QULocalConnectionToClient(QLocalClient localClient)
		{
			address = "localClient";
			LocalClient = localClient;
		}

		public override bool Send(short msgType, QMessageBase msg)
		{
			LocalClient.InvokeHandlerOnClient(msgType, msg, 0);
			return true;
		}

		public override bool SendUnreliable(short msgType, QMessageBase msg)
		{
			LocalClient.InvokeHandlerOnClient(msgType, msg, 1);
			return true;
		}

		public override bool SendByChannel(short msgType, QMessageBase msg, int channelId)
		{
			LocalClient.InvokeHandlerOnClient(msgType, msg, channelId);
			return true;
		}

		public override bool SendBytes(byte[] bytes, int numBytes, int channelId)
		{
			LocalClient.InvokeBytesOnClient(bytes, channelId);
			return true;
		}

		public override bool SendWriter(QNetworkWriter writer, int channelId)
		{
			LocalClient.InvokeBytesOnClient(writer.AsArray(), channelId);
			return true;
		}
	}
}