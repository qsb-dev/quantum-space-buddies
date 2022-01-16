using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ShipSync.Messages
{
	internal class FunnelEnableMessage : QSBMessage
	{
		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveRemote()
			=> ShipManager.Instance.ShipTractorBeam.ActivateTractorBeam();
	}
}
