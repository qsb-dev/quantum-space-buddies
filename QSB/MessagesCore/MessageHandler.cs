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
	// Extend this to create new message handlers.
	public class MessageHandler<T> where T : QSBMessageBase, new()
	{
		public event Action<T> OnClientReceiveMessage;

		public event Action<T> OnServerReceiveMessage;

		private readonly EventType _eventType;

		public MessageHandler(EventType eventType)
		{
			_eventType = eventType + MsgType.Highest + 1;
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
			var eventName = Enum.GetName(typeof(EventType), _eventType - 1 - MsgType.Highest).ToUpper();
			if (QSBNetworkServer.handlers.Keys.Contains((short)_eventType))
			{
				DebugLog.ToConsole($"Warning - NetworkServer already contains a handler for EventType {_eventType}", MessageType.Warning);
				QSBNetworkServer.handlers.Remove((short)_eventType);
			}
			QSBNetworkServer.RegisterHandler((short)_eventType, OnServerReceiveMessageHandler);
			QuantumUNET.Components.QSBNetworkManagerUNET.singleton.client.RegisterHandler((short)_eventType, OnClientReceiveMessageHandler);
		}

		public void SendToAll(T message)
		{
			if (!QSBNetworkManager.Instance.IsReady)
			{
				return;
			}
			QSBNetworkServer.SendToAll((short)_eventType, message);
		}

		public void SendToServer(T message)
		{
			if (!QSBNetworkManager.Instance.IsReady)
			{
				return;
			}
			QuantumUNET.Components.QSBNetworkManagerUNET.singleton.client.Send((short)_eventType, message);
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