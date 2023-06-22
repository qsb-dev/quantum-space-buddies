using Mirror;
using QSB.Utility;
using UnityEngine;

namespace QSB.Syncs;

[UsedInUnityProject]
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

	public Transform AttachedTransform { get; internal set; }

	private const float SmoothTime = 0.1f;
	private Vector3 _positionSmoothVelocity;
	private Quaternion _rotationSmoothVelocity;

	protected override void Update()
	{
		if (AttachedTransform)
		{
			if (isOwned)
			{
				Target.localPosition = AttachedTransform.localPosition;
				Target.localRotation = AttachedTransform.localRotation;
			}
			else
			{
				AttachedTransform.localPosition = Vector3.SmoothDamp(AttachedTransform.localPosition, Target.localPosition, ref _positionSmoothVelocity, SmoothTime);
				AttachedTransform.localRotation = QuaternionHelper.SmoothDamp(AttachedTransform.localRotation, Target.localRotation, ref _rotationSmoothVelocity, SmoothTime);
			}
		}

		base.Update();
	}
}