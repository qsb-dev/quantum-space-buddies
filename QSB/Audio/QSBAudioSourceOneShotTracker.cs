using HarmonyLib;
using QSB.Patches;
using UnityEngine;

namespace QSB.Audio;

/// <summary>
/// tracks what audioType was last played on when PlayOneShot is called on an OWAudioSource
/// makes it easier to send a message afterwards syncing what was just played
/// </summary>
[RequireComponent(typeof(OWAudioSource))]
public class QSBAudioSourceOneShotTracker : MonoBehaviour
{
	public AudioType LastPlayed { get; internal set; }
	public float Pitch { get => _source.pitch; }
	public float Volume { get; internal set; }
	public int Index { get; internal set; }

	public void Reset() => LastPlayed = AudioType.None;

	private OWAudioSource _source;
	public void Awake()
	{
		_source = GetComponent<OWAudioSource>();
	}
}

[HarmonyPatch(typeof(OWAudioSource))]
internal class OneShotTrackerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(nameof(OWAudioSource.PlayOneShot), new[] { typeof(AudioType), typeof(float) })]
	private static void TrackOneShot_AudioType(OWAudioSource __instance, AudioType type, float volume)
	{
		var tracker = __instance.gameObject.GetComponent<QSBAudioSourceOneShotTracker>();
		if (tracker)
		{
			tracker.LastPlayed = type;
			tracker.Volume = volume;
			tracker.Index = -1;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(nameof(OWAudioSource.PlayOneShot), new[] { typeof(AudioType), typeof(int), typeof(float) })]
	private static void TrackOneShot_AudioType(OWAudioSource __instance, AudioType type, int index, float volume)
	{
		var tracker = __instance.gameObject.GetComponent<QSBAudioSourceOneShotTracker>();
		if (tracker)
		{
			tracker.LastPlayed = type;
			tracker.Volume = volume;
			tracker.Index = index;
		}
	}
}
