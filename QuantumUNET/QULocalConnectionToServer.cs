using QuantumUNET.Messages;
using QuantumUNET.Transport;
using UnityEngine;

namespace QuantumUNET
{
	internal class QULocalConnectionToServer : QNetworkConnection
	{
		public QULocalConnectionToServer(QNetworkServer localServer)
		{
			address = "localServer";
			m_LocalServer = localServer;
		}

		public override bool Send(short msgType, QMessageBase msg)
			=> m_LocalServer.InvokeHandlerOnServer(this, msgType, msg, 0);

		public override bool SendUnreliable(short msgType, QMessageBase msg)
			=> m_LocalServer.InvokeHandlerOnServer(this, msgType, msg, 1);

		public override bool SendByChannel(short msgType, QMessageBase msg, int channelId)
			=> m_LocalServer.InvokeHandlerOnServer(this, msgType, msg, channelId);

		public override bool SendBytes(byte[] bytes, int numBytes, int channelId)
		{
			bool result;
			if (numBytes <= 0)
			{
				Debug.LogError("LocalConnection:SendBytes cannot send zero bytes");
				result = false;
			}
			else
			{
				result = m_LocalServer.InvokeBytes(this, bytes, numBytes, channelId);
			}

			return result;
		}

		public override bool SendWriter(QNetworkWriter writer, int channelId)
			=> m_LocalServer.InvokeBytes(this, writer.AsArray(), (short)writer.AsArray().Length, channelId);

		private readonly QNetworkServer m_LocalServer;
	}
}