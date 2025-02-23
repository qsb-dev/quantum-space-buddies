using QSB.Anglerfish.WorldObjects;
using QSB.OwnershipSync;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Anglerfish.TransformSync;

public class AnglerTransformSync : UnsectoredRigidbodySync, ILinkedNetworkBehaviour
{
	protected override bool UseInterpolation => false;
	protected override bool AllowInactiveAttachedObject => true; // since they deactivate when suspended

	private QSBAngler _qsbAngler;
	public void SetWorldObject(IWorldObject worldObject) => _qsbAngler = (QSBAngler)worldObject;

	protected override OWRigidbody InitAttachedRigidbody()
		=> _qsbAngler.AttachedObject._anglerBody;

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

	protected override float SendInterval => 1;
	protected override bool UseReliableRpc => true;

	protected override void Init()
	{
		base.Init();
		SetReferenceTransform(_qsbAngler.AttachedObject._brambleBody.transform);

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

		Popcron.Gizmos.Sphere(AttachedRigidbody.GetPosition(), _qsbAngler.AttachedObject._arrivalDistance, Color.blue);
		Popcron.Gizmos.Sphere(AttachedRigidbody.GetPosition(), _qsbAngler.AttachedObject._pursueDistance, Color.red);
		Popcron.Gizmos.Sphere(AttachedRigidbody.GetPosition(), _qsbAngler.AttachedObject._escapeDistance, Color.yellow);
		Popcron.Gizmos.Sphere(AttachedRigidbody.GetPosition()
			+ AttachedRigidbody.transform.TransformDirection(_qsbAngler.AttachedObject._mouthOffset), 3, Color.grey);

		if (_qsbAngler.TargetTransform)
		{
			Popcron.Gizmos.Line(_qsbAngler.TargetTransform.position, AttachedRigidbody.GetPosition(), Color.gray);
			Popcron.Gizmos.Line(_qsbAngler.TargetTransform.position, _qsbAngler.TargetTransform.position + _qsbAngler.TargetVelocity, Color.green);
			Popcron.Gizmos.Line(AttachedRigidbody.GetPosition(), _qsbAngler.AttachedObject._targetPos, Color.red);
			Popcron.Gizmos.Sphere(_qsbAngler.AttachedObject._targetPos, 5, Color.red);
		}

		// Popcron.Gizmos.Line(AttachedObject.GetPosition(), _qsbAngler.AttachedObject.GetTargetPosition(), Color.white);
	}
}
