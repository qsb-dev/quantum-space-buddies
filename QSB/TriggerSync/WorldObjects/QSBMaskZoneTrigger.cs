﻿using Cysharp.Threading.Tasks;
using QSB.Player;
using System.Threading;

namespace QSB.TriggerSync.WorldObjects;

public class QSBMaskZoneTrigger : QSBTrigger<MaskZoneController>
{
	public override async UniTask Init(CancellationToken ct)
	{
		await base.Init(ct);
		AttachedObject.OnEntry -= TriggerOwner.OnEnterMaskZone;
		AttachedObject.OnExit -= TriggerOwner.OnExitMaskZone;
	}

	protected override void OnEnter(PlayerInfo player)
	{
		if (Occupants.Count == 1 && !TriggerOwner._shuttle.HasLaunched())
		{
			TriggerOwner._whiteSphere.SetActive(true);
			TriggerOwner._groundSignal.SetSignalActivation(false);
			TriggerOwner._skySignal.SetSignalActivation(true);
			TriggerOwner._skeletonTower.SetIsQuantum(TriggerOwner._hasPlayerLookedAtSky);
			TriggerOwner.enabled = true;
		}
	}

	protected override void OnExit(PlayerInfo player)
	{
		if (Occupants.Count == 0 && !TriggerOwner._shuttle.HasLaunched())
		{
			TriggerOwner._whiteSphere.SetActive(false);
			TriggerOwner._skeletonTower.SetIsQuantum(false);
			TriggerOwner._groundSignal.SetSignalActivation(true);
			TriggerOwner._skySignal.SetSignalActivation(false);
			TriggerOwner.enabled = false;
		}
	}
}