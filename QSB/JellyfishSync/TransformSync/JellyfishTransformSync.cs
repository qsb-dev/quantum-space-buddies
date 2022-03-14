using Mirror;
using QSB.AuthoritySync;
using QSB.JellyfishSync.WorldObjects;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.Utility;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.JellyfishSync.TransformSync;

public class JellyfishTransformSync : UnsectoredRigidbodySync, ILinkedNetworkBehaviour<QSBJellyfish>
{
	protected override bool UseInterpolation => false;
	protected override bool AllowInactiveAttachedObject => true; // since they deactivate when suspended

	public QSBJellyfish WorldObject { get; private set; }
	public void LinkTo(IWorldObject worldObject) => WorldObject = (QSBJellyfish)worldObject;

	protected override OWRigidbody InitAttachedRigidbody()
		=> WorldObject.AttachedObject._jellyfishBody;

	public override void OnStartClient()
	{
		if (QSBCore.IsHost)
		{
			netIdentity.RegisterAuthQueue();
		}

		base.OnStartClient();
	}

	public override void OnStopClient()
	{
		if (QSBCore.IsHost)
		{
			netIdentity.UnregisterAuthQueue();
		}

		base.OnStopClient();
	}

	protected override float SendInterval => 10;
	protected override bool UseReliableRpc => true;

	protected override void Init()
	{
		base.Init();
		SetReferenceTransform(WorldObject.AttachedObject._planetBody.transform);

		AttachedRigidbody.OnUnsuspendOWRigidbody += OnUnsuspend;
		AttachedRigidbody.OnSuspendOWRigidbody += OnSuspend;
		netIdentity.UpdateAuthQueue(AttachedRigidbody.IsSuspended() ? AuthQueueAction.Remove : AuthQueueAction.Add);
	}

	protected override void Uninit()
	{
		base.Uninit();

		AttachedRigidbody.OnUnsuspendOWRigidbody -= OnUnsuspend;
		AttachedRigidbody.OnSuspendOWRigidbody -= OnSuspend;
	}

	private void OnUnsuspend(OWRigidbody suspendedBody) => netIdentity.UpdateAuthQueue(AuthQueueAction.Add);
	private void OnSuspend(OWRigidbody suspendedBody) => netIdentity.UpdateAuthQueue(AuthQueueAction.Remove);

	protected override void Serialize(NetworkWriter writer)
	{
		writer.Write(WorldObject.AttachedObject._isRising);
		base.Serialize(writer);
	}

	protected override void Deserialize(NetworkReader reader)
	{
		WorldObject.SetIsRising(reader.Read<bool>());
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
		if (!QSBCore.DebugSettings.DrawLines
			|| !IsValid
			|| !ReferenceTransform
			|| !AttachedTransform.gameObject.activeInHierarchy)
		{
			return;
		}

		base.OnRenderObject();

		var jellyfish = WorldObject.AttachedObject;
		var position = ReferenceTransform.position;
		var dir = Vector3.Normalize(jellyfish.transform.position - position);
		// Popcron.Gizmos.Line(position + dir * jellyfish._lowerLimit, position + dir * jellyfish._upperLimit, Color.magenta);
		Popcron.Gizmos.Sphere(position + dir * jellyfish._lowerLimit, 10f, Color.magenta);
		Popcron.Gizmos.Sphere(position + dir * jellyfish._upperLimit, 10f, Color.magenta);
	}
}
