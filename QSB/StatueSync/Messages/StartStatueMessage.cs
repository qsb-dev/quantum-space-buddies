using Mirror;
using QSB.ClientServerStateSync;
using QSB.ClientServerStateSync.Messages;
using QSB.Messaging;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.StatueSync.Messages;

public class StartStatueMessage : QSBMessage
{
	private Vector3 PlayerPosition;
	private Quaternion PlayerRotation;
	private float CameraDegrees;

	public StartStatueMessage(Vector3 position, Quaternion rotation, float degrees)
	{
		PlayerPosition = position;
		PlayerRotation = rotation;
		CameraDegrees = degrees;
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(PlayerPosition);
		writer.Write(PlayerRotation);
		writer.Write(CameraDegrees);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		PlayerPosition = reader.ReadVector3();
		PlayerRotation = reader.ReadQuaternion();
		CameraDegrees = reader.Read<float>();
	}

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveLocal()
	{
		if (QSBCore.IsHost)
		{
			new ServerStateMessage(ServerState.InStatueCutscene).Send();
		}
	}

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			new ServerStateMessage(ServerState.InStatueCutscene).Send();
		}

		StatueManager.Instance.BeginSequence(PlayerPosition, PlayerRotation, CameraDegrees);
	}
}