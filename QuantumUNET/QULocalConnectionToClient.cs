using QuantumUNET.Messages;
using QuantumUNET.Transport;

namespace QuantumUNET
{
	internal class QULocalConnectionToClient : QNetworkConnection
	{
		public QULocalConnectionToClient(QLocalClient localClient)
		{
			address = "localClient";
			LocalClient = localClient;
		}

		public QLocalClient LocalClient { get; }

		public override bool Send(short msgType, QMessageBase msg)
		{
			LocalClient.InvokeHandlerOnClient(msgType, msg);
			return true;
		}

		public override bool SendByChannel(short msgType, QMessageBase msg)
		{
			LocalClient.InvokeHandlerOnClient(msgType, msg);
			return true;
		}

		public override bool SendBytes(byte[] bytes, int numBytes)
		{
			LocalClient.InvokeBytesOnClient(bytes);
			return true;
		}

		public override bool SendWriter(QNetworkWriter writer)
		{
			LocalClient.InvokeBytesOnClient(writer.AsArray());
			return true;
		}
	}
}