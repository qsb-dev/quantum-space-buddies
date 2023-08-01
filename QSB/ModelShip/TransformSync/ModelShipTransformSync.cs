using QSB.ShipSync;
using QSB.Syncs.Sectored.Rigidbodies;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ModelShip.TransformSync;

public class ModelShipTransformSync : SectoredRigidbodySync
{
	public static ModelShipTransformSync LocalInstance { get; private set; }

	public ModelShipThrusterVariableSyncer ThrusterVariableSyncer { get; private set; }

	public override void OnStartClient()
	{
		base.OnStartClient();
		LocalInstance = this;
	}

	/// <summary>
	/// normally prints error when attached object is null.
	/// this overrides it so that doesn't happen, since the model ship can be destroyed.
	/// </summary>
	protected override bool CheckValid()
		=> AttachedTransform
			&& base.CheckValid();

	protected override bool UseInterpolation => true;

	protected override OWRigidbody InitAttachedRigidbody()
	{
		var modelShip = QSBWorldSync.GetUnityObject<RemoteFlightConsole>()._modelShipBody;
		SectorDetector.Init(modelShip.transform.Find("Detector").GetComponent<SectorDetector>());
		return modelShip;
	}

	protected override void Init()
	{
		base.Init();

		ThrusterVariableSyncer = this.GetRequiredComponent<ModelShipThrusterVariableSyncer>();
		ThrusterVariableSyncer.Init(AttachedRigidbody.gameObject);
	}

	/// <summary>
	/// replacement for base method
	/// using SetPos/Rot instead of Move
	/// </summary>
	protected override void ApplyToAttached()
	{
		ApplyToSector();
		if (!ReferenceTransform)
		{
			return;
		}

		var targetPos = ReferenceTransform.FromRelPos(SmoothPosition);
		var targetRot = ReferenceTransform.FromRelRot(SmoothRotation);

		AttachedRigidbody.SetPosition(targetPos);
		AttachedRigidbody.SetRotation(targetRot);

		var targetVelocity = ReferenceRigidbody.FromRelVel(Velocity, targetPos);
		var targetAngularVelocity = ReferenceRigidbody.FromRelAngVel(AngularVelocity);

		AttachedRigidbody.SetVelocity(targetVelocity);
		AttachedRigidbody.SetAngularVelocity(targetAngularVelocity);
	}
}
