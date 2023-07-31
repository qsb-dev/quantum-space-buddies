using HarmonyLib;
using QSB.Patches;

namespace QSB.PoolSync.Patches;

[HarmonyPatch]
public class PoolPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiRemoteCameraPlatform), nameof(NomaiRemoteCameraPlatform.Awake))]
	public static bool NomaiRemoteCameraPlatform_Awake() => false;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiRemoteCameraPlatform), nameof(NomaiRemoteCameraPlatform.Update))]
	public static bool NomaiRemoteCameraPlatform_Update() => false;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiRemoteCameraPlatform), nameof(NomaiRemoteCameraPlatform.OnSocketableRemoved))]
	public static bool NomaiRemoteCameraPlatform_OnSocketableRemoved() => false;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiRemoteCameraPlatform), nameof(NomaiRemoteCameraPlatform.OnSocketableDonePlacing))]
	public static bool NomaiRemoteCameraPlatform_OnSocketableDonePlacing() => false;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiRemoteCameraPlatform), nameof(NomaiRemoteCameraPlatform.OnPedestalContact))]
	public static bool NomaiRemoteCameraPlatform_OnPedestalContact() => false;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiRemoteCameraPlatform), nameof(NomaiRemoteCameraPlatform.SwitchToPlayerCamera))]
	public static bool NomaiRemoteCameraPlatform_SwitchToPlayerCamera() => false;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiRemoteCameraStreaming), nameof(NomaiRemoteCameraStreaming.FixedUpdate))]
	public static bool NomaiRemoteCameraStreaming_FixedUpdate() => false;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiRemoteCameraStreaming), nameof(NomaiRemoteCameraStreaming.OnSectorOccupantAdded))]
	public static bool NomaiRemoteCameraStreaming_OnSectorOccupantAdded() => false;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiRemoteCameraStreaming), nameof(NomaiRemoteCameraStreaming.OnSectorOccupantRemoved))]
	public static bool NomaiRemoteCameraStreaming_OnSectorOccupantRemoved() => false;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiRemoteCameraStreaming), nameof(NomaiRemoteCameraStreaming.OnEntry))]
	public static bool NomaiRemoteCameraStreaming_OnEntry() => false;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiRemoteCameraStreaming), nameof(NomaiRemoteCameraStreaming.OnExit))]
	public static bool NomaiRemoteCameraStreaming_OnExit() => false;
}