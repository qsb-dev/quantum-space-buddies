using QSB.HUD.Messages;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.HUD;

public class PlanetTrigger : SectoredMonoBehaviour
{
	public HUDIcon Icon;

	public override void OnSectorOccupantAdded(SectorDetector detector)
	{
		if (detector.GetOccupantType() != DynamicOccupant.Player)
		{
			return;
		}

		MultiplayerHUDManager.HUDIconStack.Push(Icon);
		var top = MultiplayerHUDManager.HUDIconStack.Peek();
		DebugLog.DebugWrite($"Pushed {Icon}. Top is now {top}");
		new PlanetMessage(top).Send();
	}

	public override void OnSectorOccupantRemoved(SectorDetector detector)
	{
		if (detector.GetOccupantType() != DynamicOccupant.Player)
		{
			return;
		}

		MultiplayerHUDManager.HUDIconStack.Remove(Icon);
		var top = MultiplayerHUDManager.HUDIconStack.Peek();
		DebugLog.DebugWrite($"Removed {Icon}. Top is now {top}");
		new PlanetMessage(top).Send();
	}
}
