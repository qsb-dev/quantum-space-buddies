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

				DebugLog.DebugWrite("pause, disable input, wait for other players to enter");
				
				ReticleController.Hide();
				Locator.GetFlashlight().TurnOff(false);
				Locator.GetPromptManager().SetPromptsVisible(false);
				OWInput.ChangeInputMode(InputMode.None);
				
				OWTime.SetTimeScale(0);
			}
		}

		public void Enter(PlayerInfo player)
		{
			_playersInFog.Add(player);

			if (player != QSBPlayerManager.LocalPlayer)
			{
				DebugLog.DebugWrite($"fade out player {player.PlayerId}");
				player.DitheringAnimator.SetVisible(false, 3);
			}

			if (_playersInFog.Count == QSBPlayerManager.PlayerList.Count)
			{
				StartCollapse();
			}
		}

		private void StartCollapse()
		{
			DebugLog.DebugWrite("unpause, fade in everyone, fog sphere collapse");
			
			OWTime.SetTimeScale(1);
			QSBPlayerManager.ShowAllPlayers();

			_controller._state = CosmicInflationController.State.Collapsing;
			_controller._stateChangeTime = Time.time;
			_controller._collapseStartPos = _controller._possibilitySphereRoot.localPosition;
			_controller._smokeSphereTrigger.SetTriggerActivation(false);
			_controller._inflationLight.FadeTo(1f, 1f);
			_controller._possibilitySphereController.OnCollapse();
			if (_controller._campsiteController.GetUseAltPostCollapseSocket())
			{
				_controller._playerPostCollapseSocket = _controller._altPlayerPostCollapseSocket;
				_controller._altTravelerToHidePostCollapse.SetActive(false);
			}

			Locator.GetPlayerBody().SetPosition(_controller._playerPostCollapseSocket.position);
			Locator.GetPlayerBody().SetRotation(_controller._playerPostCollapseSocket.rotation);
			Locator.GetPlayerBody().SetVelocity(-_controller._playerPostCollapseSocket.forward);
			Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().LockOn(_controller._possibilitySphereRoot, 2f);
			foreach (var particles in _controller._smokeSphereParticles)
			{
				particles.Stop();
			}
		}
	}
}
