using Mirror;
using OWML.Common;
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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace QSB.Messaging
{
	public static class QSBMessageManager
	{
		#region inner workings

		private static readonly Type[] _types;

		static QSBMessageManager()
		{
			_types = typeof(QSBMessageRaw).GetDerivedTypes()
				.Concat(typeof(QSBMessage).GetDerivedTypes())
				.ToArray();
			foreach (var type in _types)
			{
				// call static constructor of message if needed
				RuntimeHelpers.RunClassConstructor(type.TypeHandle);
			}
		}

		public static void Init()
		{
			DebugLog.DebugWrite("REGISTERING MESSAGES");

			var NetworkServer_RegisterHandlerSafe = typeof(NetworkServer).GetMethod(nameof(NetworkServer.RegisterHandlerSafe));
			var NetworkClient_RegisterHandlerSafe = typeof(NetworkClient).GetMethod(nameof(NetworkClient.RegisterHandlerSafe));
			var OnServerReceiveRaw = typeof(QSBMessageManager).GetMethod(nameof(QSBMessageManager.OnServerReceiveRaw));
			var OnClientReceiveRaw = typeof(QSBMessageManager).GetMethod(nameof(QSBMessageManager.OnClientReceiveRaw));
			var OnServerReceive = typeof(QSBMessageManager).GetMethod(nameof(QSBMessageManager.OnServerReceive));
			var OnClientReceive = typeof(QSBMessageManager).GetMethod(nameof(QSBMessageManager.OnClientReceive));

			foreach (var type in _types)
			{
				MethodInfo OnServerReceive2;
				MethodInfo OnClientReceive2;

				if (typeof(QSBMessageRaw).IsAssignableFrom(type))
				{
					OnServerReceive2 = OnServerReceiveRaw;
					OnClientReceive2 = OnClientReceiveRaw;
				}
				else
				{
					OnServerReceive2 = OnServerReceive;
					OnClientReceive2 = OnClientReceive;
				}

				var serverHandler = OnServerReceive2.MakeGenericMethod(type).CreateDelegate(typeof(Action<>));
				var clientHandler = OnClientReceive2.MakeGenericMethod(type).CreateDelegate(typeof(Action<>));
				DebugLog.DebugWrite($"server handler = {serverHandler}");
				DebugLog.DebugWrite($"client handler = {clientHandler}");
				NetworkServer_RegisterHandlerSafe.MakeGenericMethod(type).Invoke(null, new object[] { serverHandler });
				NetworkClient_RegisterHandlerSafe.MakeGenericMethod(type).Invoke(null, new object[] { clientHandler });
			}
		}

		private static void OnServerReceiveRaw<T>(T msg)
			where T : QSBMessageRaw
		{
			NetworkServer.SendToAll(msg);
		}

		private static void OnClientReceiveRaw<T>(T msg)
			where T : QSBMessageRaw
		{
			msg.OnReceive();
		}

		private static void OnServerReceive<T>(T msg)
			where T : QSBMessage
		{
			if (msg.To == uint.MaxValue)
			{
				NetworkServer.SendToAll(msg);
			}
			else if (msg.To == 0)
			{
				NetworkServer.localConnection.Send(msg);
			}
			else
			{
				var conn = NetworkServer.connections.Values.FirstOrDefault(x => msg.To == x.identity.netId);
				if (conn == null)
				{
					DebugLog.ToConsole($"SendTo unknown player! id: {msg.To}, message: {msg}", MessageType.Error);
					return;
				}

				conn.Send(msg);
			}
		}

		private static void OnClientReceive<T>(T msg)
			where T : QSBMessage
		{
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
			NetworkClient.Send(msg);
		}

		public static void ServerSendRaw<M>(this M msg, NetworkConnectionToClient conn)
			where M : QSBMessageRaw
		{
			conn.Send(msg);
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
			NetworkClient.Send(msg);
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
	public abstract class QSBMessageRaw : NetworkMessage
	{
		public abstract void OnReceive();
		public override string ToString() => GetType().Name;
	}
}
