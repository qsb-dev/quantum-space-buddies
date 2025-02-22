using QSB.HUD.Messages;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.HUD;

public class PlanetTrigger : SectoredMonoBehaviour
{
	public string PlanetID;

	public override void OnSectorOccupantAdded(SectorDetector detector)
	{
		if (detector.GetOccupantType() != DynamicOccupant.Player)
		{
			return;
		}

		MultiplayerHUDManager.HUDIconStack.Push(PlanetID);
		var top = MultiplayerHUDManager.HUDIconStack.PeekFront();
		new PlanetMessage(top).Send();
	}

	public override void OnSectorOccupantRemoved(SectorDetector detector)
	{
		if (detector.GetOccupantType() != DynamicOccupant.Player)
		{
			return;
		}

		MultiplayerHUDManager.HUDIconStack.Remove(PlanetID);
		var top = MultiplayerHUDManager.HUDIconStack.PeekFront();
		new PlanetMessage(top).Send();
	}
}
