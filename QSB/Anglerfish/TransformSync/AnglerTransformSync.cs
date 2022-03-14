using QSB.Anglerfish.WorldObjects;
using QSB.AuthoritySync;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Anglerfish.TransformSync;

public class AnglerTransformSync : UnsectoredRigidbodySync, ILinkedNetworkBehaviour<QSBAngler>
{
	protected override bool UseInterpolation => false;
	protected override bool AllowInactiveAttachedObject => true; // since they deactivate when suspended

	public QSBAngler WorldObject { get; private set; }
	public void LinkTo(IWorldObject worldObject) => WorldObject = (QSBAngler)worldObject;

	protected override OWRigidbody InitAttachedRigidbody()
		=> WorldObject.AttachedObject._anglerBody;

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

	protected override float SendInterval => 1;
	protected override bool UseReliableRpc => true;

	protected override void Init()
	{
		base.Init();
		SetReferenceTransform(WorldObject.AttachedObject._brambleBody.transform);

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

		Popcron.Gizmos.Sphere(AttachedRigidbody.GetPosition(), WorldObject.AttachedObject._arrivalDistance, Color.blue);
		Popcron.Gizmos.Sphere(AttachedRigidbody.GetPosition(), WorldObject.AttachedObject._pursueDistance, Color.red);
		Popcron.Gizmos.Sphere(AttachedRigidbody.GetPosition(), WorldObject.AttachedObject._escapeDistance, Color.yellow);
		Popcron.Gizmos.Sphere(AttachedRigidbody.GetPosition()
			+ AttachedRigidbody.transform.TransformDirection(WorldObject.AttachedObject._mouthOffset), 3, Color.grey);

		if (WorldObject.TargetTransform)
		{
			Popcron.Gizmos.Line(WorldObject.TargetTransform.position, AttachedRigidbody.GetPosition(), Color.gray);
			Popcron.Gizmos.Line(WorldObject.TargetTransform.position, WorldObject.TargetTransform.position + WorldObject.TargetVelocity, Color.green);
			Popcron.Gizmos.Line(AttachedRigidbody.GetPosition(), WorldObject.AttachedObject._targetPos, Color.red);
			Popcron.Gizmos.Sphere(WorldObject.AttachedObject._targetPos, 5, Color.red);
		}

		// Popcron.Gizmos.Line(AttachedObject.GetPosition(), _qsbAngler.AttachedObject.GetTargetPosition(), Color.white);
	}
}
