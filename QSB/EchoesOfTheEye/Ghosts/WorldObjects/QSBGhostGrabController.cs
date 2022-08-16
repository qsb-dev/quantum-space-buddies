using QSB.EchoesOfTheEye.Ghosts.Messages;
using QSB.Messaging;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.WorldObjects;

public class QSBGhostGrabController : WorldObject<GhostGrabController>
{
	public override void SendInitialState(uint to)
	{
		// todo SendInitialState
	}

	public void GrabPlayer(float speed, GhostPlayer player, bool remote = false)
	{
		if (!remote)
		{
			this.SendMessage(new GrabRemotePlayerMessage(speed, player.player.PlayerId));
		}

		var isLocalPlayer = player.player.IsLocalPlayer;

		if (isLocalPlayer)
		{
			AttachedObject.enabled = true;
			AttachedObject._snappingNeck = !player.player.AssignedSimulationLantern.AttachedObject.GetLanternController().IsHeldByPlayer();
			AttachedObject._holdingInPlace = true;
			AttachedObject._grabMoveComplete = false;
			AttachedObject._extinguishStarted = false;
			AttachedObject._attachPoint.transform.parent = AttachedObject._origParent;
			AttachedObject._attachPoint.transform.position = Locator.GetPlayerTransform().position;
			AttachedObject._attachPoint.transform.rotation = Locator.GetPlayerTransform().rotation;
			AttachedObject._startLocalPos = AttachedObject._attachPoint.transform.localPosition;
			AttachedObject._startLocalRot = AttachedObject._attachPoint.transform.localRotation;
			AttachedObject._playerAttached = true;
			AttachedObject._attachPoint.AttachPlayer();
			GlobalMessenger.FireEvent("PlayerGrabbedByGhost");
			OWInput.ChangeInputMode(InputMode.None);
			ReticleController.Hide();
			Locator.GetDreamWorldController().SetActiveGhostGrabController(AttachedObject);
			AttachedObject._grabStartTime = Time.time;
			AttachedObject._grabMoveDuration = Mathf.Min(Vector3.Distance(AttachedObject._startLocalPos, AttachedObject._holdPoint.localPosition) / speed, 2f);
			RumbleManager.PlayGhostGrab();
			Achievement_Ghost.GotCaughtByGhost();
		}

		var effects = AttachedObject._effects.GetWorldObject<QSBGhostEffects>();

		if (QSBCore.IsHost)
		{
			if (AttachedObject._snappingNeck)
			{
				effects.PlaySnapNeckAnimation();
			}
			else
			{
				// todo : make this track grab counts, so we can use the fast animation
				effects.PlayBlowOutLanternAnimation();
			}
		}

		effects.AttachedObject.PlayGrabAudio(AudioType.Ghost_Grab_Contact);
	}
}
