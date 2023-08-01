using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ShipSync.Messages;

public class FunnelEnableMessage : QSBMessage
{
	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
		=> ShipManager.Instance.ShipTractorBeam.ActivateTractorBeam();
}