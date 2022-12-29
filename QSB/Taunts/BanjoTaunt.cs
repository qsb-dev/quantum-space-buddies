using QSB.Player;
using QSB.Taunts.ThirdPersonCamera;
using QSB.TimeSync;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.Taunts;

internal class BanjoTaunt : ITaunt
{
	public bool Loops => true;
	public TauntBodyGroup BodyGroup => TauntBodyGroup.WholeBody;
	public string StateName => "Play Banjo";
	public string TriggerName => "PlayBanjo";
	public CameraMode CameraMode => CameraMode.ThirdPerson;
	public float EnableCancelTime => 3;

	private OWAudioSource _customSignal;

	public void StartTaunt()
	{
		var audioManager = Locator.GetTravelerAudioManager();

		audioManager.SyncTravelers();

		if (_customSignal == null)
		{
			var banjoSource = audioManager._signals.First(x => x.GetOWAudioSource().audioLibraryClip == AudioType.TravelerRiebeck);
			_customSignal = Object.Instantiate(banjoSource.GetComponent<OWAudioSource>());
			_customSignal.transform.parent = Locator.GetPlayerTransform();
			_customSignal.transform.localPosition = Vector3.zero;
			Object.Destroy(_customSignal.GetComponent<AudioSignal>());
			_customSignal.SetLocalVolume(0);
			_customSignal.time = WakeUpSync.LocalInstance.TimeSinceServerStart % _customSignal.clip.length;
		}

		var closestTravelerSignal = audioManager._signals.MinBy(x => Vector3.Distance(x.transform.position, Locator.GetPlayerTransform().position));

		_customSignal.Play();
		
		var time = WakeUpSync.LocalInstance.TimeSinceServerStart % _customSignal.clip.length;

		if (time == 0)
		{
			time = WakeUpSync.LocalInstance.TimeSinceServerStart;
		}

		DebugLog.DebugWrite($"TimeSinceServerStart:{WakeUpSync.LocalInstance.TimeSinceServerStart} time:{time}");

		Delay.RunNextFrame(() =>
		{
			_customSignal.time = time;
			_customSignal.FadeIn(0.5f);
		});
	}

	public void StopTaunt()
	{
		_customSignal.FadeOut(0.5f);
	}
}
