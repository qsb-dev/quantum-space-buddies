using QSB.PlayerBodySetup.Remote;
using QSB.Utility;
using UnityEngine;

namespace QSB.Audio;

[UsedInUnityProject]
public class QSBPlayerAudioController : MonoBehaviour
{
	public OWAudioSource _oneShotExternalSource;
	public OWAudioSource _repairToolSource;
	public OWAudioSource _damageAudioSource;

	private AudioManager _audioManager;
	private float _playWearHelmetTime;

	public void Start()
	{
		_audioManager = Locator.GetAudioManager();

		// TODO: This should be done in the Unity project
		var damageAudio = new GameObject("DamageAudioSource");
		damageAudio.SetActive(false);
		damageAudio.transform.SetParent(transform, false);
		damageAudio.transform.localPosition = Vector3.zero;
		_damageAudioSource = damageAudio.AddComponent<OWAudioSource>();
		_damageAudioSource._audioSource = damageAudio.GetAddComponent<AudioSource>();
		_damageAudioSource.SetTrack(_repairToolSource.GetTrack());
		_damageAudioSource.spatialBlend = 1f;
		_damageAudioSource.gameObject.GetAddComponent<QSBDopplerFixer>();
		damageAudio.SetActive(true);
	}

	private void Update()
	{
		if (Time.time > this._playWearHelmetTime)
		{
			enabled = false;
			PlayOneShot(global::AudioType.PlayerSuitWearHelmet);
		}
	}

	public void PlayEquipTool()
		=> _oneShotExternalSource?.PlayOneShot(AudioType.ToolTranslatorEquip);

	public void PlayUnequipTool()
		=> _oneShotExternalSource?.PlayOneShot(AudioType.ToolTranslatorUnequip);

	public void PlayTurnOnFlashlight()
		=> _oneShotExternalSource?.PlayOneShot(AudioType.ToolFlashlightOn);

	public void PlayTurnOffFlashlight()
		=> _oneShotExternalSource?.PlayOneShot(AudioType.ToolFlashlightOff);

	public void PlayWearSuit()
		=> PlayOneShot(AudioType.PlayerSuitWearSuit);

	public void PlayRemoveSuit()
		=> PlayOneShot(AudioType.PlayerSuitRemoveSuit);

	public void PlayRemoveHelmet()
	{
		enabled = false;
		PlayOneShot(AudioType.PlayerSuitRemoveHelmet);
	}

	public void PlayWearHelmet()
	{
		enabled = true;
		_playWearHelmetTime = Time.time + 0.4f;
	}

	public void PlayOneShot(AudioType audioType, float pitch = 1f, float volume = 1f)
	{
		if (_oneShotExternalSource)
		{
			_oneShotExternalSource.pitch = pitch;
			_oneShotExternalSource.PlayOneShot(audioType, volume);
		}
	}

	public void PlayFootstep(AudioType audioType, float pitch) => 
		PlayOneShot(audioType, pitch, 0.7f);

	public void OnJump(float pitch) =>
		PlayOneShot(AudioType.MovementJump, pitch, 0.7f);

	private void StartHazardDamage(HazardVolume.HazardType latestHazardType)
	{
		var type = AudioType.EnterVolumeDamageHeat_LP;
		if (latestHazardType == HazardVolume.HazardType.DARKMATTER)
		{
			type = AudioType.EnterVolumeDamageGhostfire_LP;
		}
		else if (latestHazardType == HazardVolume.HazardType.FIRE)
		{
			type = AudioType.EnterVolumeDamageFire_LP;
		}

		_damageAudioSource.clip = _audioManager.GetSingleAudioClip(type, true);
		_damageAudioSource.Stop();
		_damageAudioSource.FadeIn(0.2f, true, true, 1f);
	}

	private void EndHazardDamage()
	{
		if (_damageAudioSource.isPlaying)
		{
			_damageAudioSource.FadeOut(0.5f, OWAudioSource.FadeOutCompleteAction.STOP, 0f);
		}
	}

	public void SetHazardDamage(HazardVolume.HazardType latestHazardType)
	{
		if (latestHazardType == HazardVolume.HazardType.NONE)
		{
			EndHazardDamage();
		}
		else
		{
			StartHazardDamage(latestHazardType);
		}
	}
}