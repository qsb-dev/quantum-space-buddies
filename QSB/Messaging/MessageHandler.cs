using QSB.Events;
using QSB.Utility;
using QuantumUNET;
using QuantumUNET.Components;
using QuantumUNET.Messages;
using System;
using System.Linq;

namespace QSB.Messaging
{
	public class MessageHandler<T> where T : QMessageBase, new()
	{
		public event Action<T> OnClientReceiveMessage;
		public event Action<T> OnServerReceiveMessage;

		private readonly short _eventType;

		public MessageHandler(EventType eventType)
		{
			_eventType = (short)(eventType + QMsgType.Highest + 1);
			if (QSBNetworkManager.Instance.IsReady)
			{
				Init();
			}
			else
			{
				QSBNetworkManager.Instance.OnNetworkManagerReady += Init;
			}
		}

		private void Init()
		{
			if (QNetworkServer.handlers.Keys.Contains(_eventType))
			{
				QNetworkServer.handlers.Remove(_eventType);
				QNetworkManager.singleton.client.handlers.Remove(_eventType);
			}

			QNetworkServer.RegisterHandler(_eventType, OnServerReceiveMessageHandler);
			QNetworkManager.singleton.client.RegisterHandler(_eventType, OnClientReceiveMessageHandler);
		}

		public void SendToAll(T message)
		{
			if (!QSBNetworkManager.Instance.IsReady)
			{
				return;
			}

			QNetworkServer.SendToAll(_eventType, message);
		}

		public void SendToServer(T message)
		{
			if (!QSBNetworkManager.Instance.IsReady)
			{
				return;
			}

			QNetworkManager.singleton.client.Send(_eventType, message);
		}

		private void OnClientReceiveMessageHandler(QNetworkMessage netMsg)
		{
			var message = netMsg.ReadMessage<T>();
			OnClientReceiveMessage?.SafeInvoke(message);
		}

		private void OnServerReceiveMessageHandler(QNetworkMessage netMsg)
		{
			var message = netMsg.ReadMessage<T>();
			OnServerReceiveMessage?.SafeInvoke(message);
		}
	}
}