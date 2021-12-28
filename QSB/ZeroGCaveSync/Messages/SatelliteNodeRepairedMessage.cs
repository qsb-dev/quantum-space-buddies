using QSB.Messaging;
using QSB.ZeroGCaveSync.WorldObjects;

namespace QSB.ZeroGCaveSync.Messages
{
	internal class SatelliteNodeRepairedMessage : QSBWorldObjectMessage<QSBSatelliteNode>
	{
		public override void OnReceiveRemote() => WorldObject.SetRepaired();
	}
}