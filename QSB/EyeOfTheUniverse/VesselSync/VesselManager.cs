using QSB.Messaging;
using QSB.Player;
using QSB.Player.Messages;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.VesselSync
{
	internal class VesselManager : WorldObjectManager
	{
		public static VesselManager Instance { get; private set; }

		private readonly List<PlayerInfo> _playersInCage = new();
		private VesselWarpController _warpController;

		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		public override void Awake()
		{
			base.Awake();
			Instance = this;
		}

		protected override void RebuildWorldObjects(OWScene scene)
		{
			if (_warpController != null)
			{
				_warpController._cageTrigger.OnEntry -= OnEntry;
				_warpController._cageTrigger.OnExit -= OnExit;
			}

			_warpController = FindObjectOfType<VesselWarpController>();
			_warpController._cageTrigger.OnExit -= _warpController.OnExitCageTrigger;

			_warpController._cageTrigger.OnEntry += OnEntry;
			_warpController._cageTrigger.OnExit += OnExit;
		}

		private static void OnEntry(GameObject hitObj)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				new EnterLeaveMessage(EnterLeaveType.EnterVesselCage).Send();
			}
		}

		private static void OnExit(GameObject hitObj)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				new EnterLeaveMessage(EnterLeaveType.ExitVesselCage).Send();
			}
		}


		public void Enter(PlayerInfo player)
		{
			_playersInCage.Add(player);
		}

		public void Exit(PlayerInfo player)
		{
			_playersInCage.Remove(player);

			if (_playersInCage.Count == 0 && _warpController._hasPower)
			{
				var obj = _warpController;
				obj._cageClosed = true;
				obj._cageAnimator.TranslateToLocalPosition(new Vector3(0f, -8.1f, 0f), 5f);
				obj._cageAnimator.RotateToLocalEulerAngles(new Vector3(0f, 180f, 0f), 5f);
				obj._cageAnimator.OnTranslationComplete -= obj.OnCageAnimationComplete;
				obj._cageAnimator.OnTranslationComplete += obj.OnCageAnimationComplete;
				obj._cageLoopingAudio.FadeIn(1f);
			}
		}
	}
}
