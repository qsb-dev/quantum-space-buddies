using QSB.MeteorSync.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using Object = UnityEngine.Object;

namespace QSB.MeteorSync.WorldObjects
{
	public class QSBMeteor : WorldObject<MeteorController>
	{
		public MeteorTransformSync TransformSync;
		private bool _initialized;

		public override void Init(MeteorController attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;

			// remove WorldObject if prefab
			if (AttachedObject.gameObject.scene.name == null)
			{
				QSBWorldSync.RemoveWorldObject(this);
				return;
			}

			if (QSBCore.IsHost)
			{
				Object.Instantiate(QSBNetworkManager.Instance.MeteorPrefab).SpawnWithServerAuthority();
			}

			_initialized = true;
		}

		public override void OnRemoval()
		{
			if (!_initialized)
			{
				return;
			}

			if (QSBCore.IsHost)
			{
				QNetworkServer.Destroy(TransformSync.gameObject);
			}

			MeteorManager.AllReady = false;
		}
	}
}
