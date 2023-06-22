using QSB.Utility;
using UnityEngine;

namespace QSB.Syncs.Unsectored.Transforms;

public abstract class UnsectoredTransformSync : BaseUnsectoredSync
{
	protected abstract Transform InitLocalTransform();
	protected abstract Transform InitRemoteTransform();

	protected sealed override Transform InitAttachedTransform()
		=> isOwned ? InitLocalTransform() : InitRemoteTransform();

	protected override void GetFromAttached()
	{
		transform.position = ReferenceTransform.ToRelPos(AttachedTransform.position);
		transform.rotation = ReferenceTransform.ToRelRot(AttachedTransform.rotation);
	}

	protected override void ApplyToAttached()
	{
		AttachedTransform.position = ReferenceTransform.FromRelPos(UseInterpolation ? SmoothPosition : transform.position);
		AttachedTransform.rotation = ReferenceTransform.FromRelRot(UseInterpolation ? SmoothRotation : transform.rotation);
	}
}