using QSB.Anglerfish.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

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

		public void TransferAuthority(uint id)
		{
			var conn = QNetworkServer.connections.First(x => x.GetPlayerId() == id);
			var identity = transformSync.NetIdentity;

			if (identity.ClientAuthorityOwner == conn)
			{
				return;
			}

			if (identity.ClientAuthorityOwner != null)
			{
				identity.RemoveClientAuthority(identity.ClientAuthorityOwner);
			}
			identity.AssignClientAuthority(conn);

			DebugLog.DebugWrite($"angler {ObjectId} - transferred authority to {id}");
		}


		public Transform targetTransform;
		public Vector3 TargetVelocity { get; private set; }
		private Vector3 _lastTargetPosition;

		public void FixedUpdate()
		{
			if (targetTransform == null)
			{
				return;
			}
			TargetVelocity = targetTransform.position - _lastTargetPosition;
			_lastTargetPosition = targetTransform.position;
		}
	}
}
