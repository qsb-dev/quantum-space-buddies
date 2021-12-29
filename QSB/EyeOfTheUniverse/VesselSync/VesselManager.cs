using QSB.EyeOfTheUniverse.VesselSync.WorldObjects;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.VesselSync
{
	internal class VesselManager : WorldObjectManager
	{
		public static VesselManager Instance { get; private set; }

		private List<PlayerInfo> _playersInCage = new();
		private QSBVesselWarpController _warpController;

		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		public override void Awake()
		{
			base.Awake();
			Instance = this;
		}

		protected override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBVesselWarpController, VesselWarpController>();
			_warpController = QSBWorldSync.GetWorldObjects<QSBVesselWarpController>().First();
			_warpController.AttachedObject._cageTrigger.OnExit -= _warpController.AttachedObject.OnExitCageTrigger;
		}

		public void Enter(PlayerInfo player)
		{
			_playersInCage.Add(player);
		}

		public void Exit(PlayerInfo player)
		{
			_playersInCage.Remove(player);

			if (_playersInCage.Count == 0 && _warpController.AttachedObject._hasPower)
			{
				var obj = _warpController.AttachedObject;
				obj._cageClosed = true;
				obj._cageAnimator.TranslateToLocalPosition(new Vector3(0f, -8.1f, 0f), 5f);
				obj._cageAnimator.RotateToLocalEulerAngles(new Vector3(0f, 180f, 0f), 5f);
				obj._cageAnimator.OnTranslationComplete -= obj.OnCageAnimationComplete;
				obj._cageAnimator.OnTranslationComplete += obj.OnCageAnimationComplete;
				obj._cageLoopingAudio.FadeIn(1f, false, false, 1f);
			}
		}
	}
}
