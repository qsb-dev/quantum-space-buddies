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

		var targetPos = ReferenceTransform.FromRelPos(transform.position);
		var targetRot = ReferenceTransform.FromRelRot(transform.rotation);

		if (PlayerState.IsInsideShip())
		{
			if (Time.unscaledTime >= _lastSetPositionTime + ForcePositionAfterTime)
			{
				_lastSetPositionTime = Time.unscaledTime;

				AttachedRigidbody.SetPosition(targetPos);
				AttachedRigidbody.SetRotation(targetRot);
			}
		}
		else
		{
			AttachedRigidbody.SetPosition(targetPos);
			AttachedRigidbody.SetRotation(targetRot);
		}

		var targetVelocity = ReferenceRigidbody.FromRelVel(Velocity, targetPos);
		var targetAngularVelocity = ReferenceRigidbody.FromRelAngVel(AngularVelocity);

		SetVelocity(AttachedRigidbody, targetVelocity);
		AttachedRigidbody.SetAngularVelocity(targetAngularVelocity);
	}

	/// use OWRigidbody version instead of ShipBody override
	private static void SetVelocity(OWRigidbody rigidbody, Vector3 newVelocity)
	{
		if (rigidbody.RunningKinematicSimulation())
		{
			rigidbody._kinematicRigidbody.velocity = newVelocity + Locator.GetCenterOfTheUniverse().GetStaticFrameVelocity_Internal();
		}
		else
		{
			rigidbody._rigidbody.velocity = newVelocity + Locator.GetCenterOfTheUniverse().GetStaticFrameVelocity_Internal();
		}

		rigidbody._lastVelocity = rigidbody._currentVelocity;
		rigidbody._currentVelocity = newVelocity;
	}

	protected override bool UseInterpolation => !PlayerState.IsInsideShip();
}
