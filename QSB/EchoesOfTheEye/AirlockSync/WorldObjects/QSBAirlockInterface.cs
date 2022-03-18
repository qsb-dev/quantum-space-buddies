using QSB.EchoesOfTheEye.AirlockSync.VariableSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.EchoesOfTheEye.AirlockSync.WorldObjects;

internal class QSBAirlockInterface : QSBRotatingElements<AirlockInterface, AirlockVariableSyncer>
{
	protected override IEnumerable<SingleLightSensor> LightSensors => AttachedObject._lightSensors;

	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.AirlockPrefab;
}
