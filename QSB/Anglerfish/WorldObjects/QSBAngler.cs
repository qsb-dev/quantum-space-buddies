using System.Linq;
using OWML.Common;
using QSB.Anglerfish.TransformSync;
using QSB.Events;
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

			AttachedObject.OnChangeAnglerState += OnChangeState;
		}

		public override void OnRemoval()
		{
			if (QSBCore.IsHost)
			{
				QNetworkServer.Destroy(transformSync.gameObject);
			}

			AttachedObject.OnChangeAnglerState -= OnChangeState;
		}

		private void OnChangeState(AnglerfishController.AnglerState state) =>
			QSBEventManager.FireEvent(EventNames.QSBAnglerChangeState, ObjectId, state);


		public void TransferAuthority(uint id)
		{
			if (!QSBCore.IsHost)
			{
				DebugLog.ToConsole("Error - non-host trying to transfer angler authority", MessageType.Error);
				return;
			}

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
	}
}
