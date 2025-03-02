using System.Reflection;
using Mirror;
using NewHorizons;
using NewHorizons.Builder.ShipLog;
using NewHorizons.Components.Orbital;
using OWML.Common;
using QSB;
using QSB.HUD;
using QSB.HUD.Messages;
using QSB.Messaging;
using QSB.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace QSBNH
{
    public class QSBNH : MonoBehaviour
    {
	    public static QSBNH Instance;

	    public INewHorizons NewHorizonsAPI;

		private void Start()
	    {
			Instance = this;
			DebugLog.DebugWrite($"Start of QSB-NH compatibility code.", MessageType.Success);
			NewHorizonsAPI = QSBCore.Helper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");

			GlobalMessenger.AddListener(OWEvents.WakeUp, OnWakeUp);
		}

		private void OnWakeUp()
		{
			// Allow time for MultiplayerHUDManager.OnWakeUp to run
			Delay.RunNextFrame(() =>
			{
				var triggers = new List<PlanetTrigger>();

				var currentPlanets = NewHorizons.Main.BodyDict[NewHorizons.Main.Instance.CurrentStarSystem];
				foreach (var planet in currentPlanets)
				{
					if (planet.Object == null)
					{
						continue;
					}

					var nhAstro = planet.Object.GetComponent<NHAstroObject>();

					if (planet.Config?.ShipLog == null)
					{
						continue;
					}

					var _astroObjectToShipLog = (Dictionary<GameObject, ShipLogAstroObject>)typeof(MapModeBuilder)
						.GetField("_astroObjectToShipLog", BindingFlags.Static | BindingFlags.NonPublic)
						.GetValue(null);
					var shipLogAstroObject = _astroObjectToShipLog[planet.Object];

					var astroObjName = nhAstro.isVanilla
						? Enum.GetName(typeof(AstroObject.Name), nhAstro.GetAstroObjectName())
						: planet.Config.name;

					var sprite = shipLogAstroObject._imageObj.GetComponent<Image>().sprite;
					MultiplayerHUDManager.Instance.PlanetToSprite[astroObjName] = sprite;

					if (!nhAstro.isVanilla)
					{
						triggers.Add(MultiplayerHUDManager.CreateTrigger(nhAstro.GetRootSector().gameObject, astroObjName));
					}
				}

				foreach (var trigger in triggers)
				{
					if (!trigger._sector.ContainsOccupant(DynamicOccupant.Player))
					{
						continue;
					}

					MultiplayerHUDManager.HUDIconStack.Push(trigger.PlanetID);
					new PlanetMessage(trigger.PlanetID).Send();
					break;
				}
			});
		}

		public static string HashToMod(int hash)
		{
			foreach (var mod in NewHorizons.Main.MountedAddons)
			{
				var name = mod.ModHelper.Manifest.UniqueName;
				if (name.GetStableHashCode() == hash)
				{
					return name;
				}
			}

			return null;
		}

		public static int[] HashAddonsForSystem(string system)
		{
			if (NewHorizons.Main.BodyDict.TryGetValue(system, out var bodies))
			{
				var addonHashes = bodies
					.Where(x => x.Mod.ModHelper.Manifest.UniqueName != "xen.NewHorizons")
					.Select(x => x.Mod.ModHelper.Manifest.UniqueName.GetStableHashCode())
					.Distinct();

				var nhPlanetHashes = bodies
					.Where(x => x.Mod.ModHelper.Manifest.UniqueName == "xen.NewHorizons")
					.Select(x => x.Config.name.GetStableHashCode());

				return addonHashes.Concat(nhPlanetHashes).ToArray();
			}
			else
			{
				return null;
			}
		}
	}
}
