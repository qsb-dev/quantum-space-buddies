using System;
using System.Collections.Generic;
using System.Linq;
using OWML.Common;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using QuantumUNET.Components;
using QuantumUNET.Messages;

namespace QSB.Messaging
{
	public static class QSBMessageManager
	{
		#region inner workings

		private static readonly Dictionary<short, QSBMessage> _msgTypeToMsg = new();
		private static readonly Dictionary<Type, short> _typeToMsgType = new();

		static QSBMessageManager()
		{
			var types = typeof(QSBMessage).GetDerivedTypes().ToArray();
			for (var i = 0; i < types.Length; i++)
			{
				var msgType = (short)(QMsgType.Highest + 1 + i);
				if (msgType >= short.MaxValue)
				{
					DebugLog.ToConsole("Hey, uh, maybe don't create 32,767 events? You really should never be seeing this." +
						"If you are, something has either gone terrible wrong or QSB somehow needs more events that classes in Outer Wilds." +
						"In either case, I guess something has gone terribly wrong...", MessageType.Error);
				}

				var type = types[i];
				var msg = (QSBMessage)Activator.CreateInstance(type);
				_msgTypeToMsg.Add(msgType, msg);
				_typeToMsgType.Add(type, msgType);
			}
		}

		public static void Init()
		{
			foreach (var msgType in _msgTypeToMsg.Keys)
			{
				QNetworkServer.RegisterHandlerSafe(msgType, OnServerReceive);
				QNetworkManager.singleton.client.RegisterHandlerSafe(msgType, OnClientReceive);
			}
		}

		private static void OnServerReceive(QNetworkMessage netMsg)
		{
			var msgType = netMsg.MsgType;
			var msg = _msgTypeToMsg[msgType];
			netMsg.ReadMessage(msg);

			if (msg.To == 0)
			{
				QNetworkServer.SendToClient(0, msgType, msg);
			}
			else if (msg.To == uint.MaxValue)
			{
				QNetworkServer.SendToAll(msgType, msg);
			}
			else
			{
				var conn = QNetworkServer.connections.FirstOrDefault(x => msg.To == x.GetPlayerId());
				if (conn == null)
				{
					DebugLog.ToConsole($"SendTo unknown player! id: {msg.To}, message: {msg.GetType().Name}", MessageType.Error);
					return;
				}
				conn.Send(msgType, msg);
			}
		}

		private static void OnClientReceive(QNetworkMessage netMsg)
		{
			var msgType = netMsg.MsgType;
			var msg = _msgTypeToMsg[msgType];
			netMsg.ReadMessage(msg);

			try
			{
				if (!msg.ShouldReceive)
				{
					return;
				}

				if (msg.From != QSBPlayerManager.LocalPlayerId)
				{
					msg.OnReceiveRemote();
				}
				else
				{
					msg.OnReceiveLocal();
				}
			}
			catch (Exception ex)
			{
				DebugLog.ToConsole($"Error - Exception handling message {msg.GetType().Name} : {ex}", MessageType.Error);
			}
		}

		#endregion


		public static void Send<M>(this M message)
			where M : QSBMessage, new()
		{
			var msgType = _typeToMsgType[typeof(M)];
			message.From = QSBPlayerManager.LocalPlayerId;
			QNetworkManager.singleton.client.Send(msgType, message);
		}

		public static void SendMessage<T, M>(this T worldObject, M message)
			where M : QSBWorldObjectMessage<T>, new()
			where T : IWorldObject
		{
			message.ObjectId = worldObject.ObjectId;
			Send(message);
		}
	}
}
