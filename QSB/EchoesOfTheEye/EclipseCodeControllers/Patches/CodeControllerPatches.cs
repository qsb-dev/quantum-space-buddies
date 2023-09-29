using HarmonyLib;
using QSB.EchoesOfTheEye.EclipseCodeControllers.Messages;
using QSB.EchoesOfTheEye.EclipseCodeControllers.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.EchoesOfTheEye.EclipseCodeControllers.Patches;

public class CodeControllerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(EclipseCodeController4), nameof(EclipseCodeController4.Update))]
	public static bool UpdateReplacement(EclipseCodeController4 __instance)
	{
		if (__instance._movingSelector)
		{
			__instance._currentSelectorPosY = Mathf.MoveTowards(__instance._currentSelectorPosY, __instance._targetSelectorPosY, Time.deltaTime * 1.5f);
			if (OWMath.ApproxEquals(__instance._currentSelectorPosY, __instance._targetSelectorPosY, 0.001f))
			{
				__instance._currentSelectorPosY = __instance._targetSelectorPosY;
				__instance._movingSelector = false;
			}

			for (var i = 0; i < __instance._selectors.Length; i++)
			{
				__instance._selectors[i].SetLocalPositionY(__instance._currentSelectorPosY);
			}
		}

		if (!__instance._playerInteracting && !__instance._movingSelector)
		{
			__instance.enabled = false;
		}

		var flag = OWInput.IsInputMode(InputMode.SatelliteCam);
		__instance._leftRightPrompt.SetVisibility(flag);
		__instance._upDownPrompt.SetVisibility(flag);
		__instance._leavePrompt.SetVisibility(flag);
		if (!flag)
		{
			return false;
		}

		if ((OWInput.IsNewlyPressed(InputLibrary.right, InputMode.All) || OWInput.IsNewlyPressed(InputLibrary.right2, InputMode.All) || OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary, InputMode.All)) && !__instance._gearInterfaceVertical.IsRotating())
		{
			__instance.GetWorldObject<QSBEclipseCodeController>().SendMessage(new RotateDialMessage(true, __instance._selectedDial));
			__instance._dials[__instance._selectedDial].Rotate(true);
			__instance._gearInterfaceHorizontal.AddRotation(-45f, 0f);
			__instance._oneShotAudio.PlayOneShot(AudioType.CodeTotem_Horizontal, 1f);
			__instance._codeCheckDirty = true;
		}
		else if ((OWInput.IsNewlyPressed(InputLibrary.left, InputMode.All) || OWInput.IsNewlyPressed(InputLibrary.left2, InputMode.All) || OWInput.IsNewlyPressed(InputLibrary.toolActionSecondary, InputMode.All)) && !__instance._gearInterfaceVertical.IsRotating())
		{
			__instance.GetWorldObject<QSBEclipseCodeController>().SendMessage(new RotateDialMessage(false, __instance._selectedDial));
			__instance._dials[__instance._selectedDial].Rotate(false);
			__instance._gearInterfaceHorizontal.AddRotation(45f, 0f);
			__instance._oneShotAudio.PlayOneShot(AudioType.CodeTotem_Horizontal, 1f);
			__instance._codeCheckDirty = true;
		}
		else if (OWInput.IsNewlyPressed(InputLibrary.up, InputMode.All) || OWInput.IsNewlyPressed(InputLibrary.up2, InputMode.All))
		{
			__instance._selectedDial = Mathf.Max(__instance._selectedDial - 1, 0);
			__instance.GetWorldObject<QSBEclipseCodeController>().SendMessage(new MoveSelectorMessage(__instance._selectedDial, true));
			if (__instance.MoveSelectorToLocalPositionY(__instance._dials[__instance._selectedDial].transform.localPosition.y))
			{
				__instance._gearInterfaceVertical.AddRotation(45f, 0f);
			}
			else
			{
				__instance._gearInterfaceVertical.PlayFailure(true, 0.5f);
			}
		}
		else if (OWInput.IsNewlyPressed(InputLibrary.down, InputMode.All) || OWInput.IsNewlyPressed(InputLibrary.down2, InputMode.All))
		{
			__instance._selectedDial = Mathf.Min(__instance._selectedDial + 1, __instance._dials.Length - 1);
			__instance.GetWorldObject<QSBEclipseCodeController>().SendMessage(new MoveSelectorMessage(__instance._selectedDial, false));
			if (__instance.MoveSelectorToLocalPositionY(__instance._dials[__instance._selectedDial].transform.localPosition.y))
			{
				__instance._gearInterfaceVertical.AddRotation(-45f, 0f);
			}
			else
			{
				__instance._gearInterfaceVertical.PlayFailure(false, 1f);
			}
		}
		else if (OWInput.IsNewlyPressed(InputLibrary.cancel, InputMode.All))
		{
			__instance.CancelInteraction();
		}

		for (var j = 0; j < __instance._dials.Length; j++)
		{
			if (__instance._dials[j].IsRotating())
			{
				return false;
			}
		}

		if (__instance._codeCheckDirty)
		{
			__instance.CheckForCode();
			__instance._codeCheckDirty = false;
		}

		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(EclipseCodeController4), nameof(EclipseCodeController4.OnPressInteract))]
	public static void OnUse(EclipseCodeController4 __instance)
		=> __instance.GetWorldObject<QSBEclipseCodeController>().SendMessage(new UseControllerMessage(true));

	[HarmonyPostfix]
	[HarmonyPatch(typeof(EclipseCodeController4), nameof(EclipseCodeController4.CancelInteraction))]
	public static void OnLeave(EclipseCodeController4 __instance)
		=> __instance.GetWorldObject<QSBEclipseCodeController>().SendMessage(new UseControllerMessage(false));
}
