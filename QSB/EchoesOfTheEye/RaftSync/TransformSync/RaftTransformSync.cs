using QSB.AuthoritySync;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.Utility;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;
using System;
using UnityEngine;

namespace QSB.EchoesOfTheEye.RaftSync.TransformSync;

public class RaftTransformSync : UnsectoredRigidbodySync, ILinkedNetworkBehaviour
{
	protected override bool UseInterpolation => false;

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
		SetReferenceTransform(AttachedRigidbody.GetOrigParent());

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

	/// <summary>
	/// replacement for base method
	/// using SetPos/Rot instead of Move
	/// </summary>
	protected override void ApplyToAttached()
	{
		var targetPos = ReferenceTransform.FromRelPos(transform.position);

		if (Time.unscaledTime >= _lastSetPositionTime + ForcePositionAfterTime)
		{
			_lastSetPositionTime = Time.unscaledTime;

			var targetRot = ReferenceTransform.FromRelRot(transform.rotation);

			var onRaft = false;
			var localPos = Vector3.zero;
			var localRot = Quaternion.identity;
			if (Locator.GetPlayerController().GetGroundBody() == AttachedRigidbody)
			{
				onRaft = true;
				localPos = AttachedRigidbody.transform.InverseTransformPoint(Locator.GetPlayerTransform().position);
				localRot = AttachedRigidbody.transform.InverseTransformRotation(Locator.GetPlayerTransform().rotation);
			}

			AttachedRigidbody.SetPosition(targetPos);
			AttachedRigidbody.SetRotation(targetRot);

			if (onRaft)
			{
				var playerTransform = Locator.GetPlayerBody().transform;
				playerTransform.position = AttachedRigidbody.transform.TransformPoint(localPos);
				playerTransform.rotation = AttachedRigidbody.transform.TransformRotation(localRot);
			}
		}

		var targetVelocity = ReferenceRigidbody.FromRelVel(Velocity, targetPos);
		var targetAngularVelocity = ReferenceRigidbody.FromRelAngVel(AngularVelocity);

		AttachedRigidbody.SetVelocity(targetVelocity);
		AttachedRigidbody.SetAngularVelocity(targetAngularVelocity);
	}
}
