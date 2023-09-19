using QSB.OwnershipSync;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.Utility;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;
using System;
using UnityEngine;

namespace QSB.EchoesOfTheEye.RaftSync.TransformSync;

public class RaftTransformSync : UnsectoredRigidbodySync, ILinkedNetworkBehaviour
{
	/// <summary>
	/// move if on the raft
	/// or in air near the raft
	/// </summary>
	private bool ShouldMovePlayer =>
		Locator.GetPlayerController().GetGroundBody() == AttachedRigidbody ||
		(Locator.GetPlayerController().GetGroundBody() == null && Vector3.Distance(AttachedTransform.position, Locator.GetPlayerBody().GetPosition()) < 10);
	protected override bool UseInterpolation => !ShouldMovePlayer;

	private float _lastSetPositionTime;
	private const float ForcePositionAfterTime = 1;

	private IWorldObject _worldObject;
	public void SetWorldObject(IWorldObject worldObject) => _worldObject = worldObject;

	protected override OWRigidbody InitAttachedRigidbody() =>
		_worldObject.AttachedObject switch
		{
			RaftController x => x._raftBody,
			DreamRaftController x => x._raftBody,
			SealRaftController x => x._raftBody,
			_ => throw new ArgumentOutOfRangeException(nameof(_worldObject.AttachedObject), _worldObject.AttachedObject, null)
		};

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
		SetReferenceTransform(AttachedRigidbody.GetOrigParent());

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


	public override void OnStartAuthority() => DebugLog.DebugWrite($"{this} + OWN");
	public override void OnStopAuthority() => DebugLog.DebugWrite($"{this} - OWN");

	/// <summary>
	/// replacement for base method
	/// using SetPos/Rot instead of Move
	/// </summary>
	protected override void ApplyToAttached()
	{
		if (_worldObject.AttachedObject is RaftController raft)
		{
			if (raft.IsDockingOrDocked())
			{
				// don't sync position if we're docking, dock sequence moves the raft itself
				return;
			}
		}

		var targetPos = ReferenceTransform.FromRelPos(UseInterpolation ? SmoothPosition : transform.position);
		var targetRot = ReferenceTransform.FromRelRot(UseInterpolation ? SmoothRotation : transform.rotation);

		if (ShouldMovePlayer)
		{
			if (Time.unscaledTime >= _lastSetPositionTime + ForcePositionAfterTime)
			{
				_lastSetPositionTime = Time.unscaledTime;

				var playerBody = Locator.GetPlayerBody();
				var relPos = AttachedTransform.ToRelPos(playerBody.GetPosition());
				var relRot = AttachedTransform.ToRelRot(playerBody.GetRotation());

				AttachedRigidbody.SetPosition(targetPos);
				AttachedRigidbody.SetRotation(targetRot);

				playerBody.SetPosition(AttachedTransform.FromRelPos(relPos));
				playerBody.SetRotation(AttachedTransform.FromRelRot(relRot));

				if (!Physics.autoSyncTransforms)
				{
					Physics.SyncTransforms();
				}
			}
		}
		else
		{
			AttachedRigidbody.SetPosition(targetPos);
			AttachedRigidbody.SetRotation(targetRot);
		}

		var targetVelocity = ReferenceRigidbody.FromRelVel(Velocity, targetPos);
		var targetAngularVelocity = ReferenceRigidbody.FromRelAngVel(AngularVelocity);

		AttachedRigidbody.SetVelocity(targetVelocity);
		AttachedRigidbody.SetAngularVelocity(targetAngularVelocity);
	}
}
