﻿using QSB.EchoesOfTheEye.LightSensorSync.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * For those who come here,
 * leave while you still can.
 */

namespace QSB.EchoesOfTheEye.LightSensorSync;

/// <summary>
/// stores a bit of extra data needed for player light sensor sync
/// </summary>
[RequireComponent(typeof(SingleLightSensor))]
public class QSBPlayerLightSensor : MonoBehaviour
{
	private SingleLightSensor _lightSensor;
	[NonSerialized]
	public uint PlayerId;

	internal bool _locallyIlluminated;
	internal readonly List<uint> _illuminatedBy = new();

	private void Awake()
	{
		_lightSensor = GetComponent<SingleLightSensor>();
		PlayerId = QSBPlayerManager.PlayerList.First(x => x.LightSensor == _lightSensor).PlayerId;

		RequestInitialStatesMessage.SendInitialState += SendInitialState;
		QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;

		if (_lightSensor._sector != null)
		{
			if (_lightSensor._startIlluminated)
			{
				_locallyIlluminated = true;
				// do it manually so as not to invoke OnDetectLight, since that's handled by Start
				// BUG? should add all players to the list, not just local
				_illuminatedBy.SafeAdd(PlayerId);
			}
		}
	}

	private void OnDestroy()
	{
		RequestInitialStatesMessage.SendInitialState -= SendInitialState;
		QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;
	}

	private void SendInitialState(uint to)
	{
		new PlayerIlluminatedByMessage(PlayerId, _illuminatedBy.ToArray()) { To = to }.Send();
		if (_lightSensor._illuminatingDreamLanternList != null)
		{
			new PlayerIlluminatingLanternsMessage(PlayerId, _lightSensor._illuminatingDreamLanternList) { To = to }.Send();
		}
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
