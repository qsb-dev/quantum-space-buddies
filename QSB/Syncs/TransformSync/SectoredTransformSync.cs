using OWML.Utils;
using QSB.SectorSync;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;
using System.Linq;
using UnityEngine;

namespace QSB.Syncs.TransformSync
{
	public abstract class SectoredTransformSync : BaseTransformSync, ISectoredSync<Transform>
	{
		public QSBSector ReferenceSector { get; set; }
		public SectorSync.SectorSync SectorSync { get; private set; }
		public abstract TargetType Type { get; }

		public override void Start()
		{
			SectorSync = gameObject.AddComponent<SectorSync.SectorSync>();
			QSBSectorManager.Instance.SectoredTransformSyncs.Add(this);
			base.Start();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			QSBSectorManager.Instance.SectoredTransformSyncs.Remove(this);
			if (SectorSync != null)
			{
				Destroy(SectorSync);
			}
		}

		protected override void Init()
		{
			base.Init();
			if (!QSBSectorManager.Instance.IsReady)
			{
				return;
			}

			var closestSector = SectorSync.GetClosestSector(AttachedObject.transform);
			if (closestSector != null)
			{
				SetReferenceTransform(closestSector.Transform);
			}
		}

		public override void SerializeTransform(QNetworkWriter writer)
		{
			if (_intermediaryTransform == null)
			{
				_intermediaryTransform = new IntermediaryTransform(transform);
			}

			if (ReferenceSector != null)
			{
				writer.Write(ReferenceSector.ObjectId);
			}
			else
			{
				writer.Write(-1);
			}

			base.SerializeTransform(writer);
		}

		public override void DeserializeTransform(QNetworkReader reader)
		{
			if (!QSBCore.WorldObjectsReady)
			{
				reader.ReadInt32();
				reader.ReadVector3();
				DeserializeRotation(reader);
				return;
			}

			var sectorId = reader.ReadInt32();
			var sector = sectorId == -1
				? null
				: QSBWorldSync.GetWorldFromId<QSBSector>(sectorId);

			if (sector != ReferenceSector)
			{
				DebugLog.DebugWrite($"DESERAILIZE new sector ({ReferenceSector.Name} to {sector.Name})");
				SetReferenceSector(sector);
			}

			base.DeserializeTransform(reader);
		}

		protected override void UpdateTransform()
		{
			if ((ReferenceTransform == null || ReferenceSector == null) && QSBSectorManager.Instance.IsReady)
			{
				var closestSector = SectorSync.GetClosestSector(AttachedObject.transform);
				if (closestSector != null)
				{
					SetReferenceTransform(closestSector.Transform);
				}
				else
				{
					return;
				}
			}

			base.UpdateTransform();
		}

		public void SetReferenceSector(QSBSector sector)
		{
			DebugLog.DebugWrite($"{Player.PlayerId}.{GetType().Name} set reference sector to {sector.Name}");
			ReferenceSector = sector;
			SetReferenceTransform(sector?.Transform);
		}

		protected override void OnRenderObject()
		{
			if (!QSBCore.WorldObjectsReady
				|| !QSBCore.DebugMode
				|| !QSBCore.ShowLinesInDebug
				|| !IsReady
				|| ReferenceTransform == null)
			{
				return;
			}

			base.OnRenderObject();

			var allSectorsCurrentlyIn = SectorSync.SectorList;
			var allSectorScores = allSectorsCurrentlyIn.Select(x => QSB.SectorSync.SectorSync.CalculateSectorScore(x, AttachedObject.transform, SectorSync.GetValue<OWRigidbody>("_attachedOWRigidbody")));

			foreach (var sector in allSectorsCurrentlyIn)
			{
				var sectorScore = QSB.SectorSync.SectorSync.CalculateSectorScore(sector, AttachedObject.transform, SectorSync.GetValue<OWRigidbody>("_attachedOWRigidbody"));

				var mappedScore = sectorScore.Map(allSectorScores.Min(), allSectorScores.Max(), 0, 1);
				Popcron.Gizmos.Line(AttachedObject.transform.position, sector.Transform.position, Color.Lerp(Color.green, Color.red, mappedScore));
			}
			
		}
	}
}
