using QSB.HUD;
using QSB.Messaging;
using QSB.ShipSync.Messages;
using QSB.WorldSync;
using Steamworks;
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
			case "copy-id":
				CopySteamID();
				break;
			default:
				WriteToChat($"Unknown command \"{command}\".", Color.red);
				break;
		}

		return true;
	}

	private static void WriteToChat(string message, Color color)
	{
		// TODO : make italics work in chat so we can use them here
		MultiplayerHUDManager.Instance.WriteMessage(message, color);
	}

	public static void CopySteamID()
	{
		if (QSBCore.UseKcpTransport)
		{
			WriteToChat($"Cannot get Steam ID for KCP-hosted server.", Color.red);
			return;
		}

		var steamID = QSBCore.IsHost
			? SteamUser.GetSteamID().ToString()
			: QSBNetworkManager.singleton.networkAddress;

		GUIUtility.systemCopyBuffer = steamID;
		WriteToChat($"Copied {steamID} to the clipboard.", Color.green);
	}

	public static void ShipCommand(string[] arguments)
	{
		var command = arguments[0];

		switch (command)
		{
/*			case "explode":
				WriteToChat($"Blowing up the ship.", Color.green);
				var shipDamageController = Locator.GetShipTransform().GetComponentInChildren<ShipDamageController>();
				shipDamageController.Explode();
				break;*/
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
				WriteToChat($"{(damage ? "Damaging" : "Repairing")} the {arguments[1]}.", Color.green);
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
				WriteToChat($"Unknown ship command \"{command}\".", Color.red);
				break;
		}
	}
}
