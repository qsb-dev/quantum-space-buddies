using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.MeteorSync.Events
{
	public class MeteorLaunchMessage : WorldObjectMessage
	{
		public float LaunchSpeed;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			LaunchSpeed = reader.ReadSingle();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(LaunchSpeed);
		}
	}
}
