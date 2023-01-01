using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.MeteorSync.Messages;

public class MeteorLaunchMessage : QSBWorldObjectMessage<QSBMeteorLauncher, (int MeteorId, float LaunchSpeed)>
{
	public MeteorLaunchMessage(MeteorController meteor, float launchSpeed) : base((
		meteor.GetWorldObject<QSBMeteor>().ObjectId,
		launchSpeed
	))
	{ }

	public override void OnReceiveRemote() => WorldObject.LaunchMeteor(
		Data.MeteorId.GetWorldObject<QSBMeteor>().AttachedObject,
		Data.LaunchSpeed
	);
}
