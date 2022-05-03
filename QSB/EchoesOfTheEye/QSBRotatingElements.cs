﻿using Cysharp.Threading.Tasks;
using Mirror;
using QSB.AuthoritySync;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace QSB.EchoesOfTheEye;

internal abstract class QSBRotatingElements<T, U> : LinkedWorldObject<T, U>
	where T : MonoBehaviour
	where U : NetworkBehaviour
{
	public override void SendInitialState(uint to) { }

	protected abstract IEnumerable<SingleLightSensor> LightSensors { get; }
	private QSBLightSensor[] _qsbLightSensors;
	private int _litSensors;

	public override async UniTask Init(CancellationToken ct)
	{
		await base.Init(ct);

		await UniTask.WaitUntil(() => QSBWorldSync.AllObjectsAdded, cancellationToken: ct);
		_qsbLightSensors = LightSensors.Select(x => x.GetWorldObject<QSBLightSensor>()).ToArray();

		foreach (var lightSensor in _qsbLightSensors)
		{
			lightSensor.OnDetectLocalLight += OnDetectLocalLight;
			lightSensor.OnDetectLocalDarkness += OnDetectLocalDarkness;
		}
	}

	public override void OnRemoval()
	{
		base.OnRemoval();

		foreach (var lightSensor in _qsbLightSensors)
		{
			lightSensor.OnDetectLocalLight -= OnDetectLocalLight;
			lightSensor.OnDetectLocalDarkness -= OnDetectLocalDarkness;
		}
	}

	private void OnDetectLocalLight()
	{
		_litSensors++;
		if (_litSensors == 1)
		{
			NetworkBehaviour.netIdentity.UpdateAuthQueue(AuthQueueAction.Add);
		}
	}

	private void OnDetectLocalDarkness()
	{
		_litSensors--;
		if (_litSensors == 0)
		{
			NetworkBehaviour.netIdentity.UpdateAuthQueue(AuthQueueAction.Remove);
		}
	}

	protected override bool SpawnWithServerAuthority => false;
}