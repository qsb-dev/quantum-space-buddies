using Mirror;
using QSB.Utility;
using UnityEngine;

namespace QSB.Syncs;

public class QSBNetworkTransformChild : QSBNetworkBehaviour
{
	public Transform Target;

	protected override float SendInterval => 0.05f;

	private const float PositionChangeThreshold = 0.05f;
	private const float RotationChangeThreshold = 0.05f;

	private Vector3 _prevPosition;
	private Quaternion _prevRotation;

	protected override bool HasChanged() =>
		Vector3.Distance(Target.localPosition, _prevPosition) > PositionChangeThreshold ||
		Quaternion.Angle(Target.localRotation, _prevRotation) > RotationChangeThreshold;

	protected override void Serialize(NetworkWriter writer)
	{
		writer.Write(Target.localPosition);
		writer.Write(Target.localRotation);
	}

	protected override void UpdatePrevData()
	{
		_prevPosition = Target.localPosition;
		_prevRotation = Target.localRotation;
	}

	protected override void Deserialize(NetworkReader reader)
	{
		Target.localPosition = reader.ReadVector3();
		Target.localRotation = reader.ReadQuaternion();
	}
}