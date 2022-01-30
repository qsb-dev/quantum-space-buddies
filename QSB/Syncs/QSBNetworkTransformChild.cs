using Mirror;
using QSB.Utility;
using UnityEngine;

namespace QSB.Syncs
{
	public class QSBNetworkTransformChild : QSBNetworkBehaviour
	{
		public Transform Target;

		protected override float SendInterval => 0.05f;

		protected Vector3 _prevPosition;
		protected Quaternion _prevRotation;

		protected override void UpdatePrevData()
		{
			_prevPosition = Target.localPosition;
			_prevRotation = Target.localRotation;
		}

		protected override bool HasChanged() =>
			Vector3.Distance(Target.localPosition, _prevPosition) > 1E-05f ||
			Quaternion.Angle(Target.localRotation, _prevRotation) > 1E-05f;

		protected override void Serialize(NetworkWriter writer)
		{
			writer.Write(Target.localPosition);
			writer.Write(Target.localRotation);
		}

		protected override void Deserialize(NetworkReader reader)
		{
			Target.localPosition = reader.ReadVector3();
			Target.localRotation = reader.ReadQuaternion();
		}
	}
}
