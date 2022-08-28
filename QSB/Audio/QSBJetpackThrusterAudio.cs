using QSB.Player;
using QSB.Utility;
using UnityEngine;

namespace QSB.Audio;

[UsedInUnityProject]
internal class QSBJetpackThrusterAudio : QSBThrusterAudio
{
	public OWAudioSource _underwaterSource;
	public OWAudioSource _oxygenSource;
	public OWAudioSource _boostSource;

	private PlayerInfo _attachedPlayer;
	private bool _wasBoosting;

	// Taken from Player_Body settings
	private const float maxTranslationalThrust = 6f;

	public void Init(PlayerInfo player)
	{
		_attachedPlayer = player;
		enabled = true;
	}

	private void Update()
	{
		if(_attachedPlayer == null)
		{
			enabled = false;
			return;
		}

		var acc = _attachedPlayer.JetpackAcceleration.AccelerationVariableSyncer.Value;
		var thrustFraction = acc.magnitude / maxTranslationalThrust;

		// TODO: Sync
		var usingBooster = false;
		var usingOxygen = false;
		var underwater = false;

		float targetVolume = usingBooster ? 0f : thrustFraction;
		float targetPan = -acc.x / maxTranslationalThrust * 0.4f;
		UpdateTranslationalSource(_translationalSource, targetVolume, targetPan, !underwater && !usingOxygen);
		UpdateTranslationalSource(_underwaterSource, targetVolume, targetPan, underwater);
		UpdateTranslationalSource(_oxygenSource, targetVolume, targetPan, !underwater && usingOxygen);

		if (!_wasBoosting && usingBooster)
		{
			_boostSource.FadeIn(0.3f, false, false, 1f);
		}
		else if (_wasBoosting && !usingBooster)
		{
			_boostSource.FadeOut(0.3f, OWAudioSource.FadeOutCompleteAction.STOP, 0f);
		}

		_wasBoosting = usingBooster;
	}

	private void UpdateTranslationalSource(OWAudioSource source, float targetVolume, float targetPan, bool active)
	{
		if (!active)
		{
			targetVolume = 0f;
			targetPan = 0f;
		}
		if (!source.isPlaying && targetVolume > 0f)
		{
			source.SetLocalVolume(0f);
			source.Play();
		}
		else if (source.isPlaying && source.volume <= 0f)
		{
			source.Stop();
		}
		source.SetLocalVolume(Mathf.MoveTowards(source.GetLocalVolume(), targetVolume, 5f * Time.deltaTime));
		source.panStereo = Mathf.MoveTowards(source.panStereo, targetPan, 5f * Time.deltaTime);
	}
}