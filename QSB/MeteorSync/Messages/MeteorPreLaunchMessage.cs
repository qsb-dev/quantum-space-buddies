using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;

namespace QSB.MeteorSync.Messages
{
	public class MeteorPreLaunchMessage : QSBWorldObjectMessage<QSBMeteorLauncher>
	{
		public override void OnReceiveRemote() => WorldObject.PreLaunchMeteor();
	}
}