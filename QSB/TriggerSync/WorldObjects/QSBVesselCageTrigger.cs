﻿using QSB.Player;
using UnityEngine;

namespace QSB.TriggerSync.WorldObjects
{
	public class QSBVesselCageTrigger : QSBTrigger<VesselWarpController>
	{
		public override void Init()
		{
			base.Init();
			AttachedObject.OnExit -= TriggerOwner.OnExitCageTrigger;
		}

		protected override void OnExit(PlayerInfo player)
		{
			if (Occupants.Count == 0 && TriggerOwner._hasPower)
			{
				TriggerOwner._cageClosed = true;
				TriggerOwner._cageAnimator.TranslateToLocalPosition(new Vector3(0f, -8.1f, 0f), 5f);
				TriggerOwner._cageAnimator.RotateToLocalEulerAngles(new Vector3(0f, 180f, 0f), 5f);
				TriggerOwner._cageAnimator.OnTranslationComplete -= TriggerOwner.OnCageAnimationComplete;
				TriggerOwner._cageAnimator.OnTranslationComplete += TriggerOwner.OnCageAnimationComplete;
				TriggerOwner._cageLoopingAudio.FadeIn(1f);
			}
		}
	}
}
