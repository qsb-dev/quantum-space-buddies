using HarmonyLib;
using QSB.Events;
using QSB.OrbSync.WorldObjects;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.OrbSync.Patches
{
	[HarmonyPatch(typeof(NomaiInterfaceOrb))]
	public class OrbPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPostfix]
		[HarmonyPatch(nameof(NomaiInterfaceOrb.StartDragFromPosition))]
		public static void StartDragFromPosition(NomaiInterfaceOrb __instance)
		{
			if (!__instance._isBeingDragged)
			{
				return;
			}
			if (!WorldObjectManager.AllObjectsReady)
			{
				return;
			}
			var qsbOrb = QSBWorldSync.GetWorldFromUnity<QSBOrb>(__instance);
			QSBEventManager.FireEvent(EventNames.QSBOrbDrag, qsbOrb, true);
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(NomaiInterfaceOrb.CancelDrag))]
		public static void CancelDrag(NomaiInterfaceOrb __instance)
		{
			if (!__instance._isBeingDragged)
			{
				return;
			}
			if (!WorldObjectManager.AllObjectsReady)
			{
				return;
			}
			var qsbOrb = QSBWorldSync.GetWorldFromUnity<QSBOrb>(__instance);
			if (!qsbOrb.TransformSync.HasAuthority)
			{
				return;
			}
			QSBEventManager.FireEvent(EventNames.QSBOrbDrag, qsbOrb, false);
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(NomaiInterfaceOrb.MoveTowardPosition))]
		public static bool MoveTowardPosition(NomaiInterfaceOrb __instance)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				return true;
			}
			var qsbOrb = QSBWorldSync.GetWorldFromUnity<QSBOrb>(__instance);
			if (qsbOrb.TransformSync.HasAuthority)
			{
				return true;
			}

			var pointVelocity = __instance._parentBody.GetPointVelocity(__instance._orbBody.GetPosition());
			__instance._orbBody.SetVelocity(pointVelocity);
			if (!__instance._applyForcesWhileMoving)
			{
				__instance._forceApplier.SetApplyForces(false);
			}

			return false;
		}
	}
}
