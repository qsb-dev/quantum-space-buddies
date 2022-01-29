using Mirror;
using QSB.Utility;
using UnityEngine;

namespace QSB.Syncs.Unsectored.Transforms
{
	public abstract class UnsectoredTransformSync : BaseUnsectoredSync
	{
		protected abstract Transform InitLocalTransform();
		protected abstract Transform InitRemoteTransform();

		protected sealed override Transform InitAttachedTransform()
			=> hasAuthority ? InitLocalTransform() : InitRemoteTransform();

		protected override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(transform.position);
			writer.Write(transform.rotation);
		}

		protected override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			transform.position = reader.ReadVector3();
			transform.rotation = reader.ReadQuaternion();
		}

		protected override void GetFromAttached()
		{
			transform.position = ReferenceTransform.ToRelPos(AttachedTransform.position);
			transform.rotation = ReferenceTransform.ToRelRot(AttachedTransform.rotation);
		}

		protected override void ApplyToAttached()
		{
			if (IsPlayerObject)
			{
				if (UseInterpolation)
				{
					AttachedTransform.localPosition = SmoothPosition;
					AttachedTransform.localRotation = SmoothRotation;
				}
				else
				{
					AttachedTransform.localPosition = transform.position;
					AttachedTransform.localRotation = transform.rotation;
				}
			}
			else
			{
				if (UseInterpolation)
				{
					AttachedTransform.position = ReferenceTransform.FromRelPos(SmoothPosition);
					AttachedTransform.rotation = ReferenceTransform.FromRelRot(SmoothRotation);
				}
				else
				{
					AttachedTransform.position = ReferenceTransform.FromRelPos(transform.position);
					AttachedTransform.rotation = ReferenceTransform.FromRelRot(transform.rotation);
				}
			}
		}
	}
}
