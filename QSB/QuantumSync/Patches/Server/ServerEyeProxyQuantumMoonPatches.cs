using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.QuantumSync.Messages;
using QSB.QuantumSync.WorldObjects;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.QuantumSync.Patches.Server;

[HarmonyPatch(typeof(EyeProxyQuantumMoon))]
public class ServerEyeProxyQuantumMoonPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnServerClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(EyeProxyQuantumMoon), nameof(EyeProxyQuantumMoon.ChangeQuantumState))]
	public static bool EyeProxyQuantumMoon_ChangeQuantumState(EyeProxyQuantumMoon __instance, ref bool __result, bool skipInstantVisibilityCheck)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var qsbEyeProxyQuantumMoon = __instance.GetWorldObject<QSBEyeProxyQuantumMoon>();
		if (TimeLoop.GetSecondsRemaining() > 0f && Random.value > 0.3f)
		{
			__instance._moonStateRoot.SetActive(false);
			qsbEyeProxyQuantumMoon.SendMessage(new EyeProxyMoonStateChangeMessage(false, -1f));
			__result = true;
			return false;
		}

		__instance._moonStateRoot.SetActive(true);
		for (var i = 0; i < 20; i++)
		{
			var angle = Random.Range(0f, 360f);
			__instance.transform.localEulerAngles = new Vector3(0f, angle, 0f);
			if (skipInstantVisibilityCheck || !__instance.CheckVisibilityInstantly())
			{
				qsbEyeProxyQuantumMoon.SendMessage(new EyeProxyMoonStateChangeMessage(true, angle));
				__result = true;
				return false;
			}
		}

		__result = true;
		return false;
	}
}
