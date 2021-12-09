using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;
using QuantumUNET.Transport;

namespace QSB.MeteorSync.Events
{
	public class MeteorLaunchMessage : QSBWorldObjectMessage<QSBMeteorLauncher>
	{
		private int _meteorId;
		private float _launchSpeed;

		public MeteorLaunchMessage(QSBMeteorLauncher qsbMeteorLauncher)
		{
			_meteorId = qsbMeteorLauncher.MeteorId;
			_launchSpeed = qsbMeteorLauncher.LaunchSpeed;
		}

		public MeteorLaunchMessage() { }

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(_meteorId);
			writer.Write(_launchSpeed);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			_meteorId = reader.ReadInt32();
			_launchSpeed = reader.ReadSingle();
		}

		public override void OnReceiveRemote(uint from) => WorldObject.LaunchMeteor(_meteorId, _launchSpeed);
	}
}
