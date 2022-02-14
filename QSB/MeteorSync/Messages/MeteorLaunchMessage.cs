using Mirror;
using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.MeteorSync.Messages
{
	public class MeteorLaunchMessage : QSBWorldObjectMessage<QSBMeteorLauncher>
	{
		private int MeteorId;
		private float LaunchSpeed;

		public MeteorLaunchMessage(MeteorController meteorController, float launchSpeed)
		{
			MeteorId = meteorController.GetWorldObject<QSBMeteor>().ObjectId;
			LaunchSpeed = launchSpeed;
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(MeteorId);
			writer.Write(LaunchSpeed);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			MeteorId = reader.Read<int>();
			LaunchSpeed = reader.Read<float>();
		}

		public override void OnReceiveRemote() =>
			WorldObject.LaunchMeteor(MeteorId.GetWorldObject<QSBMeteor>(), LaunchSpeed);
	}
}
