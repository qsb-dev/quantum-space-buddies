﻿using QSB.EyeOfTheUniverse.EyeStateSync.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.Player.Messages;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.MaskSync
{
	internal class MaskManager : WorldObjectManager
	{
		public static MaskManager Instance { get; private set; }

		private readonly List<PlayerInfo> _playersInZone = new();
		private MaskZoneController _controller;
		private bool _flickering;
		private float _flickerOutTime;

		public override WorldObjectType WorldObjectType => WorldObjectType.Eye;

		public override void Awake()
		{
			base.Awake();
			Instance = this;

			QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;
		}

		private void OnPlayerLeave(PlayerInfo player)
		{
			if (_playersInZone.Contains(player))
			{
				Exit(player);
			}
		}

		protected override void RebuildWorldObjects(OWScene scene)
		{
			_playersInZone.Clear();

			if (_controller != null)
			{
				_controller._maskZoneTrigger.OnEntry -= OnEntry;
				_controller._maskZoneTrigger.OnExit -= OnExit;
			}

			_controller = QSBWorldSync.GetUnityObjects<MaskZoneController>().First();
			_controller._maskZoneTrigger.OnEntry -= _controller.OnEnterMaskZone;
			_controller._maskZoneTrigger.OnExit -= _controller.OnExitMaskZone;

			_controller._maskZoneTrigger.OnEntry += OnEntry;
			_controller._maskZoneTrigger.OnExit += OnExit;
		}

		private static void OnEntry(GameObject hitObj)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				new EnterLeaveMessage(EnterLeaveType.EnterMaskZone).Send();
			}
		}

		private static void OnExit(GameObject hitObj)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				new EnterLeaveMessage(EnterLeaveType.ExitMaskZone).Send();
			}
		}

		public void Enter(PlayerInfo player)
		{
			if (_playersInZone.Count == 0)
			{
				_controller._whiteSphere.SetActive(true);
				_controller._groundSignal.SetSignalActivation(false);
				_controller._skySignal.SetSignalActivation(true);
				_controller._skeletonTower.SetIsQuantum(_controller._hasPlayerLookedAtSky);
				_controller.enabled = true;
			}

			_playersInZone.Add(player);
		}

		public void Exit(PlayerInfo player)
		{
			_playersInZone.Remove(player);

			if (_playersInZone.Count == 0 && !_controller._shuttle.HasLaunched())
			{
				_controller._whiteSphere.SetActive(false);
				_controller._skeletonTower.SetIsQuantum(false);
				_controller._groundSignal.SetSignalActivation(true);
				_controller._skySignal.SetSignalActivation(false);
				_controller.enabled = false;
			}
		}

		public void FlickerOutShuttle()
		{
			FlickerMessage.IgnoreNextMessage = true;
			GlobalMessenger<float, float>.FireEvent("FlickerOffAndOn", 0.5f, 0.5f);
			_flickerOutTime = Time.time + 0.5f;
			_flickering = true;
		}

		private void Update()
		{
			if (_flickering && Time.time > _flickerOutTime)
			{
				var controller = QSBWorldSync.GetUnityObjects<EyeShuttleController>().First();
				controller._shuttleObject.SetActive(false);
				_flickering = false;
				_flickerOutTime = 0f;
			}
		}
	}
}