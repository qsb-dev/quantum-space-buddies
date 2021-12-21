using System;
using System.Collections.Generic;
using System.Linq;
using OWML.Common;
using QSB.ClientServerStateSync;
using QSB.ClientServerStateSync.Events;
using QSB.Events;
using QSB.Player;
using QSB.Player.Events;
using QSB.Player.TransformSync;
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

		private static readonly Dictionary<short, Type> _msgTypeToType = new();
		private static readonly Dictionary<Type, short> _typeToMsgType = new();

		static QSBMessageManager()
		{
			var types = typeof(QSBMessageRaw).GetDerivedTypes().ToArray();
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
			}
		}

		public static void Init()
		{
			foreach (var msgType in _msgTypeToType.Keys)
			{
				QNetworkServer.RegisterHandlerSafe(msgType, OnServerReceive);
				QNetworkManager.singleton.client.RegisterHandlerSafe(msgType, OnClientReceive);
			}
		}

		private static void OnServerReceive(QNetworkMessage netMsg)
		{
			var msgType = netMsg.MsgType;
			var msg = (QSBMessageRaw)Activator.CreateInstance(_msgTypeToType[msgType]);
			netMsg.ReadMessage(msg);

			if (msg is QSBMessage m)
			{
				if (m.To == uint.MaxValue)
				{
					QNetworkServer.SendToAll(msgType, m);
				}
				else if (m.To == 0)
				{
					OnReceive(m);
				}
				else
				{
					var conn = QNetworkServer.connections.FirstOrDefault(x => m.To == x.GetPlayerId());
					if (conn == null)
					{
						DebugLog.ToConsole($"SendTo unknown player! id: {m.To}, message: {m}", MessageType.Error);
						return;
					}
					conn.Send(msgType, m);
				}
			}
			else
			{
				QNetworkServer.SendToAll(msgType, msg);
			}
		}

		private static void OnClientReceive(QNetworkMessage netMsg)
		{
			var msgType = netMsg.MsgType;
			var msg = (QSBMessageRaw)Activator.CreateInstance(_msgTypeToType[msgType]);
			netMsg.ReadMessage(msg);

			OnReceive(msg);
		}

		private static void OnReceive(QSBMessageRaw msg)
		{
			if (PlayerTransformSync.LocalInstance == null)
			{
				DebugLog.ToConsole($"Warning - Tried to handle message {msg} before local player was established.", MessageType.Warning);
				return;
			}

			if (msg is QSBMessage m)
			{
				if (QSBPlayerManager.PlayerExists(m.From))
				{
					var player = QSBPlayerManager.GetPlayer(m.From);

					if (!player.IsReady
					    && player.PlayerId != QSBPlayerManager.LocalPlayerId
					    && player.State is ClientState.AliveInSolarSystem or ClientState.AliveInEye or ClientState.DeadInSolarSystem
					    && m is not QSBEventRelay { Event: PlayerInformationEvent or PlayerReadyEvent or RequestStateResyncEvent or ServerStateEvent })
					{
						DebugLog.ToConsole($"Warning - Got message {m} from player {m.From}, but they were not ready. Asking for state resync, just in case.", MessageType.Warning);
						QSBEventManager.FireEvent(EventNames.QSBRequestStateResync);
					}
				}
			}

			try
			{
				msg.OnReceive();
			}
			catch (Exception ex)
			{
				DebugLog.ToConsole($"Error - Exception handling message {msg} : {ex}", MessageType.Error);
			}
		}

		#endregion


		public static void Send<M>(this M msg)
			where M : QSBMessageRaw, new()
		{
			if (PlayerTransformSync.LocalInstance == null)
			{
				DebugLog.ToConsole($"Warning - Tried to send message {msg} before local player was established.", MessageType.Warning);
				return;
			}

			if (msg is QSBMessage m)
			{
				m.From = QSBPlayerManager.LocalPlayerId;
			}

			var msgType = _typeToMsgType[typeof(M)];
			QNetworkManager.singleton.client.Send(msgType, msg);
		}

		public static void SendMessage<T, M>(this T worldObject, M msg)
			where T : IWorldObject
			where M : QSBWorldObjectMessage<T>, new()
		{
			msg.ObjectId = worldObject.ObjectId;
			Send(msg);
		}
	}

	/// <summary>
	/// message that will be sent to every client
	/// </summary>
	public abstract class QSBMessageRaw : QMessageBase
	{
		public abstract void OnReceive();
		public override string ToString() => GetType().Name;
	}
}
