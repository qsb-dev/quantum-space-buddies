using QSB.AuthoritySync;
using QSB.OrbSync.WorldObjects;
using QSB.Syncs.Unsectored.Transforms;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.OrbSync.TransformSync;

public class NomaiOrbTransformSync : UnsectoredTransformSync, ILinkedNetworkBehaviour
{
	protected override bool AllowDestroyedAttachedObject => true;

	protected override bool UseInterpolation => true;
	protected override float DistanceChangeThreshold => 1f;

	private QSBOrb _qsbOrb;
	public void SetWorldObject(IWorldObject worldObject) => _qsbOrb = (QSBOrb)worldObject;

	protected override Transform InitLocalTransform() => _qsbOrb.AttachedObject.transform;
	protected override Transform InitRemoteTransform() => _qsbOrb.AttachedObject.transform;

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

	protected override void Init()
	{
		base.Init();
		var body = AttachedTransform.GetAttachedOWRigidbody();
		SetReferenceTransform(body.GetOrigParent());

		body.OnUnsuspendOWRigidbody += OnUnsuspend;
		body.OnSuspendOWRigidbody += OnSuspend;
		netIdentity.UpdateAuthQueue(body.IsSuspended() ? AuthQueueAction.Remove : AuthQueueAction.Add);
	}

	protected override void Uninit()
	{
		base.Uninit();

		var body = AttachedTransform.GetAttachedOWRigidbody();
		body.OnUnsuspendOWRigidbody -= OnUnsuspend;
		body.OnSuspendOWRigidbody -= OnSuspend;
	}

	private void OnUnsuspend(OWRigidbody suspendedBody) => netIdentity.UpdateAuthQueue(AuthQueueAction.Add);
	private void OnSuspend(OWRigidbody suspendedBody) => netIdentity.UpdateAuthQueue(AuthQueueAction.Remove);

	protected override void ApplyToAttached()
	{
		base.ApplyToAttached();

		_qsbOrb.AttachedObject.SetTargetPosition(AttachedTransform.position);
	}
}
