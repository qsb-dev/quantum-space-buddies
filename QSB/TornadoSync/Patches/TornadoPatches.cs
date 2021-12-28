using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.TornadoSync.Messages;
using QSB.TornadoSync.WorldObjects;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.TornadoSync.Patches
{
	public class TornadoPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(TornadoController), nameof(TornadoController.FixedUpdate))]
		public static bool FixedUpdate(TornadoController __instance)
		{
			if (QSBCore.IsHost && __instance._secondsUntilFormation > 0f)
			{
				__instance._secondsUntilFormation -= Time.fixedDeltaTime;
				if (__instance._secondsUntilFormation < 0f)
				{
					__instance.StartFormation();
					var qsbTornado = __instance.GetWorldObject<QSBTornado>();
					qsbTornado.SendMessage(new TornadoFormStateMessage(qsbTornado.FormState));
					return false;
				}
			}
			else
			{
				if (__instance._tornadoCollapsing)
				{
					__instance.UpdateCollapse();
				}
				else if (__instance._tornadoForming)
				{
					__instance.UpdateFormation();
				}
				if (__instance._isSectorOccupied)
				{
					__instance.UpdateAnimation();
					if (__instance._wander)
					{
						var num = Mathf.PerlinNoise(Time.time * __instance._wanderRate, 0f) * 2f - 1f;
						var num2 = Mathf.PerlinNoise(Time.time * __instance._wanderRate, 5f) * 2f - 1f;
						var localEulerAngles = new Vector3(num * __instance._wanderDegreesX, 0f, num2 * __instance._wanderDegreesZ);
						__instance.transform.localEulerAngles = localEulerAngles;
					}
				}
			}

			return false;
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(TornadoController), nameof(TornadoController.OnEnterCollapseTrigger))]
		public static bool OnEnterCollapseTrigger(TornadoController __instance,
			GameObject hitObject)
		{
			if (QSBCore.IsHost && hitObject.GetComponentInParent<OWRigidbody>().GetMass() > 50f)
			{
				__instance.StartCollapse();
				var qsbTornado = __instance.GetWorldObject<QSBTornado>();
				qsbTornado.SendMessage(new TornadoFormStateMessage(qsbTornado.FormState));
			}

			return false;
		}
	}
}
