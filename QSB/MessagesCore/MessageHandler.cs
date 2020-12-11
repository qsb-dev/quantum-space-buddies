using OWML.Common;
using QSB.EventsCore;
using QSB.Utility;
using QuantumUNET;
using QuantumUNET.Messages;
using System;
using System.Linq;
using UnityEngine.Networking;

namespace QSB.Messaging
{
	public class MessageHandler<T> where T : QSBMessageBase, new()
	{
		public event Action<T> OnClientReceiveMessage;

		public event Action<T> OnServerReceiveMessage;

		private readonly short _eventType;
		public EventType BaseEventType => (EventType)(_eventType - 1 - MsgType.Highest);

		public MessageHandler(EventType eventType)
		{
			_eventType = (short)(eventType + MsgType.Highest + 1);
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
			if (QSBNetworkServer.handlers.Keys.Contains(_eventType))
			{
				DebugLog.ToConsole($"Warning - NetworkServer already contains a handler for EventType {BaseEventType}", MessageType.Warning);
				QSBNetworkServer.handlers.Remove(_eventType);
			}
			QSBNetworkServer.RegisterHandler(_eventType, OnServerReceiveMessageHandler);
			QuantumUNET.Components.QSBNetworkManagerUNET.singleton.client.RegisterHandler(_eventType, OnClientReceiveMessageHandler);
		}

		public void SendToAll(T message)
		{
			if (!QSBNetworkManager.Instance.IsReady)
			{
				return;
			}
			QSBNetworkServer.SendToAll(_eventType, message);
		}

		public void SendToServer(T message)
		{
			if (!QSBNetworkManager.Instance.IsReady)
			{
				return;
			}
			QuantumUNET.Components.QSBNetworkManagerUNET.singleton.client.Send(_eventType, message);
		}

		private void OnClientReceiveMessageHandler(QSBNetworkMessage netMsg)
		{
			var message = netMsg.ReadMessage<T>();
			OnClientReceiveMessage?.Invoke(message);
		}

		private void OnServerReceiveMessageHandler(QSBNetworkMessage netMsg)
		{
			var message = netMsg.ReadMessage<T>();
			OnServerReceiveMessage?.Invoke(message);
		}
	}
}