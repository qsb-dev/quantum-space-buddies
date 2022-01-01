using QSB.Messaging;
using QSB.Player;
using QSB.Player.Messages;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.CosmicInflation
{
	internal class InflationManager : WorldObjectManager
	{
		public static InflationManager Instance { get; private set; }

		private readonly List<PlayerInfo> _playersInFog = new();
		private CosmicInflationController _controller;

		public override WorldObjectType WorldObjectType => WorldObjectType.Eye;

		public override void Awake()
		{
			base.Awake();
			Instance = this;

			QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;
		}

		private void OnPlayerLeave(uint id)
		{
			_playersInFog.Remove(QSBPlayerManager.GetPlayer(id));

			// count - 1 because this happens right before player is actually removed
			if (_playersInFog.Count == QSBPlayerManager.PlayerList.Count - 1)
			{
				StartCollapse();
			}
		}

		protected override void RebuildWorldObjects(OWScene scene)
		{
			_playersInFog.Clear();

			if (_controller != null)
			{
				_controller._smokeSphereTrigger.OnEntry -= OnEntry;
			}

			_controller = QSBWorldSync.GetUnityObjects<CosmicInflationController>().First();
			_controller._smokeSphereTrigger.OnEntry -= _controller.OnEnterFogSphere;

			_controller._smokeSphereTrigger.OnEntry += OnEntry;
		}

		private void OnEntry(GameObject hitObj)
		{
			if (hitObj.CompareTag("PlayerCameraDetector") && _controller._state == CosmicInflationController.State.ReadyToCollapse)
			{
				_controller._smokeSphereTrigger.SetTriggerActivation(false);
				_controller._probeDestroyTrigger.SetTriggerActivation(false);
				new EnterLeaveMessage(EnterLeaveType.EnterCosmicFog).Send();

				// the pausing and stuff happens here
				DebugLog.DebugWrite("TODO: pause and wait for other players to enter");
			}
		}


		public void Enter(PlayerInfo player)
		{
			_playersInFog.Add(player);

			if (_playersInFog.Count == QSBPlayerManager.PlayerList.Count)
			{
				StartCollapse();
			}
		}

		private void StartCollapse()
		{
			// the actual collapsing happens here
			DebugLog.DebugWrite("TODO: fog sphere collapse");
		}
	}
}
