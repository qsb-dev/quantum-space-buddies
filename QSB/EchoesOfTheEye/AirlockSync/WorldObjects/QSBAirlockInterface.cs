using QSB.EchoesOfTheEye.AirlockSync.VariableSync;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.EchoesOfTheEye.AirlockSync.WorldObjects;

public class QSBAirlockInterface : QSBRotatingElements<AirlockInterface, AirlockVariableSyncer>
{
	protected override IEnumerable<SingleLightSensor> LightSensors => AttachedObject._lightSensors;

	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.AirlockPrefab;

	public override string ReturnLabel()
	{
		var baseString = $"{this}{Environment.NewLine}CurrentRotation:{AttachedObject._currentRotation}";
		foreach (var element in AttachedObject._rotatingElements)
		{
			baseString += $"{Environment.NewLine}localRotation:{element.localRotation}";
		}
		return baseString;
	}
}
