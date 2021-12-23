using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;
using QuantumUNET.Transport;

namespace QSB.MeteorSync.Messages
{
	public class MeteorLaunchMessage : QSBWorldObjectMessage<QSBMeteorLauncher>
	{
		private int MeteorId;
		private float LaunchSpeed;

		public MeteorLaunchMessage(QSBMeteorLauncher qsbMeteorLauncher)
		{
			MeteorId = qsbMeteorLauncher.MeteorId;
			LaunchSpeed = qsbMeteorLauncher.LaunchSpeed;
		}

		public MeteorLaunchMessage() { }

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(MeteorId);
			writer.Write(LaunchSpeed);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			MeteorId = reader.ReadInt32();
			LaunchSpeed = reader.ReadSingle();
		}

		public override void OnReceiveRemote() => WorldObject.LaunchMeteor(MeteorId, LaunchSpeed);
	}
}
