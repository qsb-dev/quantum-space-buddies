using QSB.HUD;
using QSB.Messaging;
using QSB.ShipSync;
using QSB.ShipSync.Messages;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.Utility;

public class CommandInterpreter : MonoBehaviour, IAddComponentOnStart
{
	public static bool InterpretCommand(string message)
	{
		if (message[0] != '/')
		{
			return false;
		}

		var commandParts = message.ToLower().Substring(1).Split(' ');
		var command = commandParts[0];

		switch (command)
		{
			case "ship":
				ShipCommand(commandParts.Skip(1).ToArray());
				break;
			default:
				MultiplayerHUDManager.Instance.WriteMessage($"Unknown command \"{command}\".", Color.red);
				break;
		}

		return true;
	}

	public static void ShipCommand(string[] arguments)
	{
		var command = arguments[0];

		switch (command)
		{
			case "explode":
				MultiplayerHUDManager.Instance.WriteMessage($"Blowing up the ship.", Color.green);
				var shipDamageController = Locator.GetShipTransform().GetComponentInChildren<ShipDamageController>();
				shipDamageController.Explode();
				break;
			case "repair":
			case "damage":
				var damage = command == "damage";
				switch (arguments[1])
				{
					case "headlight":
						var headlight = QSBWorldSync.GetUnityObject<ShipHeadlightComponent>();
						headlight.SetDamaged(damage);
						break;
					default:
						break;
				}
				MultiplayerHUDManager.Instance.WriteMessage($"{(damage ? "Damaging" : "Repairing")} the {arguments[1]}.", Color.green);
				break;
			case "open-hatch":
				QSBWorldSync.GetUnityObject<HatchController>().OpenHatch();
				new HatchMessage(true).Send();
				break;
			case "close-hatch":
				QSBWorldSync.GetUnityObject<HatchController>().CloseHatch();
				new HatchMessage(false).Send();
				break;
			default:
				MultiplayerHUDManager.Instance.WriteMessage($"Unknown ship command \"{command}\".", Color.red);
				break;
		}
	}
}
