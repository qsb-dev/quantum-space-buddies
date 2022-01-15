using Mirror;
using OWML.Common;
using QSB.ClientServerStateSync;
using QSB.ClientServerStateSync.Messages;
using QSB.Player;
using QSB.Player.Messages;
using QSB.Player.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
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

		internal static readonly Type[] _types;
		private static readonly Dictionary<Type, ushort> _typeToId = new();

		static QSBMessageManager()
		{
			_types = typeof(QSBMessage).GetDerivedTypes().ToArray();
			for (ushort i = 0; i < _types.Length; i++)
			{
				_typeToId.Add(_types[i], i);
				// call static constructor of message if needed
				RuntimeHelpers.RunClassConstructor(_types[i].TypeHandle);
			}
		}

		public static void Init()
		{
			NetworkServer.RegisterHandler<Wrapper>((_, wrapper) => OnServerReceive(wrapper));
			NetworkClient.RegisterHandler<Wrapper>(wrapper => OnClientReceive(wrapper.Msg));
		}

		private static void OnServerReceive(Wrapper wrapper)
		{
			var msg = wrapper.Msg;
			if (msg.To == uint.MaxValue)
			{
				NetworkServer.SendToAll(wrapper);
			}
			else if (msg.To == 0)
			{
				NetworkServer.localConnection.Send(wrapper);
			}
			else
			{
				var conn = NetworkServer.connections.Values.FirstOrDefault(x => msg.To == x.GetPlayerId());
				if (conn == null)
				{
					DebugLog.ToConsole($"SendTo unknown player! id: {msg.To}, message: {msg}", MessageType.Error);
					return;
				}

				conn.Send(wrapper);
			}
		}

		private static void OnClientReceive(QSBMessage msg)
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

		public static void Send<M>(this M msg)
			where M : QSBMessage
		{
			if (PlayerTransformSync.LocalInstance == null)
			{
				DebugLog.ToConsole($"Warning - Tried to send message {msg} before local player was established.", MessageType.Warning);
				return;
			}

			msg.From = QSBPlayerManager.LocalPlayerId;
			NetworkClient.Send(new Wrapper
			{
				Id = _typeToId[msg.GetType()],
				Msg = msg
			});
		}

		public static void SendMessage<T, M>(this T worldObject, M msg)
			where T : IWorldObject
			where M : QSBWorldObjectMessage<T>
		{
			msg.ObjectId = worldObject.ObjectId;
			Send(msg);
		}
	}

	internal struct Wrapper : NetworkMessage
	{
		internal ushort Id;
		internal QSBMessage Msg;
	}

	internal static class ReaderWriterExtensions
	{
		private static Wrapper ReadWrapper(this NetworkReader reader)
		{
			var wrapper = new Wrapper();
			wrapper.Id = reader.ReadUShort();
			wrapper.Msg = (QSBMessage)FormatterServices.GetUninitializedObject(QSBMessageManager._types[wrapper.Id]);
			wrapper.Msg.Deserialize(reader);
			return wrapper;
		}

		private static void WriteWrapper(this NetworkWriter writer, Wrapper wrapper)
		{
			writer.Write(wrapper.Id);
			wrapper.Msg.Serialize(writer);
		}
	}
}
