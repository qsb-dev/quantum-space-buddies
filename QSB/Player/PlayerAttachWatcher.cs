using HarmonyLib;
using QSB.Utility;
using UnityEngine;

namespace QSB.Player;

[HarmonyPatch(typeof(PlayerAttachPoint))]
internal class PlayerAttachWatcher : MonoBehaviour, IAddComponentOnStart
{
	private void Awake()
	{
		Harmony.CreateAndPatchAll(typeof(PlayerAttachWatcher));
		Destroy(this);
	}

	public static PlayerAttachPoint Current { get; private set; }

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PlayerAttachPoint.AttachPlayer))]
	private static void AttachPlayer(PlayerAttachPoint __instance)
	{
		if (Current != null)
		{
			Current.DetachPlayer();
		}

		Current = __instance;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PlayerAttachPoint.DetachPlayer))]
	private static void DetachPlayer(PlayerAttachPoint __instance)
	{
		if (!__instance.enabled)
		{
			return;
		}

		Current = null;
	}
}