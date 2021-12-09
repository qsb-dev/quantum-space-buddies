using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;

namespace QSB.MeteorSync.Events
{
	public class MeteorPreLaunchMessage : QSBWorldObjectMessage<QSBMeteorLauncher>
	{
		public override void OnReceiveRemote(uint from) => WorldObject.PreLaunchMeteor();
	}
}
