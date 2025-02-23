using Mirror;
using QSB.JellyfishSync.WorldObjects;
using QSB.OwnershipSync;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.Utility;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.JellyfishSync.TransformSync;

public class JellyfishTransformSync : UnsectoredRigidbodySync, ILinkedNetworkBehaviour
{
	protected override bool UseInterpolation => false;
	protected override bool AllowInactiveAttachedObject => true; // since they deactivate when suspended

	private QSBJellyfish _qsbJellyfish;
	public void SetWorldObject(IWorldObject worldObject) => _qsbJellyfish = (QSBJellyfish)worldObject;

	protected override OWRigidbody InitAttachedRigidbody()
		=> _qsbJellyfish.AttachedObject._jellyfishBody;

	public override void OnStartClient()
	{
		if (QSBCore.IsHost)
		{
			netIdentity.RegisterOwnerQueue();
		}

		base.OnStartClient();
	}

	public override void OnStopClient()
	{
		if (QSBCore.IsHost)
		{
			netIdentity.UnregisterOwnerQueue();
		}

		base.OnStopClient();
	}

	protected override float SendInterval => 10;
	protected override bool UseReliableRpc => true;

	protected override void Init()
	{
		base.Init();
		SetReferenceTransform(_qsbJellyfish.AttachedObject._planetBody.transform);

		AttachedRigidbody.OnUnsuspendOWRigidbody += OnUnsuspend;
		AttachedRigidbody.OnSuspendOWRigidbody += OnSuspend;
		netIdentity.UpdateOwnerQueue(AttachedRigidbody.IsSuspended() ? OwnerQueueAction.Remove : OwnerQueueAction.Add);
	}

	protected override void Uninit()
	{
		base.Uninit();

		AttachedRigidbody.OnUnsuspendOWRigidbody -= OnUnsuspend;
		AttachedRigidbody.OnSuspendOWRigidbody -= OnSuspend;
	}

	private void OnUnsuspend(OWRigidbody suspendedBody) => netIdentity.UpdateOwnerQueue(OwnerQueueAction.Add);
	private void OnSuspend(OWRigidbody suspendedBody) => netIdentity.UpdateOwnerQueue(OwnerQueueAction.Remove);

	protected override void Serialize(NetworkWriter writer)
	{
		writer.Write(_qsbJellyfish.AttachedObject._isRising);
		base.Serialize(writer);
	}

	protected override void Deserialize(NetworkReader reader)
	{
		_qsbJellyfish.SetIsRising(reader.Read<bool>());
		base.Deserialize(reader);
	}

	/// replacement using SetPosition/Rotation instead of Move
	protected override void ApplyToAttached()
	{
		var pos = ReferenceTransform.FromRelPos(transform.position);
		AttachedRigidbody.SetPosition(pos);
		AttachedRigidbody.SetRotation(ReferenceTransform.FromRelRot(transform.rotation));
		AttachedRigidbody.SetVelocity(ReferenceRigidbody.FromRelVel(Velocity, pos));
		AttachedRigidbody.SetAngularVelocity(ReferenceRigidbody.FromRelAngVel(AngularVelocity));
	}

	protected override void OnRenderObject()
	{
		if (!QSBCore.DrawLines
			|| !IsValid
			|| !ReferenceTransform
			|| !AttachedTransform.gameObject.activeInHierarchy)
		{
			return;
		}

		base.OnRenderObject();

		var jellyfish = _qsbJellyfish.AttachedObject;
		var position = ReferenceTransform.position;
		var dir = Vector3.Normalize(jellyfish.transform.position - position);
		// Popcron.Gizmos.Line(position + dir * jellyfish._lowerLimit, position + dir * jellyfish._upperLimit, Color.magenta);
		Popcron.Gizmos.Sphere(position + dir * jellyfish._lowerLimit, 10f, Color.magenta);
		Popcron.Gizmos.Sphere(position + dir * jellyfish._upperLimit, 10f, Color.magenta);
	}
}
