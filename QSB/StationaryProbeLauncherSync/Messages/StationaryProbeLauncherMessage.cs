using QSB.Messaging;
using QSB.StationaryProbeLauncherSync.WorldObjects;

namespace QSB.StationaryProbeLauncherSync.Messages;

public class StationaryProbeLauncherMessage : QSBWorldObjectMessage<QSBStationaryProbeLauncher, bool>
{
	public StationaryProbeLauncherMessage(bool inUse) : base(inUse) { }

	public override void OnReceiveRemote() => WorldObject.OnRemoteUseStateChanged(Data, From);
}
