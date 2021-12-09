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
using QuantumUNET.Transport;

namespace QSB.Messaging
{
	public static class QSBMessageManager
	{
		#region inner workings

		private class Msg : QMessageBase
		{
			public uint From;
			public uint To;
			public QSBMessage Message;

			public override void Serialize(QNetworkWriter writer)
			{
				writer.Write(From);
				writer.Write(To);
				if (!_typeToIndex.TryGetValue(Message.GetType(), out var index))
				{
					DebugLog.ToConsole($"QSBMessageManager - unknown message type {Message.GetType()}", MessageType.Error);
				}
				writer.Write(index);
				Message.Serialize(writer);
			}

			public override void Deserialize(QNetworkReader reader)
			{
				From = reader.ReadUInt32();
				To = reader.ReadUInt32();
				Message = (QSBMessage)Activator.CreateInstance(_types[reader.ReadInt32()]);
				Message.Deserialize(reader);
			}
		}

		private const short msgType = short.MaxValue - 1;
		private static readonly Type[] _types = typeof(QSBMessage).GetDerivedTypes().ToArray();
		private static readonly Dictionary<Type, int> _typeToIndex = new();

		static QSBMessageManager()
		{
			for (var i = 0; i < _types.Length; i++)
			{
				_typeToIndex.Add(_types[i], i);
			}
		}

		public static void Init()
		{
			QNetworkServer.RegisterHandlerSafe(msgType, OnServerReceive);
			QNetworkManager.singleton.client.RegisterHandlerSafe(msgType, OnClientReceive);
		}

		private static void OnServerReceive(QNetworkMessage netMsg)
		{
			var msg = netMsg.ReadMessage<Msg>();
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
					DebugLog.ToConsole($"SendTo unknown player! id: {msg.To}, message: {msg.Message.GetType().Name}", MessageType.Error);
					return;
				}
				conn.Send(msgType, msg);
			}
		}

		private static void OnClientReceive(QNetworkMessage netMsg)
		{
			var msg = netMsg.ReadMessage<Msg>();
			if (!msg.Message.ShouldReceive)
			{
				return;
			}

			try
			{
				if (msg.From != QSBPlayerManager.LocalPlayerId)
				{
					msg.Message.OnReceiveRemote(msg.From);
				}
				else
				{
					msg.Message.OnReceiveLocal();
				}
			}
			catch (Exception ex)
			{
				DebugLog.ToConsole($"Error - Exception handling message {msg.Message.GetType().Name} : {ex}", MessageType.Error);
			}
		}

		#endregion


		public static void Send<M>(this M message, uint to = uint.MaxValue)
			where M : QSBMessage, new()
		{
			QNetworkManager.singleton.client.Send(msgType, new Msg
			{
				From = QSBPlayerManager.LocalPlayerId,
				To = to,
				Message = message
			});
		}

		public static void SendMessage<T, M>(this T worldObject, M message, uint to = uint.MaxValue)
			where M : QSBWorldObjectMessage<T>, new()
			where T : IWorldObject
		{
			message.Id = worldObject.ObjectId;
			Send(message, to);
		}
	}
}
