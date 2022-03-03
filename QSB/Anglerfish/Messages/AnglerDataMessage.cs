using Mirror;
using QSB.Anglerfish.WorldObjects;
using QSB.Messaging;
using QSB.Player;
using UnityEngine;

namespace QSB.Anglerfish.Messages;

/// <summary>
/// angler state, target transform, and local disturbance pos
/// </summary>
public class AnglerDataMessage : QSBWorldObjectMessage<QSBAngler, AnglerfishController.AnglerState>
{
	private uint TargetId;
	private Vector3 LocalDisturbancePos;

	public AnglerDataMessage(QSBAngler qsbAngler)
	{
		Data = qsbAngler.AttachedObject._currentState;
		TargetId = TargetToId(qsbAngler.TargetTransform);
		LocalDisturbancePos = qsbAngler.AttachedObject._localDisturbancePos;
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(TargetId);
		writer.Write(LocalDisturbancePos);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		TargetId = reader.Read<uint>();
		LocalDisturbancePos = reader.ReadVector3();
	}

	public override void OnReceiveRemote()
	{
		WorldObject.TargetTransform = IdToTarget(TargetId);
		WorldObject.AttachedObject._localDisturbancePos = LocalDisturbancePos;
		WorldObject.AttachedObject.ChangeState(Data);
	}

	private static uint TargetToId(Transform transform)
	{
		if (transform == null)
		{
			return uint.MaxValue;
		}

		if (transform == Locator.GetShipTransform())
		{
			return uint.MaxValue - 1;
		}

		return QSBPlayerManager.LocalPlayerId;
	}

	private static Transform IdToTarget(uint id)
	{
		if (id == uint.MaxValue)
		{
			return null;
		}

		if (id == uint.MaxValue - 1)
		{
			return Locator.GetShipTransform();
		}

		return QSBPlayerManager.GetPlayer(id).Body.transform;
	}
}