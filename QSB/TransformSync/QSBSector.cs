using QSB.WorldSync;
using UnityEngine;

namespace QSB.TransformSync
{
    public class QSBSector : WorldObject
    {
        public Sector Sector { get; private set; }
        public Sector.Name EnumName => Sector.GetName();
        public string GOName => Sector.name;
        public Transform Transform => Sector.transform;
        public Vector3 Position => Transform.position;
        
        public void Init(Sector sector, int id)
        {
            Sector = sector;
            ObjectId = id;
        }
    }
}
