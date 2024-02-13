using QSB.EchoesOfTheEye.LightSensorSync.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync.Messages;
using System.Linq;
using UnityEngine;

/*
 * For those who come here,
 * leave while you still can.
 */

namespace QSB.EchoesOfTheEye.LightSensorSync;

/// <summary>
/// only purpose is to handle initial state sync.
///
/// we don't have to worry about start illuminated or sectors.
/// ownership is always given to local player light sensor.
///
/// 2 uses:
/// - AlarmTotem.CheckPlayerVisible
/// - GhostSensors.FixedUpdate_Sensors
/// </summary>
[RequireComponent(typeof(SingleLightSensor))]
public class QSBPlayerLightSensor : MonoBehaviour
{
	private SingleLightSensor _lightSensor;
	private uint _playerId;

	private void Awake()
	{
		_lightSensor = GetComponent<SingleLightSensor>();
		_playerId = QSBPlayerManager.PlayerList.First(x => x.LightSensor == _lightSensor).PlayerId;

		RequestInitialStatesMessage.SendInitialState += SendInitialState;
	}

	private void OnDestroy() =>
		RequestInitialStatesMessage.SendInitialState -= SendInitialState;

	private void SendInitialState(uint to)
	{
		new PlayerSetIlluminatedMessage(_playerId, _lightSensor._illuminated) { To = to }.Send();
		if (_lightSensor._illuminatingDreamLanternList != null)
		{
			new PlayerIlluminatingLanternsMessage(_playerId, _lightSensor._illuminatingDreamLanternList) { To = to }.Send();
		}
	}
}
