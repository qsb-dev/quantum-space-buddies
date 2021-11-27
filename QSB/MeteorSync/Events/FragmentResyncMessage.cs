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
		public bool IsDetached;

		public bool IsThruWhiteHole;
		public Vector3 Pos;
		public Quaternion Rot;
		public Vector3 Vel;
		public Vector3 AngVel;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Integrity = reader.ReadSingle();
			OrigIntegrity = reader.ReadSingle();
			LeashLength = reader.ReadSingle();
			IsDetached = reader.ReadBoolean();
			if (IsDetached)
			{
				IsThruWhiteHole = reader.ReadBoolean();
				Pos = reader.ReadVector3();
				Rot = reader.ReadQuaternion();
				Vel = reader.ReadVector3();
				AngVel = reader.ReadVector3();
			}
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Integrity);
			writer.Write(OrigIntegrity);
			writer.Write(LeashLength);
			writer.Write(IsDetached);
			if (IsDetached)
			{
				writer.Write(IsThruWhiteHole);
				writer.Write(Pos);
				writer.Write(Rot);
				writer.Write(Vel);
				writer.Write(AngVel);
			}
		}
	}
}
