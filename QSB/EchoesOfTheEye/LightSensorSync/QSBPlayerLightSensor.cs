using System;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.EchoesOfTheEye.LightSensorSync;

[RequireComponent(typeof(LightSensor))]
public class QSBPlayerLightSensor : MonoBehaviour
{
	private LightSensor _lightSensor;
	private void Awake() => _lightSensor = GetComponent<LightSensor>();

	internal bool _locallyIlluminated;

	public Action OnDetectLocalLight;
	public Action OnDetectLocalDarkness;

	internal readonly List<uint> _illuminatedBy = new();
}
