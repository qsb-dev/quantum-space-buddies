﻿using QSB.OrbSync.WorldObjects;
using QSB.OwnershipSync;
using QSB.Syncs.Unsectored.Transforms;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.OrbSync.TransformSync;

public class NomaiOrbTransformSync : UnsectoredTransformSync, ILinkedNetworkBehaviour
{
	/// <summary>
	/// normally prints error when attached object is null.
	/// this overrides it so that doesn't happen, since the orb can be destroyed.
	/// </summary>
	protected override bool CheckValid() => AttachedTransform && base.CheckValid();

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

	protected override void Init()
	{
		base.Init();
		var body = AttachedTransform.GetAttachedOWRigidbody();
		SetReferenceTransform(body.GetOrigParent());

		body.OnUnsuspendOWRigidbody += OnUnsuspend;
		body.OnSuspendOWRigidbody += OnSuspend;
		netIdentity.UpdateOwnerQueue(body.IsSuspended() ? OwnerQueueAction.Remove : OwnerQueueAction.Add);
	}

	protected override void Uninit()
	{
		base.Uninit();

		var body = AttachedTransform.GetAttachedOWRigidbody();
		body.OnUnsuspendOWRigidbody -= OnUnsuspend;
		body.OnSuspendOWRigidbody -= OnSuspend;
	}

	private void OnUnsuspend(OWRigidbody suspendedBody) => netIdentity.UpdateOwnerQueue(OwnerQueueAction.Add);
	private void OnSuspend(OWRigidbody suspendedBody) => netIdentity.UpdateOwnerQueue(OwnerQueueAction.Remove);

	protected override void ApplyToAttached()
	{
		base.ApplyToAttached();

		_qsbOrb.AttachedObject.SetTargetPosition(AttachedTransform.position);
	}
}
