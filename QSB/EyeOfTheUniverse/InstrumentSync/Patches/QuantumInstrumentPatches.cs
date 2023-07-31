using HarmonyLib;
using QSB.EyeOfTheUniverse.InstrumentSync.Messages;
using QSB.EyeOfTheUniverse.InstrumentSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.InstrumentSync.Patches;

public class QuantumInstrumentPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(QuantumInstrument), nameof(QuantumInstrument.OnPressInteract))]
	public static void OnPressInteract(QuantumInstrument __instance)
		=> __instance.GetWorldObject<QSBQuantumInstrument>().SendMessage(new GatherInstrumentMessage());

	[HarmonyPrefix]
	[HarmonyPatch(typeof(QuantumInstrument), nameof(QuantumInstrument.Update))]
	public static bool Update(QuantumInstrument __instance)
	{
		if (__instance._gatherWithScope && !__instance._waitToFlickerOut)
		{
			__instance._scopeGatherPrompt.SetVisibility(false);
			if (Locator.GetToolModeSwapper().GetSignalScope().InZoomMode()
			    && Vector3.Angle(__instance.transform.position - Locator.GetPlayerCamera().transform.position, Locator.GetPlayerCamera().transform.forward) < 1f)
			{
				__instance._scopeGatherPrompt.SetVisibility(true);
				if (OWInput.IsNewlyPressed(InputLibrary.interact))
				{
					__instance.Gather();
					__instance.GetWorldObject<QSBQuantumInstrument>().SendMessage(new GatherInstrumentMessage());
					Locator.GetPromptManager().RemoveScreenPrompt(__instance._scopeGatherPrompt);
				}
			}
		}

		if (__instance._waitToFlickerOut && Time.time > __instance._flickerOutTime)
		{
			__instance.FinishGather();
		}

		return false;
	}
}