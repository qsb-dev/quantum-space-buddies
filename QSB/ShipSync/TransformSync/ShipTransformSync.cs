using QSB.Syncs.Sectored.Rigidbodies;
using QSB.Utility;
using UnityEngine;

namespace QSB.ShipSync.TransformSync;

public class ShipTransformSync : SectoredRigidbodySync
{
	public static ShipTransformSync LocalInstance { get; private set; }

	public ShipThrusterVariableSyncer ThrusterVariableSyncer { get; private set; }

	private float _lastSetPositionTime;
	private const float ForcePositionAfterTime = 1;

	/// <summary>
	/// normally prints error when attached object is null.
	/// this overrides it so that doesn't happen, since the ship can be destroyed.
	/// </summary>
	protected override bool CheckValid()
		=> AttachedTransform
			&& base.CheckValid();

	protected override bool CheckReady() =>
		base.CheckReady() &&
		Locator.GetShipBody();

	public override void OnStartClient()
	{
		base.OnStartClient();
		LocalInstance = this;
	}

	protected override OWRigidbody InitAttachedRigidbody()
	{
		SectorDetector.Init(Locator.GetShipDetector().GetComponent<SectorDetector>());
		return Locator.GetShipBody();
	}

	protected override void Init()
	{
		base.Init();

		ThrusterVariableSyncer = this.GetRequiredComponent<ShipThrusterVariableSyncer>();
		ThrusterVariableSyncer.Init();

		ShipThrusterManager.CreateShipVFX();
	}

	/// Dont do base... this is a replacement!
	protected override void ApplyToAttached()
	{
		ApplyToSector();
		if (!ReferenceTransform)
		{
			return;
		}

		var targetPos = ReferenceTransform.FromRelPos(UseInterpolation ? SmoothPosition : transform.position);
		var targetRot = ReferenceTransform.FromRelRot(UseInterpolation ? SmoothRotation : transform.rotation);

		if (ShouldMovePlayer)
		{
			if (Time.unscaledTime >= _lastSetPositionTime + ForcePositionAfterTime)
			{
				_lastSetPositionTime = Time.unscaledTime;

				if (!PlayerState.IsAttached())
				{
					var playerBody = Locator.GetPlayerBody();
					var relPos = AttachedTransform.ToRelPos(playerBody.GetPosition());
					var relRot = AttachedTransform.ToRelRot(playerBody.GetRotation());

					SetPosition(AttachedRigidbody, targetPos);
					SetRotation(AttachedRigidbody, targetRot);

					playerBody.SetPosition(AttachedTransform.FromRelPos(relPos));
					playerBody.SetRotation(AttachedTransform.FromRelRot(relRot));

					if (!Physics.autoSyncTransforms)
					{
						Physics.SyncTransforms();
					}
				}
				else
				{
					SetPosition(AttachedRigidbody, targetPos);
					SetRotation(AttachedRigidbody, targetRot);
					GlobalMessenger.FireEvent("PlayerRepositioned");
				}
			}
		}
		else
		{
			SetPosition(AttachedRigidbody, targetPos);
			SetRotation(AttachedRigidbody, targetRot);
		}

		var targetVelocity = ReferenceRigidbody.FromRelVel(Velocity, targetPos);
		var targetAngularVelocity = ReferenceRigidbody.FromRelAngVel(AngularVelocity);

		SetVelocity(AttachedRigidbody, targetVelocity);
		AttachedRigidbody.SetAngularVelocity(targetAngularVelocity);
	}

	#region copied from OWRigidbody

	private static void SetPosition(OWRigidbody @this, Vector3 worldPosition)
	{
		@this._transform.position = worldPosition;
	}

	private static void SetRotation(OWRigidbody @this, Quaternion rotation)
	{
		@this._transform.rotation = rotation;
	}

	private static void SetVelocity(OWRigidbody @this, Vector3 newVelocity)
	{
		if (@this.RunningKinematicSimulation())
		{
			@this._kinematicRigidbody.velocity = newVelocity + Locator.GetCenterOfTheUniverse().GetStaticFrameVelocity_Internal();
		}
		else
		{
			@this._rigidbody.velocity = newVelocity + Locator.GetCenterOfTheUniverse().GetStaticFrameVelocity_Internal();
		}
		@this._lastVelocity = @this._currentVelocity;
		@this._currentVelocity = newVelocity;
	}

	#endregion


	private bool ShouldMovePlayer =>
		PlayerState.IsInsideShip() ||
		(PlayerState.InZeroG() && Vector3.Distance(AttachedTransform.position, Locator.GetPlayerBody().GetPosition()) < 100);
	protected override bool UseInterpolation => !ShouldMovePlayer;
}
