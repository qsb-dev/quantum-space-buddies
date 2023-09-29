using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ShipSync.Messages;

public class HatchMessage : QSBMessage<bool>
{
	public HatchMessage(bool open) : base(open) { }

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