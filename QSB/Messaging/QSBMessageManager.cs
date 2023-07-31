using Mirror;
using OWML.Common;
using QSB.ClientServerStateSync;
using QSB.ClientServerStateSync.Messages;
using QSB.Patches;
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

namespace QSB.Messaging;

public static class QSBMessageManager
{
	#region inner workings

	internal static readonly Type[] _types;
	internal static readonly Dictionary<Type, ushort> _typeToId = new();

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
		NetworkClient.RegisterHandler<Wrapper>(wrapper => OnClientReceive(wrapper));
	}

	private static void OnServerReceive(QSBMessage msg)
	{
		if (msg.To == uint.MaxValue)
		{
			NetworkServer.SendToAll<Wrapper>(msg);
		}
		else if (msg.To == 0)
		{
			NetworkServer.localConnection.Send<Wrapper>(msg);
		}
		else
		{
			var connection = msg.To.GetNetworkConnection();

			if (connection == default)
			{
				DebugLog.ToConsole($"Warning - Tried to handle message from disconnected(?) player.", MessageType.Warning);
				return;
			}

			connection.Send<Wrapper>(msg);
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
				//DebugLog.ToConsole($"Warning - Got message {msg} from player {msg.From}, but they were not ready. Asking for state resync, just in case.", MessageType.Warning);
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
				QSBPatch.Remote = true;
				msg.OnReceiveRemote();
				QSBPatch.Remote = false;
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
		NetworkClient.Send<Wrapper>(msg);
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
	public QSBMessage Msg;

	public static implicit operator QSBMessage(Wrapper wrapper) => wrapper.Msg;
	public static implicit operator Wrapper(QSBMessage msg) => new() { Msg = msg };
}

public static class ReaderWriterExtensions
{
	private static QSBMessage ReadQSBMessage(this NetworkReader reader)
	{
		var id = reader.ReadUShort();
		var type = QSBMessageManager._types[id];
		var msg = (QSBMessage)FormatterServices.GetUninitializedObject(type);
		msg.Deserialize(reader);
		return msg;
	}

	private static void WriteQSBMessage(this NetworkWriter writer, QSBMessage msg)
	{
		var type = msg.GetType();
		var id = QSBMessageManager._typeToId[type];
		writer.Write(id);
		msg.Serialize(writer);
	}
}