using Mirror;
using QSB.Utility;
using UnityEngine;

namespace QSB.Syncs;

public class QSBNetworkTransform : QSBNetworkBehaviour
{
	protected override float SendInterval => 0.05f;

	private const float PositionChangeThreshold = 0.05f;
	private const float RotationChangeThreshold = 0.05f;

	private Vector3 _prevPosition;
	private Quaternion _prevRotation;

	protected override bool HasChanged() =>
		Vector3.Distance(transform.position, _prevPosition) > PositionChangeThreshold ||
		Quaternion.Angle(transform.rotation, _prevRotation) > RotationChangeThreshold;

	protected override void Serialize(NetworkWriter writer)
	{
		writer.Write(transform.position);
		writer.Write(transform.rotation);
	}

	protected override void UpdatePrevData()
	{
		_prevPosition = transform.position;
		_prevRotation = transform.rotation;
	}

	protected override void Deserialize(NetworkReader reader)
	{
		transform.position = reader.ReadVector3();
		transform.rotation = reader.ReadQuaternion();
	}
}