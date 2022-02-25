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

	public override void SetReferenceTransform(Transform referenceTransform)
	{
		if (ReferenceTransform == referenceTransform)
		{
			return;
		}

		base.SetReferenceTransform(referenceTransform);
		ReferenceRigidbody = ReferenceTransform ? ReferenceTransform.GetAttachedOWRigidbody() : null;
	}

	protected override void UpdatePrevData()
	{
		base.UpdatePrevData();
		_prevVelocity = Velocity;
		_prevAngularVelocity = AngularVelocity;
	}

	protected override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(Velocity);
		writer.Write(AngularVelocity);
	}

	protected override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		Velocity = reader.ReadVector3();
		AngularVelocity = reader.ReadVector3();
	}

	protected override void GetFromAttached()
	{
		GetFromSector();
		if (!ReferenceTransform)
		{
			return;
		}

		transform.position = ReferenceTransform.ToRelPos(AttachedRigidbody.GetPosition());
		transform.rotation = ReferenceTransform.ToRelRot(AttachedRigidbody.GetRotation());
		Velocity = ReferenceRigidbody.ToRelVel(AttachedRigidbody.GetVelocity(), AttachedRigidbody.GetPosition());
		AngularVelocity = ReferenceRigidbody.ToRelAngVel(AttachedRigidbody.GetAngularVelocity());
	}

	protected override void ApplyToAttached()
	{
		ApplyToSector();
		if (!ReferenceTransform)
		{
			return;
		}

		var targetPos = ReferenceTransform.FromRelPos(transform.position);
		var targetRot = ReferenceTransform.FromRelRot(transform.rotation);

		var positionToSet = targetPos;
		var rotationToSet = targetRot;

		if (UseInterpolation)
		{
			positionToSet = ReferenceTransform.FromRelPos(SmoothPosition);
			rotationToSet = ReferenceTransform.FromRelRot(SmoothRotation);
		}

		AttachedRigidbody.MoveToPosition(positionToSet);
		AttachedRigidbody.MoveToRotation(rotationToSet);

		var targetVelocity = ReferenceRigidbody.FromRelVel(Velocity, targetPos);
		var targetAngularVelocity = ReferenceRigidbody.FromRelAngVel(AngularVelocity);

		AttachedRigidbody.SetVelocity(targetVelocity);
		AttachedRigidbody.SetAngularVelocity(targetAngularVelocity);
	}

	protected override bool HasChanged() =>
		base.HasChanged() ||
		Vector3.Distance(Velocity, _prevVelocity) > VelocityChangeThreshold ||
		Vector3.Distance(AngularVelocity, _prevAngularVelocity) > AngularVelocityChangeThreshold;
}