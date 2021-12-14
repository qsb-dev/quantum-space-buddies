using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.Audio
{
	public class QSBPlayerAudioController : MonoBehaviour
	{
		//public OWAudioSource _oneShotSource;
		public OWAudioSource _oneShotExternalSource;
		//public OWAudioSource _mapTrackSource;
		public OWAudioSource _repairToolSource;
		//public OWAudioSource _translatorSource;
		//public OWAudioSource _damageAudioSource;
		//public OWAudioSource _damageAudioSourceExternal;
		//public OWAudioSource _notificationAudio;
		//public OWAudioSource _fluidVolumeSource;
		//public OWAudioSource _forceVolumeAudio;
		//public OWAudioSource _oxygenLeakSource;
		//public OWAudioSource _recorderLoopSource;
		//public NomaiTextRevealAudioController[] _nomaiTextAudioControllers;

		private void Start()
		{
			DebugLog.DebugWrite($"START");
			_oneShotExternalSource = CreateBaseAudio("OneShotAudio_PlayerExternal", false, 0, 1, AudioType.None, OWAudioMixer.TrackName.Player_External, false);
			_repairToolSource = CreateBaseAudio("RepairToolAudio", true, 128, 0.5f, AudioType.None, OWAudioMixer.TrackName.Player_External, false);
		}

		public void PlayEquipTool()
			=> _oneShotExternalSource.PlayOneShot(AudioType.ToolTranslatorEquip, 1f);

		public void PlayUnequipTool()
			=> _oneShotExternalSource.PlayOneShot(AudioType.ToolTranslatorUnequip, 1f);

		public void PlayTurnOnFlashlight()
			=> _oneShotExternalSource.PlayOneShot(AudioType.ToolFlashlightOn, 1f);

		public void PlayTurnOffFlashlight()
			=> _oneShotExternalSource.PlayOneShot(AudioType.ToolFlashlightOff, 1f);

		private OWAudioSource CreateBaseAudio(
			string name,
			bool loop,
			int priority,
			float volume,
			AudioType audioLibraryClip,
			OWAudioMixer.TrackName track,
			bool randomize)
		{
			DebugLog.DebugWrite($"createBaseAudio {name}");
			var go = new GameObject(name);
			go.transform.parent = transform;
			go.transform.localPosition = Vector3.zero;
			go.transform.localScale = Vector3.one;

			var audioSource = go.AddComponent<AudioSource>();
			audioSource.mute = false;
			audioSource.bypassEffects = false;
			audioSource.bypassListenerEffects = false;
			audioSource.bypassReverbZones = false;
			audioSource.playOnAwake = false;
			audioSource.loop = loop;
			audioSource.priority = priority;
			audioSource.volume = volume;
			audioSource.spatialBlend = 1f;

			var owAudioSource = go.AddComponent<OWAudioSource>();
			owAudioSource._audioLibraryClip = audioLibraryClip;
			owAudioSource._clipSelectionOnPlay = OWAudioSource.ClipSelectionOnPlay.RANDOM;
			owAudioSource._track = track;
			owAudioSource._randomizePlayheadOnAwake = randomize;

			return owAudioSource;
		}
	}
}
