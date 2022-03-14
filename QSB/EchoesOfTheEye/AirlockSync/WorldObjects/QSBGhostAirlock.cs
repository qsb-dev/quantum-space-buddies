using QSB.EchoesOfTheEye.AirlockSync.VariableSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.EchoesOfTheEye.AirlockSync.WorldObjects;

internal class QSBGhostAirlock : QSBRotatingElements<GhostAirlock, AirlockVariableSyncer>
{
	protected override IEnumerable<SingleLightSensor> LightSensors => AttachedObject._interface._lightSensors;

	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.AirlockPrefab;
}
