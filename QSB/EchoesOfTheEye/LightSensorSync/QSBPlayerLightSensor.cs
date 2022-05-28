using QSB.Player;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.EchoesOfTheEye.LightSensorSync;

/// <summary>
/// stores a bit of extra data needed for player light sensor sync
/// </summary>
[RequireComponent(typeof(SingleLightSensor))]
public class QSBPlayerLightSensor : MonoBehaviour
{
	private SingleLightSensor _lightSensor;

	internal bool _locallyIlluminated;
	internal readonly List<uint> _illuminatedBy = new();

	private void Awake()
	{
		_lightSensor = GetComponent<SingleLightSensor>();

		RequestInitialStatesMessage.SendInitialState += SendInitialState;
		QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;
	}

	private void OnDestroy()
	{
		RequestInitialStatesMessage.SendInitialState -= SendInitialState;
		QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;
	}

	private void SendInitialState(uint to)
	{
		// todo send the messages
	}

	private void OnPlayerLeave(PlayerInfo player) => SetIlluminated(player.PlayerId, false);

	public void SetIlluminated(uint playerId, bool locallyIlluminated)
	{
		var illuminated = _illuminatedBy.Count > 0;
		if (locallyIlluminated)
		{
			_illuminatedBy.SafeAdd(playerId);
		}
		else
		{
			_illuminatedBy.QuickRemove(playerId);
		}

		if (!illuminated && _illuminatedBy.Count > 0)
		{
			_lightSensor._illuminated = true;
			_lightSensor.OnDetectLight.Invoke();
		}
		else if (illuminated && _illuminatedBy.Count == 0)
		{
			_lightSensor._illuminated = false;
			_lightSensor.OnDetectDarkness.Invoke();
		}
	}
}
