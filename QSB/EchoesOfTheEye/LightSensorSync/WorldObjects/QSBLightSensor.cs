﻿using Cysharp.Threading.Tasks;
using QSB.OwnershipSync;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Threading;

/*
 * For those who come here,
 * leave while you still can.
 */

namespace QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;

public class QSBLightSensor : OwnedWorldObject<SingleLightSensor>
{
	internal bool _locallyIlluminated;

	public Action OnDetectLocalLight;
	public Action OnDetectLocalDarkness;

	public override bool CanOwn => AttachedObject.enabled;

	public override async UniTask Init(CancellationToken ct)
	{
		await base.Init(ct);

		// do this stuff here instead of Start, since world objects won't be ready by that point
		Delay.RunWhen(() => QSBWorldSync.AllObjectsReady, () =>
		{
			if (AttachedObject._sector != null)
			{
				if (AttachedObject._startIlluminated)
				{
					// dont need to do _illuminated cuz _startIlluminated means it already sets the thing and fires the event in Start
					_locallyIlluminated = true;
					OnDetectLocalLight?.Invoke();
				}
			}
		});
	}
}
