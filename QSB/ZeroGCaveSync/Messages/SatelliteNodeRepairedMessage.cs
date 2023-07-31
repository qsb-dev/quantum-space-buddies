using QSB.Messaging;
using QSB.ZeroGCaveSync.WorldObjects;

namespace QSB.ZeroGCaveSync.Messages;

public class SatelliteNodeRepairedMessage : QSBWorldObjectMessage<QSBSatelliteNode>
{
	public override void OnReceiveRemote() => WorldObject.SetRepaired();
}