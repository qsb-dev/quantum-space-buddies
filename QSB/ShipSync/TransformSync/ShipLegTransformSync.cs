using QSB.ShipSync.WorldObjects;
using QSB.Syncs.Sectored.Rigidbodies;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;

namespace QSB.ShipSync.TransformSync;

internal class ShipLegTransformSync : SectoredRigidbodySync, ILinkedNetworkBehaviour
{
	/// <summary>
	/// normally prints error when attached object is null.
	/// this overrides it so that doesn't happen, since the module can be destroyed.
	/// </summary>
	protected override bool CheckValid()
		=> AttachedTransform
			&& base.CheckValid();

	protected override bool CheckReady()
		=> base.CheckReady()
		&& _qsbModule != null
		&& _qsbModule.AttachedObject != null
		&& _qsbModule.AttachedObject.isDetached;

	protected override bool UseInterpolation => true;

	private QSBShipDetachableLeg _qsbModule;
	public void SetWorldObject(IWorldObject worldObject) => _qsbModule = (QSBShipDetachableLeg)worldObject;

	protected override OWRigidbody InitAttachedRigidbody()
	{
		var owRigidbody = _qsbModule.AttachedObject.GetComponent<OWRigidbody>();
		SectorDetector.Init(owRigidbody.transform.Find("DetectorVolume").GetComponent<SectorDetector>());
		return owRigidbody;
	}
}
