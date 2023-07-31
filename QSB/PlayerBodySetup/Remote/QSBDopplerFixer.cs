using HarmonyLib;
using QSB.Patches;
using QSB.Utility;
using UnityEngine;

namespace QSB.PlayerBodySetup.Remote;

/// <summary>
/// forces the remote prefab audio sources to use AudioVelocityUpdateMode.Dynamic
/// so the velocity calculation runs on Update (which is when we move remote objects)
/// instead of FixedUpdate
/// </summary>
[RequireComponent(typeof(OWAudioSource))]
public class QSBDopplerFixer : MonoBehaviour
{
	public static void AddDopplerFixers(GameObject prefab) =>
		prefab.GetComponentsInChildren<OWAudioSource>(true)
			.ForEach(x => x.gameObject.AddComponent<QSBDopplerFixer>());
}

[HarmonyPatch(typeof(OWAudioSource))]
public class DopplerFixerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(nameof(OWAudioSource.Awake))]
	[HarmonyPatch(nameof(OWAudioSource.OnGamePaused))]
	[HarmonyPatch(nameof(OWAudioSource.OnGameUnpaused))]
	[HarmonyPatch(nameof(OWAudioSource.Start))]
	private static void FixDoppler(OWAudioSource __instance)
	{
		if (__instance.TryGetComponent<QSBDopplerFixer>(out _))
		{
			__instance._audioSource.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
		}
	}
}
