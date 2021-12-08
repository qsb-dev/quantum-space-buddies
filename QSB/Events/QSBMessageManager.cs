﻿using System;
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

namespace QSB.Events
{
	public static class QSBMessageManager
	{
		#region inner workings

		private const short msgType = short.MaxValue - 1;

		private class Msg : QMessageBase
		{
			public uint From;
			public uint To;
			public QSBMessage Message;

			public override void Serialize(QNetworkWriter writer)
			{
				writer.Write(From);
				writer.Write(To);
				writer.Write(_typeToId[Message.GetType()]);
				Message.Serialize(writer);
			}

			public override void Deserialize(QNetworkReader reader)
			{
				From = reader.ReadUInt32();
				To = reader.ReadUInt32();
				Message = (QSBMessage)Activator.CreateInstance(_idToType[reader.ReadInt32()]);
				Message.Deserialize(reader);
			}
		}

		private static readonly List<Type> _idToType = new();
		private static readonly Dictionary<Type, int> _typeToId = new();

		public static void Init()
		{
			var types = typeof(QSBMessage).Assembly.GetTypes()
				.Where(x => !x.IsAbstract && typeof(QSBMessage).IsAssignableFrom(x))
				.ToArray();
			for (var i = 0; i < types.Length; i++)
			{
				_idToType.Add(types[i]);
				_typeToId.Add(types[i], i);
			}

			QNetworkServer.RegisterHandler(msgType, netMsg =>
			{
				var msg = netMsg.ReadMessage<Msg>();
				if (msg.To != uint.MaxValue)
				{
					var conn = QNetworkServer.connections.FirstOrDefault(x => msg.To == x.GetPlayerId());
					if (conn == null)
					{
						DebugLog.ToConsole($"SendTo unknown player! id: {msg.To}, message: {msg.Message.GetType().Name}", MessageType.Error);
						return;
					}
					QNetworkServer.SendToClient(conn.connectionId, msgType, msg);
				}
				else
				{
					QNetworkServer.SendToAll(msgType, msg);
				}
			});

			QNetworkManager.singleton.client.RegisterHandler(msgType, netMsg =>
			{
				var msg = netMsg.ReadMessage<Msg>();
				if (msg.Message.ShouldReceive)
				{
					msg.Message.OnReceive(msg.From == QSBPlayerManager.LocalPlayerId);
				}
			});
		}

		#endregion


		public static void Send<TMessage>(this TMessage message, uint to = uint.MaxValue)
			where TMessage : QSBMessage, new()
		{
			QNetworkManager.singleton.client.Send(msgType, new Msg
			{
				From = QSBPlayerManager.LocalPlayerId,
				To = to,
				Message = message
			});
		}

		public static void SendMessage<TWorldObject, TMessage>(this TWorldObject worldObject, TMessage message, uint to = uint.MaxValue)
			where TMessage : QSBWorldObjectMessage<TWorldObject>, new()
			where TWorldObject : IWorldObject
		{
			message.Id = worldObject.ObjectId;
			Send(message, to);
		}
	}
}
