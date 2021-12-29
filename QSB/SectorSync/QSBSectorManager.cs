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
	public class QSBSectorManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		public static QSBSectorManager Instance { get; private set; }
		public bool IsReady { get; private set; }
		public readonly List<QSBSector> FakeSectors = new();

		public readonly List<IBaseSectoredSync> SectoredSyncs = new();

		#region repeating timer

		private const float TimeInterval = 0.4f;
		private float _checkTimer = TimeInterval;

		private void Update()
		{
			_checkTimer += Time.unscaledDeltaTime;
			if (_checkTimer < TimeInterval)
			{
				return;
			}

			Invoke();

			_checkTimer = 0;
		}

		#endregion

		public void Invoke()
		{
			if (!Instance.IsReady || !AllObjectsReady)
			{
				return;
			}

			foreach (var sync in SectoredSyncs)
			{
				if (sync.ReturnObject() == null)
				{
					continue;
				}

				if (sync.HasAuthority
					&& sync.ReturnObject().gameObject.activeInHierarchy
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

		private void CheckTransformSyncSector(IBaseSectoredSync transformSync)
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