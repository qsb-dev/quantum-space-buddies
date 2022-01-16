﻿using OWML.Common;
using QSB.ClientServerStateSync;
using QSB.ClientServerStateSync.Messages;
using QSB.Player;
using QSB.Player.Messages;
using QSB.Player.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using QuantumUNET.Components;
using QuantumUNET.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace QSB.Messaging
{
	public static class QSBMessageManager
	{
		#region inner workings

		private static readonly Dictionary<short, Type> _msgTypeToType = new();
		private static readonly Dictionary<Type, short> _typeToMsgType = new();

		static QSBMessageManager()
		{
			var types = typeof(QSBMessageRaw).GetDerivedTypes()
				.Concat(typeof(QSBMessage).GetDerivedTypes())
				.ToArray();
			for (var i = 0; i < types.Length; i++)
			{
				var msgType = (short)(QMsgType.Highest + 1 + i);
				if (msgType >= short.MaxValue)
				{
					DebugLog.ToConsole("Hey, uh, maybe don't create 32,767 events? You really should never be seeing this." +
						"If you are, something has either gone terrible wrong or QSB somehow needs more events that classes in Outer Wilds." +
						"In either case, I guess something has gone terribly wrong...", MessageType.Error);
				}

				_msgTypeToType.Add(msgType, types[i]);
				_typeToMsgType.Add(types[i], msgType);

				// call static constructor of message if needed
				RuntimeHelpers.RunClassConstructor(types[i].TypeHandle);
			}
		}

		public static void Init()
		{
			foreach (var (msgType, type) in _msgTypeToType)
			{
				if (typeof(QSBMessageRaw).IsAssignableFrom(type))
				{
					QNetworkServer.RegisterHandlerSafe(msgType, OnServerReceiveRaw);
					QNetworkManager.singleton.client.RegisterHandlerSafe(msgType, OnClientReceiveRaw);
				}
				else
				{
					QNetworkServer.RegisterHandlerSafe(msgType, OnServerReceive);
					QNetworkManager.singleton.client.RegisterHandlerSafe(msgType, OnClientReceive);
				}
			}
		}

		private static void OnServerReceiveRaw(QNetworkMessage netMsg)
		{
			var msgType = netMsg.MsgType;
			var msg = (QSBMessageRaw)FormatterServices.GetUninitializedObject(_msgTypeToType[msgType]);
			netMsg.ReadMessage(msg);

			QNetworkServer.SendToAll(msgType, msg);
		}

		private static void OnClientReceiveRaw(QNetworkMessage netMsg)
		{
			var msgType = netMsg.MsgType;
			var msg = (QSBMessageRaw)FormatterServices.GetUninitializedObject(_msgTypeToType[msgType]);
			netMsg.ReadMessage(msg);

			msg.OnReceive();
		}

		private static void OnServerReceive(QNetworkMessage netMsg)
		{
			var msgType = netMsg.MsgType;
			var msg = (QSBMessage)FormatterServices.GetUninitializedObject(_msgTypeToType[msgType]);
			netMsg.ReadMessage(msg);

			if (msg.To == uint.MaxValue)
			{
				QNetworkServer.SendToAll(msgType, msg);
			}
			else if (msg.To == 0)
			{
				QNetworkServer.localConnection.Send(msgType, msg);
			}
			else
			{
				var conn = QNetworkServer.connections.FirstOrDefault(x => msg.To == x.GetPlayerId());
				if (conn == null)
				{
					DebugLog.ToConsole($"SendTo unknown player! id: {msg.To}, message: {msg}", MessageType.Error);
					return;
				}

				conn.Send(msgType, msg);
			}
		}

		private static void OnClientReceive(QNetworkMessage netMsg)
		{
			var msgType = netMsg.MsgType;
			var msg = (QSBMessage)FormatterServices.GetUninitializedObject(_msgTypeToType[msgType]);
			netMsg.ReadMessage(msg);

			if (PlayerTransformSync.LocalInstance == null)
			{
				DebugLog.ToConsole($"Warning - Tried to handle message {msg} before local player was established.", MessageType.Warning);
				return;
			}

			if (QSBPlayerManager.PlayerExists(msg.From))
			{
				var player = QSBPlayerManager.GetPlayer(msg.From);

				if (!player.IsReady
					&& player.PlayerId != QSBPlayerManager.LocalPlayerId
					&& player.State is ClientState.AliveInSolarSystem or ClientState.AliveInEye or ClientState.DeadInSolarSystem
					&& msg is not (PlayerInformationMessage or PlayerReadyMessage or RequestStateResyncMessage or ServerStateMessage))
				{
					DebugLog.ToConsole($"Warning - Got message {msg} from player {msg.From}, but they were not ready. Asking for state resync, just in case.", MessageType.Warning);
					new RequestStateResyncMessage().Send();
				}
			}

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
				DebugLog.ToConsole($"Error - Exception handling message {msg} : {ex}", MessageType.Error);
			}
		}

		#endregion

		public static void SendRaw<M>(this M msg)
			where M : QSBMessageRaw
		{
			var msgType = _typeToMsgType[typeof(M)];
			QNetworkManager.singleton.client.Send(msgType, msg);
		}

		public static void ServerSendRaw<M>(this M msg, QNetworkConnection conn)
			where M : QSBMessageRaw
		{
			var msgType = _typeToMsgType[typeof(M)];
			conn.Send(msgType, msg);
		}

		public static void Send<M>(this M msg)
			where M : QSBMessage
		{
			if (PlayerTransformSync.LocalInstance == null)
			{
				DebugLog.ToConsole($"Warning - Tried to send message {msg} before local player was established.", MessageType.Warning);
				return;
			}

			msg.From = QSBPlayerManager.LocalPlayerId;
			var msgType = _typeToMsgType[typeof(M)];
			QNetworkManager.singleton.client.Send(msgType, msg);
		}

		public static void SendMessage<T, M>(this T worldObject, M msg)
			where T : IWorldObject
			where M : QSBWorldObjectMessage<T>
		{
			msg.ObjectId = worldObject.ObjectId;
			Send(msg);
		}
	}

	/// <summary>
	/// message that will be sent to every client. <br/>
	/// no checks are performed on the message. it is just sent and received.
	/// </summary>
	public abstract class QSBMessageRaw : QMessageBase
	{
		public abstract void OnReceive();
		public override string ToString() => GetType().Name;
	}
}
