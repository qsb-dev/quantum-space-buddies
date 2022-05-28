using QSB.Syncs.Sectored.Rigidbodies;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.ModelShip.TransformSync;

internal class ModelShipTransformSync : SectoredRigidbodySync
{
	public static ModelShipTransformSync LocalInstance { get; private set; }

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
}
