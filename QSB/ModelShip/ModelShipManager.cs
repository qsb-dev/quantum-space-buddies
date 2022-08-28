using Cysharp.Threading.Tasks;
using Mirror;
using OWML.Common;
using QSB.ModelShip.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using System.Threading;

namespace QSB.ModelShip;

internal class ModelShipManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => false;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		if (QSBCore.IsHost)
		{
			Instantiate(QSBNetworkManager.singleton.ModelShipPrefab).SpawnWithServerAuthority();
		}

		// Is 0 by default -> 2D
		QSBWorldSync.GetUnityObject<RemoteFlightConsole>()._consoleAudio.spatialBlend = 1;
	}

	public override void UnbuildWorldObjects()
	{
		if (QSBCore.IsHost)
		{
			if (ModelShipTransformSync.LocalInstance != null)
			{
				if (ModelShipTransformSync.LocalInstance.gameObject == null)
				{
					DebugLog.ToConsole($"Warning - ShipTransformSync's LocalInstance is not null, but it's gameobject is null!", MessageType.Warning);
					return;
				}

				NetworkServer.Destroy(ModelShipTransformSync.LocalInstance.gameObject);
			}
		}
	}
}
