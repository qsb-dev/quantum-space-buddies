using OWML.Common;
using QSB.Events;
using QSB.Player;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.SectorSync
{
	public class QSBSectorManager : MonoBehaviour, IRepeating
	{
		public static QSBSectorManager Instance { get; private set; }
		public bool IsReady { get; private set; }
		public List<QSBSector> FakeSectors = new List<QSBSector>();

		private void OnEnable() => RepeatingManager.Repeatings.Add(this);
		private void OnDisable() => RepeatingManager.Repeatings.Remove(this);

		public void Invoke()
		{
			QSBPlayerManager.GetSyncObjects<TransformSync.TransformSync>()
				.Where(x => x.HasAuthority).ToList().ForEach(CheckTransformSyncSector);
		}

		public void Awake()
		{
			if (Instance != null)
			{
				DebugLog.ToConsole("Error - Cannot have multiple QSBSectorManagers!", MessageType.Error);
				Destroy(this);
				return;
			}
			Instance = this;
			QSBSceneManager.OnUniverseSceneLoaded += (OWScene scene) => RebuildSectors();
			DebugLog.DebugWrite("Sector Manager ready.", MessageType.Success);
		}

		public void OnDestroy()
			=> QSBSceneManager.OnUniverseSceneLoaded -= (OWScene scene) => RebuildSectors();


		public void RebuildSectors()
		{
			DebugLog.DebugWrite("Rebuilding sectors...", MessageType.Warning);
			if (QSBSceneManager.CurrentScene == OWScene.SolarSystem)
			{
				var timeLoopRing = GameObject.Find("TimeLoopRing_Body");
				if (timeLoopRing != null)
				{
					if (timeLoopRing.GetComponent<FakeSector>() == null)
					{
						timeLoopRing.AddComponent<FakeSector>().AttachedSector = GameObject.Find("Sector_TimeLoopInterior").GetComponent<Sector>();
					}
				}
				else
				{
					DebugLog.ToConsole($"Error - TimeLoopRing_Body not found!", MessageType.Error);
				}
			}
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