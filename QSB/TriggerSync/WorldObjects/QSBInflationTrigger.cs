using Cysharp.Threading.Tasks;
using QSB.Player;
using QSB.Utility;
using System.Threading;
using UnityEngine;

namespace QSB.TriggerSync.WorldObjects;

public class QSBInflationTrigger : QSBTrigger<CosmicInflationController>
{
	protected override string CompareTag => "PlayerCameraDetector";

	public override async UniTask Init(CancellationToken ct)
	{
		await base.Init(ct);
		AttachedObject.OnEntry -= TriggerOwner.OnEnterFogSphere;
		AttachedObject.OnExit -= OnExitEvent;
	}

	protected override void OnEnter(PlayerInfo player)
	{
		if (TriggerOwner._state != CosmicInflationController.State.ReadyToCollapse)
		{
			return;
		}

		if (player.IsLocalPlayer)
		{
			AttachedObject.OnEntry -= OnEnterEvent;

			AttachedObject.SetTriggerActivation(false);
			TriggerOwner._probeDestroyTrigger.SetTriggerActivation(false);

			var repelVolume = (WhiteHoleFluidVolume)TriggerOwner._repelVolume;
			repelVolume._flowSpeed = -repelVolume._flowSpeed;
			repelVolume._massiveFlowSpeed = -repelVolume._massiveFlowSpeed;
			repelVolume.SetVolumeActivation(true);
			QSBPlayerManager.HideAllPlayers();

			ReticleController.Hide();
			Locator.GetFlashlight().TurnOff(false);
			Locator.GetPromptManager().SetPromptsVisible(false);
			OWInput.ChangeInputMode(InputMode.None);
		}
		else
		{
			player.SetVisible(false, .3f);
		}

		if (Occupants.Count == QSBPlayerManager.PlayerList.Count)
		{
			StartCollapse();
		}
	}

	protected override void OnExit(PlayerInfo player) =>
		// wait 1 frame for player to be removed
		Delay.RunNextFrame(() =>
		{
			if (QSBCore.IsInMultiplayer && Occupants.Count == QSBPlayerManager.PlayerList.Count)
			{
				StartCollapse();
			}
		});

	private void StartCollapse()
	{
		var repelVolume = (WhiteHoleFluidVolume)TriggerOwner._repelVolume;
		repelVolume.SetVolumeActivation(false);

		TriggerOwner._state = CosmicInflationController.State.Collapsing;
		TriggerOwner._stateChangeTime = Time.time;
		TriggerOwner._collapseStartPos = TriggerOwner._possibilitySphereRoot.localPosition;
		AttachedObject.SetTriggerActivation(false);
		TriggerOwner._inflationLight.FadeTo(1f, 1f);
		TriggerOwner._possibilitySphereController.OnCollapse();
		if (TriggerOwner._campsiteController.GetUseAltPostCollapseSocket())
		{
			TriggerOwner._playerPostCollapseSocket = TriggerOwner._altPlayerPostCollapseSocket;
			TriggerOwner._altTravelerToHidePostCollapse.SetActive(false);
		}

		Locator.GetPlayerBody().SetPosition(TriggerOwner._playerPostCollapseSocket.position);
		Locator.GetPlayerBody().SetRotation(TriggerOwner._playerPostCollapseSocket.rotation);
		Locator.GetPlayerBody().SetVelocity(-TriggerOwner._playerPostCollapseSocket.forward);
		Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().LockOn(TriggerOwner._possibilitySphereRoot, 2f);
		foreach (var particles in TriggerOwner._smokeSphereParticles)
		{
			particles.Stop();
		}
	}
}
