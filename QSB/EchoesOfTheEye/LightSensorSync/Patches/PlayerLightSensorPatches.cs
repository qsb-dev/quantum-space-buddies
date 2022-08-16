using HarmonyLib;
using QSB.Patches;

/*
 * For those who come here,
 * leave while you still can.
 */

namespace QSB.EchoesOfTheEye.LightSensorSync.Patches;

[HarmonyPatch(typeof(SingleLightSensor))]
internal class PlayerLightSensorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;
}
