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
			=> m_LocalServer.InvokeHandlerOnServer(this, msgType, msg);

		public override bool SendByChannel(short msgType, QMessageBase msg)
			=> m_LocalServer.InvokeHandlerOnServer(this, msgType, msg);

		public override bool SendBytes(byte[] bytes, int numBytes)
		{
			bool result;
			if (numBytes <= 0)
			{
				Debug.LogError("LocalConnection:SendBytes cannot send zero bytes");
				result = false;
			}
			else
			{
				result = m_LocalServer.InvokeBytes(this, bytes, numBytes);
			}

			return result;
		}

		public override bool SendWriter(QNetworkWriter writer)
			=> m_LocalServer.InvokeBytes(this, writer.AsArray(), (short)writer.AsArray().Length);

		private readonly QNetworkServer m_LocalServer;
	}
}