using QSB.Anglerfish.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using UnityEngine;

namespace QSB.Anglerfish.WorldObjects
{
	public class QSBAngler : WorldObject<AnglerfishController>
	{
		public AnglerTransformSync transformSync;

		public override void Init(AnglerfishController attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;

			if (QSBCore.IsHost)
			{
				Object.Instantiate(QSBNetworkManager.Instance.AnglerPrefab).SpawnWithServerAuthority();
			}
		}

		public override void OnRemoval()
		{
			if (QSBCore.IsHost)
			{
				QNetworkServer.Destroy(transformSync.gameObject);
			}
		}
	}
}
