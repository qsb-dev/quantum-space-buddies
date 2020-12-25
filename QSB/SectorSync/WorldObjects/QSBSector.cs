using QSB.WorldSync;
using UnityEngine;

namespace QSB.SectorSync.WorldObjects
{
	public class QSBSector : WorldObject<Sector>
	{
		public Sector.Name Type => AttachedObject.GetName();
		public string Name => AttachedObject.name;
		public Transform Transform => AttachedObject.transform;
		public Vector3 Position => Transform.position;

		public override void Init(Sector sector, int id)
		{
			ObjectId = id;
			AttachedObject = sector;
		}
	}
}