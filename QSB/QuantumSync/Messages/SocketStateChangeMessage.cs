using Mirror;
using OWML.Common;
using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;
using QSB.Utility;
using UnityEngine;

namespace QSB.QuantumSync.Messages;

public class SocketStateChangeMessage : QSBWorldObjectMessage<QSBSocketedQuantumObject>
{
	private int SocketId;
	private Quaternion LocalRotation;

	public SocketStateChangeMessage(int socketId, Quaternion localRotation)
	{
		SocketId = socketId;
		LocalRotation = localRotation;
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(SocketId);
		writer.Write(LocalRotation);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		SocketId = reader.Read<int>();
		LocalRotation = reader.ReadQuaternion();
	}

	public override void OnReceiveRemote()
	{
		if (WorldObject.ControllingPlayer != From)
		{
			DebugLog.ToConsole($"Error - Got SocketStateChangeEvent for {WorldObject.Name} from {From}, but it's currently controlled by {WorldObject.ControllingPlayer}!", MessageType.Error);
			return;
		}

		WorldObject.MoveToSocket(From, SocketId, LocalRotation);
	}
}