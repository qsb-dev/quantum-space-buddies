using UnityEngine;

namespace QSB.EchoesOfTheEye.AlarmTotemSync;

/// <summary>
/// copied and modified from base game
/// </summary>
public class QSBAlarmSequenceController : MonoBehaviour
{
	private const int CHIME_COUNT = 3;

	[SerializeField]
	private float _wakeDuration = 5f;

	[SerializeField]
	private float _recoveryDuration = 2f;

	[Header("Audio")]
	[SerializeField]
	private float _audioLingerTime = 3f;

	[SerializeField]
	private AnimationCurve _volumeCurve;

	[Header("Pulse")]
	[SerializeField]
	private float _pulseAttackLength = 0.2f;

	[SerializeField]
	private float _pulseHoldLength = 0.1f;

	[SerializeField]
	private float _pulseCooldownLength = 1f;

	private DreamWorldController _dreamWorldController;

	private AlarmBell _activeBell;

	private int _alarmCounter;

	private float _wakeFraction;

	private float _targetPulse;

	private float _pulse;

	private bool _playing;

	private bool _animationStarted;

	private bool _stopRequested;

	private float _stopTime;

	private int _chimeIndex;

	private float _lastChimeTime;

	private float _chimeInterval;

	private void Awake()
	{
		// Locator.RegisterAlarmSequenceController(this);
		GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
	}

	private void Start()
	{
		_dreamWorldController = Locator.GetDreamWorldController();
		enabled = false;
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
	}

	public void RegisterDreamEyeMaskController(DreamEyeMaskController dreamEyeMaskController) { }

	public float GetPulseIntensity() => _pulse;

	public bool IsAlarmWakingPlayer() => _alarmCounter > 0 && !PlayerState.IsResurrected();

	public void IncreaseAlarmCounter()
	{
		if (!_dreamWorldController.IsInDream() || _dreamWorldController.IsExitingDream())
		{
			return;
		}

		_alarmCounter++;
		if (_alarmCounter == 1)
		{
			PlayChimes();
		}

		enabled = true;
	}

	public void DecreaseAlarmCounter()
	{
		if (!_dreamWorldController.IsInDream() || _dreamWorldController.IsExitingDream())
		{
			return;
		}

		_alarmCounter--;
		if (_alarmCounter < 0)
		{
			_alarmCounter = 0;
			Debug.LogError("Something went wrong, alarm counter should never drop below zero!");
		}
	}

	private void PlayChimes()
	{
		if (Locator.GetDreamWorldController().GetDreamCampfire() != null)
		{
			_activeBell = Locator.GetDreamWorldController().GetDreamCampfire().GetAlarmBell();
		}

		_playing = true;
		_chimeInterval = 1f;
		_lastChimeTime = 0f;
		_stopRequested = false;
	}

	private void StopChimes()
	{
		_playing = false;
		_stopRequested = false;
		_animationStarted = false;
		if (_activeBell != null)
		{
			_activeBell.StopAnimation();
			_activeBell = null;
		}
	}

	private void Update()
	{
		if (_dreamWorldController.IsInDream() && !_dreamWorldController.IsExitingDream())
		{
			UpdateWakeFraction();
		}

		if (_playing)
		{
			UpdateChimes();
		}

		UpdatePulse();
		if (!_playing && _alarmCounter == 0 && _pulse <= 0.01f)
		{
			_pulse = 0f;
			_targetPulse = 0f;
			enabled = false;
		}
	}

	private void UpdateWakeFraction()
	{
		if (_alarmCounter > 0)
		{
			_wakeFraction = Mathf.MoveTowards(_wakeFraction, 1f, Time.deltaTime / _wakeDuration);
			if (_wakeFraction >= 1f && !PlayerState.IsResurrected())
			{
				_dreamWorldController.ExitDreamWorld(DreamWakeType.Alarm);
			}
		}
		else if (_wakeFraction > 0f)
		{
			_wakeFraction = Mathf.MoveTowards(_wakeFraction, 0f, Time.deltaTime / _recoveryDuration);
			if (_wakeFraction <= 0f)
			{
				StopChimes();
			}
		}
	}

	private void UpdateChimes()
	{
		if (Time.time > _lastChimeTime + _chimeInterval)
		{
			if (!PlayerState.IsResurrected())
			{
				PlaySingleChime();
			}

			_targetPulse = 1f;
			_lastChimeTime = Time.time;
			_chimeInterval = Mathf.Max(_chimeInterval - 0.08f, 0.4f);
			_chimeIndex++;
			_chimeIndex = _chimeIndex >= CHIME_COUNT ? 0 : _chimeIndex;
		}

		if (_stopRequested && Time.time > _stopTime)
		{
			StopChimes();
		}
	}

	private void UpdatePulse()
	{
		if (Time.time > _lastChimeTime + _pulseAttackLength + _pulseHoldLength)
		{
			var num = _pulseCooldownLength * _chimeInterval;
			_targetPulse = Mathf.MoveTowards(_targetPulse, 0f, 1f / num * Time.deltaTime);
			_pulse = _targetPulse;
			return;
		}

		_pulse = Mathf.MoveTowards(_pulse, _targetPulse, 1f / _pulseAttackLength * Time.deltaTime);
	}

	private void PlaySingleChime()
	{
		if (_activeBell != null)
		{
			_activeBell.PlaySingleChime(_chimeIndex);
			if (!_animationStarted && !_dreamWorldController.IsInDream())
			{
				_activeBell.PlayAnimation();
				_animationStarted = true;
			}
		}

		if (_dreamWorldController.IsInDream() && !_dreamWorldController.IsExitingDream())
		{
			Locator.GetDreamWorldAudioController().PlaySingleAlarmChime(_chimeIndex, _volumeCurve.Evaluate(_wakeFraction));
		}
	}

	private void OnExitDreamWorld()
	{
		if (_playing)
		{
			_stopRequested = true;
			_stopTime = Time.time + _audioLingerTime;
		}

		_alarmCounter = 0;
		_wakeFraction = 0f;
	}
}
