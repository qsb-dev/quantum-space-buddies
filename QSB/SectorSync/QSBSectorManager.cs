using OWML.Common;
using QSB.Events;
using QSB.Player;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.SectorSync
{
	public class QSBSectorManager : MonoBehaviour, IRepeating
	{
		public static QSBSectorManager Instance { get; private set; }
		public bool IsReady { get; private set; }

		private void OnEnable() => RepeatingManager.Repeatings.Add(this);
		private void OnDisable() => RepeatingManager.Repeatings.Remove(this);

		public void Invoke()
		{
			QSBPlayerManager.GetSyncObjects<TransformSync.TransformSync>()
				.Where(x => x.HasAuthority).ToList().ForEach(CheckTransformSyncSector);
		}

		public void Awake()
		{
			Instance = this;
			QSBSceneManager.OnUniverseSceneLoaded += (OWScene scene) => RebuildSectors();
			DebugLog.DebugWrite("Sector Manager ready.", MessageType.Success);
		}

		public void OnDestroy() 
			=> QSBSceneManager.OnUniverseSceneLoaded -= (OWScene scene) => RebuildSectors();

		public void RebuildSectors()
		{
			DebugLog.DebugWrite("Rebuilding sectors...", MessageType.Warning);
			QSBWorldSync.Init<QSBSector, Sector>();
			IsReady = QSBWorldSync.GetWorldObjects<QSBSector>().Any();
		}

		private void CheckTransformSyncSector(TransformSync.TransformSync transformSync)
		{
			var syncedTransform = transformSync.SyncedTransform;
			if (syncedTransform == null || syncedTransform.position == Vector3.zero)
			{
				return;
			}
			var closestSector = transformSync.SectorSync.GetClosestSector(syncedTransform);
			if (closestSector == default(QSBSector))
			{
				return;
			}
			if (closestSector == transformSync.ReferenceSector)
			{
				return;
			}
			transformSync.SetReferenceSector(closestSector);
			SendSector(transformSync.NetId.Value, closestSector);
		}

		private void SendSector(uint id, QSBSector sector) =>
			QSBEventManager.FireEvent(EventNames.QSBSectorChange, id, sector);
	}
}