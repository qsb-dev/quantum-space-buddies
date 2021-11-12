using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.MeteorSync.Events
{
	public class MeteorLaunchMessage : WorldObjectMessage
	{
		public bool Flag;
		public int PoolIndex;
		public float LaunchSpeed;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Flag = reader.ReadBoolean();
			PoolIndex = reader.ReadInt32();
			LaunchSpeed = reader.ReadSingle();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Flag);
			writer.Write(PoolIndex);
			writer.Write(LaunchSpeed);
		}
	}
}
