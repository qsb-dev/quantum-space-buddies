using QSB.Messaging;
using QSB.StationaryProbeLauncherSync.WorldObjects;

namespace QSB.StationaryProbeLauncherSync.Messages;

public class StationaryProbeLauncherMessage : QSBWorldObjectMessage<QSBStationaryProbeLauncher, (bool, uint)>
{
	public StationaryProbeLauncherMessage(bool inUse, uint userID) : base((inUse, userID)) { }

	public override void OnReceiveRemote() => WorldObject.OnRemoteUseStateChanged(Data.Item1, Data.Item2);
}
