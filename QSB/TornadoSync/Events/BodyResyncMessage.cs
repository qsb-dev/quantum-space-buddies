using QSB.Messaging;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.TornadoSync.Events
{
	public class BodyResyncMessage : PlayerMessage
	{
		public int BodyIndex;
		public int RefBodyIndex;
		public Vector3 Pos;
		public Quaternion Rot;
		public Vector3 Vel;
		public Vector3 AngVel;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			BodyIndex = reader.ReadInt32();
			RefBodyIndex = reader.ReadInt32();
			Pos = reader.ReadVector3();
			Rot = reader.ReadQuaternion();
			Vel = reader.ReadVector3();
			AngVel = reader.ReadVector3();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(BodyIndex);
			writer.Write(RefBodyIndex);
			writer.Write(Pos);
			writer.Write(Rot);
			writer.Write(Vel);
			writer.Write(AngVel);
		}
	}
}
