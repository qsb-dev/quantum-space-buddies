using OWML.Utils;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.SectorSync.WorldObjects
{
	public class QSBSector : WorldObject<Sector>
	{
		public Sector.Name Type => AttachedObject.GetName();
		public Transform Transform => AttachedObject.transform;
		public Vector3 Position => Transform.position;
		public bool IsFakeSector => AttachedObject.GetType() == typeof(FakeSector);

		public override void Init(Sector sector, int id)
		{
			ObjectId = id;
			AttachedObject = sector;
			if (IsFakeSector)
			{
				QSBSectorManager.Instance.FakeSectors.Add(this);
			}
		}

		public override void OnRemoval()
		{
			if (IsFakeSector)
			{
				QSBSectorManager.Instance.FakeSectors.Remove(this);
			}
		}

		public bool ShouldSyncTo()
		{
			if (Type == Sector.Name.Ship)
			{
				return false;
			}
			if (AttachedObject.name == "Sector_Shuttle" || AttachedObject.name == "Sector_NomaiShuttleInterior")
			{
				if (QSBSceneManager.CurrentScene == OWScene.SolarSystem)
				{
					if (!AttachedObject.gameObject.GetComponentInParent<NomaiShuttleController>().IsPlayerInside())
					{
						return false;
					}
				}
				else if (QSBSceneManager.CurrentScene == OWScene.EyeOfTheUniverse)
				{
					if (!Resources.FindObjectsOfTypeAll<EyeShuttleController>().First().GetValue<bool>("_isPlayerInside"))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}