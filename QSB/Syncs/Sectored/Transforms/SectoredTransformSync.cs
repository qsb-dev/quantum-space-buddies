using QSB.Utility;
using UnityEngine;

namespace QSB.Syncs.Sectored.Transforms;

public abstract class SectoredTransformSync : BaseSectoredSync
{
	protected abstract Transform InitLocalTransform();
	protected abstract Transform InitRemoteTransform();

	protected sealed override Transform InitAttachedTransform()
		=> isOwned ? InitLocalTransform() : InitRemoteTransform();

	protected override void GetFromAttached()
	{
		GetFromSector();
		if (!ReferenceTransform)
		{
			return;
		}

		transform.position = ReferenceTransform.ToRelPos(AttachedTransform.position);
		transform.rotation = ReferenceTransform.ToRelRot(AttachedTransform.rotation);
	}

	protected override void ApplyToAttached()
	{
		ApplyToSector();
		if (!ReferenceTransform)
		{
			return;
		}

		AttachedTransform.position = ReferenceTransform.FromRelPos(UseInterpolation ? SmoothPosition : transform.position);
		AttachedTransform.rotation = ReferenceTransform.FromRelRot(UseInterpolation ? SmoothRotation : transform.rotation);
	}
}