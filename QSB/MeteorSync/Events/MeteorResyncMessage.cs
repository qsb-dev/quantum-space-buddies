using QSB.Messaging;
using QuantumUNET.Messages;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.MeteorSync.Events
{
	/// this will be the best and worst thing you've seen outta me :D
	public class MeteorResyncMessage : PlayerMessage
	{
		public bool[] Suspended;
		public float[] Damage;
		public TransformMessage[] MeteorTransforms;

		public float[] Integrity;
		public TransformMessage[] FragmentTransforms;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);

			Suspended = new bool[reader.ReadInt32()];
			for (var i = 0; i < Suspended.Length; i++)
			{
				Suspended[i] = reader.ReadBoolean();
			}
			Damage = new float[reader.ReadInt32()];
			for (var i = 0; i < Damage.Length; i++)
			{
				Damage[i] = reader.ReadSingle();
			}
			MeteorTransforms = new TransformMessage[reader.ReadInt32()];
			for (var i = 0; i < MeteorTransforms.Length; i++)
			{
				MeteorTransforms[i] = reader.ReadMessage<TransformMessage>();
			}

			Integrity = new float[reader.ReadInt32()];
			for (var i = 0; i < Integrity.Length; i++)
			{
				Integrity[i] = reader.ReadSingle();
			}
			FragmentTransforms = new TransformMessage[reader.ReadInt32()];
			for (var i = 0; i < FragmentTransforms.Length; i++)
			{
				FragmentTransforms[i] = reader.ReadMessage<TransformMessage>();
			}
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);

			writer.Write(Suspended.Length);
			foreach (var x in Suspended)
			{
				writer.Write(x);
			}
			writer.Write(Damage.Length);
			foreach (var x in Damage)
			{
				writer.Write(x);
			}
			writer.Write(MeteorTransforms.Length);
			foreach (var x in MeteorTransforms)
			{
				writer.Write(x);
			}

			writer.Write(Integrity.Length);
			foreach (var x in Integrity)
			{
				writer.Write(x);
			}
			writer.Write(FragmentTransforms.Length);
			foreach (var x in FragmentTransforms)
			{
				writer.Write(x);
			}
		}


		public class TransformMessage : QMessageBase
		{
			public Vector3 pos;
			public Quaternion rot;
			public Vector3 vel;
			public Vector3 angVel;

			public override void Deserialize(QNetworkReader reader)
			{
				base.Deserialize(reader);
				pos = reader.ReadVector3();
				rot = reader.ReadQuaternion();
				vel = reader.ReadVector3();
				angVel = reader.ReadVector3();
			}

			public override void Serialize(QNetworkWriter writer)
			{
				base.Serialize(writer);
				writer.Write(pos);
				writer.Write(rot);
				writer.Write(vel);
				writer.Write(angVel);
			}
		}
	}
}
