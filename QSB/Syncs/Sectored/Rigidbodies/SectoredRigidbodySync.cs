using Mirror;
using QSB.Utility;
using UnityEngine;

namespace QSB.Syncs.Sectored.Rigidbodies;

public abstract class SectoredRigidbodySync : BaseSectoredSync
{
	private const float VelocityChangeThreshold = 0.05f;
	private const float AngularVelocityChangeThreshold = 0.05f;

	protected Vector3 Velocity;
	protected Vector3 AngularVelocity;
	private Vector3 _prevVelocity;
	private Vector3 _prevAngularVelocity;

	public OWRigidbody AttachedRigidbody { get; private set; }
	public OWRigidbody ReferenceRigidbody { get; private set; }

	protected abstract OWRigidbody InitAttachedRigidbody();

	protected sealed override Transform InitAttachedTransform()
	{
		AttachedRigidbody = InitAttachedRigidbody();
		return AttachedRigidbody ? AttachedRigidbody.transform : null;
	}

	public sealed override void SetReferenceTransform(Transform referenceTransform)
	{
		if (ReferenceTransform == referenceTransform)
		{
			return;
		}

		base.SetReferenceTransform(referenceTransform);
		ReferenceRigidbody = ReferenceTransform ? ReferenceTransform.GetAttachedOWRigidbody() : null;
	}

	protected override bool CheckChanged() =>
		base.CheckChanged() ||
		Vector3.Distance(Velocity, _prevVelocity) > VelocityChangeThreshold ||
		Vector3.Distance(AngularVelocity, _prevAngularVelocity) > AngularVelocityChangeThreshold;

	protected override void Serialize(NetworkWriter writer)
	{
		writer.Write(Velocity);
		writer.Write(AngularVelocity);
		base.Serialize(writer);
	}

	protected override void UpdatePrevData()
	{
		base.UpdatePrevData();
		_prevVelocity = Velocity;
		_prevAngularVelocity = AngularVelocity;
	}

	protected override void Deserialize(NetworkReader reader)
	{
		Velocity = reader.ReadVector3();
		AngularVelocity = reader.ReadVector3();
		base.Deserialize(reader);
	}

	protected override void GetFromAttached()
	{
		GetFromSector();
		if (!ReferenceTransform)
		{
			return;
		}

		var pos = AttachedRigidbody.GetPosition();
		transform.position = ReferenceTransform.ToRelPos(pos);
		transform.rotation = ReferenceTransform.ToRelRot(AttachedRigidbody.GetRotation());
		Velocity = ReferenceRigidbody.ToRelVel(AttachedRigidbody.GetVelocity(), pos);
		AngularVelocity = ReferenceRigidbody.ToRelAngVel(AttachedRigidbody.GetAngularVelocity());
	}

	protected override void ApplyToAttached()
	{
		ApplyToSector();
		if (!ReferenceTransform)
		{
			return;
		}

		var targetPos = ReferenceTransform.FromRelPos(UseInterpolation ? SmoothPosition : transform.position);
		var targetRot = ReferenceTransform.FromRelRot(UseInterpolation ? SmoothRotation : transform.rotation);

		AttachedRigidbody.MoveToPosition(targetPos);
		AttachedRigidbody.MoveToRotation(targetRot);

		var targetVelocity = ReferenceRigidbody.FromRelVel(Velocity, targetPos);
		var targetAngularVelocity = ReferenceRigidbody.FromRelAngVel(AngularVelocity);

		AttachedRigidbody.SetVelocity(targetVelocity);
		AttachedRigidbody.SetAngularVelocity(targetAngularVelocity);
	}
}