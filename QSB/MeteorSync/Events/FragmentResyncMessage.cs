using QSB.WorldSync.Events;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.MeteorSync.Events
{
	public class FragmentResyncMessage : WorldObjectMessage
	{
		public float Integrity;
		public float OrigIntegrity;
		public float LeashLength;

		public Vector3 Pos;
		public Quaternion Rot;
		public Vector3 Vel;
		public Vector3 AngVel;
		public bool IsThruWhiteHole;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Integrity = reader.ReadSingle();
			OrigIntegrity = reader.ReadSingle();
			LeashLength = reader.ReadSingle();
			if (Integrity <= 0)
			{
				Pos = reader.ReadVector3();
				Rot = reader.ReadQuaternion();
				Vel = reader.ReadVector3();
				AngVel = reader.ReadVector3();
				IsThruWhiteHole = reader.ReadBoolean();
			}
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Integrity);
			writer.Write(OrigIntegrity);
			writer.Write(LeashLength);
			if (Integrity <= 0)
			{
				writer.Write(IsThruWhiteHole);
			}
		}
	}
}
