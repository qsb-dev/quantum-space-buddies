using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ShipSync.Messages
{
	internal class FunnelEnableMessage : QSBMessage
	{
		public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

		public override void OnReceiveRemote()
			=> ShipManager.Instance.ShipTractorBeam.ActivateTractorBeam();
	}
}