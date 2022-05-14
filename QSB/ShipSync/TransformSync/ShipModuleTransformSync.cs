using QSB.ShipSync.WorldObjects;
using QSB.Syncs.Sectored.Rigidbodies;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;

namespace QSB.ShipSync.TransformSync;

internal class ShipModuleTransformSync : SectoredRigidbodySync, ILinkedNetworkBehaviour
{
	/// <summary>
	/// normally prints error when attached object is null.
	/// this overrides it so that doesn't happen, since the module can be destroyed.
	/// </summary>
	protected override bool CheckValid()
		=> AttachedTransform
		&& base.CheckValid();

	protected override bool CheckReady()
		=> _qsbModule != null
		&& _qsbModule.AttachedObject.isDetached
		&& base.CheckReady();

	protected override bool UseInterpolation => true;
	protected override float DistanceChangeThreshold => 1f;

	private QSBShipDetachableModule _qsbModule;
	public void SetWorldObject(IWorldObject worldObject) => _qsbModule = (QSBShipDetachableModule)worldObject;

	protected override OWRigidbody InitAttachedRigidbody()
	{
		var owRigidbody = _qsbModule.AttachedObject.GetComponent<OWRigidbody>();
		SectorDetector.Init(owRigidbody.transform.Find("DetectorVolume").GetComponent<SectorDetector>());
		return owRigidbody;
	}

	protected override void ApplyToAttached()
	{
		if (!_qsbModule.AttachedObject.isDetached)
		{
			return;
		}

		base.ApplyToAttached();
	}
}
