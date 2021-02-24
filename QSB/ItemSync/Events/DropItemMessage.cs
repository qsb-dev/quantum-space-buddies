using QSB.Messaging;
using QSB.SectorSync.WorldObjects;
using QSB.WorldSync;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.ItemSync.Events
{
	public class DropItemMessage : PlayerMessage
	{
		public int ObjectId { get; set; }
		public Vector3 Position { get; set; }
		public Vector3 Normal { get; set; }
		public Transform Parent { get; set; }
		public Sector Sector { get; set; }
		public DetachableFragment DetachableFragment { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			ObjectId = reader.ReadInt32();
			Position = reader.ReadVector3();
			Normal = reader.ReadVector3();
			var sectorId = reader.ReadInt32();
			Sector = QSBWorldSync.GetWorldFromId<QSBSector>(sectorId).AttachedObject;
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ObjectId);
			writer.Write(Position);
			writer.Write(Normal);
			var qsbSector = QSBWorldSync.GetWorldFromUnity<QSBSector, Sector>(Sector);
			writer.Write(qsbSector.ObjectId);
		}
	}
}
