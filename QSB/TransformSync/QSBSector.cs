using OWML.Common;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.TransformSync
{
    public class QSBSector : WorldObject
    {
        public Sector Sector { get; private set; }
        public Sector.Name Type => Sector.GetName();
        public string Name => Sector.name;
        public Transform Transform => Sector.transform;
        public Vector3 Position => Transform.position;
        
        public void Init(Sector sector, int id)
        {
            if (sector == null)
            {
                DebugLog.ToConsole($"Error - Init of QSBSector with null sector - id of {id}", MessageType.Error);
            }
            else
            {
                DebugLog.ToConsole($"Init of QSBSector with id of {id} and sector of {sector.name}");
            }
            Sector = sector;
            ObjectId = id;
        }
    }
}
