using UnityEngine;

namespace QSB.Audio
{
	public class QSBPlayerAudioController : MonoBehaviour
	{
		public OWAudioSource _oneShotExternalSource;
		public OWAudioSource _repairToolSource;

		private void Start()
		{
			_oneShotExternalSource = CreateBaseAudio(transform, "OneShotAudio_PlayerExternal", false, 0, 1, AudioType.None, OWAudioMixer.TrackName.Player_External, false);
			_repairToolSource = CreateBaseAudio(transform, "RepairToolAudio", true, 128, 0.5f, AudioType.None, OWAudioMixer.TrackName.Player_External, false);

			var thrusterAudio = new GameObject("REMOTE_ThrusterAudio").transform;
			thrusterAudio.parent = transform;
			var jetpatchThrusterAudio = thrusterAudio.gameObject.AddComponent<QSBJetpackThrusterAudio>();
			jetpatchThrusterAudio._rotationalSource = CreateBaseAudio(thrusterAudio, "RotationalSource", false, 0, 1, AudioType.None, OWAudioMixer.TrackName.Player, false);
			jetpatchThrusterAudio._translationalSource = CreateBaseAudio(thrusterAudio, "TranslationalSource", true, 0, 0.1f, AudioType.PlayerSuitJetpackThrustTranslational_LP, OWAudioMixer.TrackName.Player_External, false);
			jetpatchThrusterAudio._underwaterSource = CreateBaseAudio(thrusterAudio, "UnderwaterSource", true, 0, 0.1f, AudioType.PlayerSuitJetpackThrustUnderwater_LP, OWAudioMixer.TrackName.Player_External, false);
			jetpatchThrusterAudio._oxygenSource = CreateBaseAudio(thrusterAudio, "OxygenPropellantSource", true, 0, 0.2f, AudioType.PlayerSuitJetpackOxygenPropellant_LP, OWAudioMixer.TrackName.Player_External, false);
			jetpatchThrusterAudio._boostSource = CreateBaseAudio(thrusterAudio, "BoosterSource", true, 0, 0.35f, AudioType.PlayerSuitJetpackBoost, OWAudioMixer.TrackName.Player_External, false);
		}

		public void PlayEquipTool()
			=> _oneShotExternalSource.PlayOneShot(AudioType.ToolTranslatorEquip);

		public void PlayUnequipTool()
			=> _oneShotExternalSource.PlayOneShot(AudioType.ToolTranslatorUnequip);

		public void PlayTurnOnFlashlight()
			=> _oneShotExternalSource.PlayOneShot(AudioType.ToolFlashlightOn);

		public void PlayTurnOffFlashlight()
			=> _oneShotExternalSource.PlayOneShot(AudioType.ToolFlashlightOff);

		private OWAudioSource CreateBaseAudio(
			Transform parent,
			string name,
			bool loop,
			int priority,
			float volume,
			AudioType audioLibraryClip,
			OWAudioMixer.TrackName track,
			bool randomize)
		{
			var go = new GameObject(name);
			go.transform.parent = parent;
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
