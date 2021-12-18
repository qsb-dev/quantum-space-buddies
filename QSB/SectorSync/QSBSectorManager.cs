using OWML.Common;
using QSB.SectorSync.WorldObjects;
using QSB.Syncs.Sectored;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.SectorSync
{
	public class QSBSectorManager : WorldObjectManager, IRepeating
	{
		public static QSBSectorManager Instance { get; private set; }
		public bool IsReady { get; private set; }
		public List<QSBSector> FakeSectors = new();

		private void OnEnable() => RepeatingManager.Repeatings.Add(this);
		private void OnDisable() => RepeatingManager.Repeatings.Remove(this);

		public List<BaseSectoredSync> SectoredSyncs = new();

		public void Invoke()
		{
			if (!Instance.IsReady || !AllObjectsReady)
			{
				return;
			}

			foreach (var sync in SectoredSyncs)
			{
				if (sync.AttachedObject == null)
				{
					continue;
				}

				if (sync.HasAuthority
					&& sync.AttachedObject.gameObject.activeInHierarchy
					&& sync.IsReady
					&& sync.SectorSync.IsReady)
				{
					CheckTransformSyncSector(sync);
				}
			}
		}

		public override void Awake()
		{
			base.Awake();
			Instance = this;
			DebugLog.DebugWrite("Sector Manager ready.", MessageType.Success);
		}

		protected override void RebuildWorldObjects(OWScene scene)
		{
			DebugLog.DebugWrite("Rebuilding sectors...", MessageType.Info);
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

		private void CheckTransformSyncSector(BaseSectoredSync transformSync)
		{
			var closestSector = transformSync.SectorSync.GetClosestSector();
			if (closestSector == default(QSBSector))
			{
				return;
			}

			if (closestSector == transformSync.ReferenceSector)
			{
				return;
			}

			transformSync.SetReferenceSector(closestSector);
		}
	}
}