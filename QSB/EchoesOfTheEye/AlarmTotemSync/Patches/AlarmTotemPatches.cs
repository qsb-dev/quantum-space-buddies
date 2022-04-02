using HarmonyLib;
using QSB.EchoesOfTheEye.AlarmTotemSync.Messages;
using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Patches;

public class AlarmTotemPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AlarmTotem), nameof(AlarmTotem.SetFaceOpen))]
	private static void SetFaceOpen(AlarmTotem __instance, bool open)
	{
		if (Remote)
		{
			return;
		}

		if (__instance._isFaceOpen == open)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBAlarmTotem>()
			.SendMessage(new SetFaceOpenMessage(open));
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AlarmTotem), nameof(AlarmTotem.OnSectorOccupantAdded))]
	private static void OnSectorOccupantAdded(AlarmTotem __instance, SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player && QSBWorldSync.AllObjectsAdded)
		{
			__instance.GetWorldObject<QSBAlarmTotem>()
				.SendMessage(new SetEnabledMessage(true));
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AlarmTotem), nameof(AlarmTotem.OnSectorOccupantRemoved))]
	private static void OnSectorOccupantRemoved(AlarmTotem __instance, SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player && QSBWorldSync.AllObjectsAdded)
		{
			__instance.GetWorldObject<QSBAlarmTotem>()
				.SendMessage(new SetEnabledMessage(false));
		}
	}

	/// <summary>
	/// prefix to check for local visibility
	/// </summary>
	/// <param name="__instance"></param>
	[HarmonyPrefix]
	[HarmonyPatch(typeof(AlarmTotem), nameof(AlarmTotem.FixedUpdate))]
	private static void FixedUpdate(AlarmTotem __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBAlarmTotem>().FixedUpdate();
	}

	/// <summary>
	/// check for global visibility
	/// </summary>
	[HarmonyPrefix]
	[HarmonyPatch(typeof(AlarmTotem), nameof(AlarmTotem.CheckPlayerVisible))]
	private static bool CheckPlayerVisible(AlarmTotem __instance, ref bool __result)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__result = __instance.GetWorldObject<QSBAlarmTotem>().IsVisible();
		return false;
	}
}
