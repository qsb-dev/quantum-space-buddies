using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ShipSync.Messages
{
	internal class HatchMessage : QSBBoolMessage
	{
		public HatchMessage(bool open) => Value = open;

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			if (Value)
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