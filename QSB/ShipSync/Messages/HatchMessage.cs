using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ShipSync.Messages
{
	internal class HatchMessage : QSBMessage<bool>
	{
		public HatchMessage(bool open) => Data = open;

		public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			if (Data)
			{
				ShipManager.Instance.HatchController.OpenHatch();
			}
			else
			{
				ShipManager.Instance.ShipTractorBeam.DeactivateTractorBeam();
				ShipManager.Instance.HatchController.CloseHatch();
			}
		}
	}
}