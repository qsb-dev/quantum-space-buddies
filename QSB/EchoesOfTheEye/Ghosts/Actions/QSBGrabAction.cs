using GhostEnums;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.EchoesOfTheEye.Ghosts.Actions;

internal class QSBGrabAction : QSBGhostAction
{
	private bool _playerIsGrabbed;

	private bool _grabAnimComplete;

	public override GhostAction.Name GetName()
	{
		return GhostAction.Name.Grab;
	}

	public override float CalculateUtility()
	{
		if (_data.interestedPlayer == null)
		{
			return -100f;
		}

		if (PlayerState.IsAttached() || !_data.interestedPlayer.sensor.inContactWithPlayer)
		{
			return -100f;
		}

		return 100f;
	}

	public override bool IsInterruptible()
	{
		return false;
	}

	protected override void OnEnterAction()
	{
		_effects.SetMovementStyle(GhostEffects.MovementStyle.Chase);
		_effects.PlayGrabAnimation();
		_effects.AttachedObject.OnGrabComplete += OnGrabComplete;
		_controller.SetLanternConcealed(false, true);
		_controller.ChangeLanternFocus(0f, 2f);
		if (_data.previousAction != GhostAction.Name.Chase)
		{
			_effects.PlayVoiceAudioNear((_data.interestedPlayer.sensor.isPlayerVisible || PlayerData.GetReducedFrights()) ? AudioType.Ghost_Grab_Shout : AudioType.Ghost_Grab_Scream, 1f);
		}
	}

	protected override void OnExitAction()
	{
		_effects.PlayDefaultAnimation();
		_playerIsGrabbed = false;
		_grabAnimComplete = false;
		_effects.AttachedObject.OnGrabComplete -= OnGrabComplete;
	}

	public override bool Update_Action()
	{
		if (_playerIsGrabbed)
		{
			return true;
		}

		if (_data.interestedPlayer.playerLocation.distanceXZ > 1.7f)
		{
			_controller.MoveToLocalPosition(_data.interestedPlayer.playerLocation.localPosition, MoveType.GRAB);
		}

		_controller.FaceLocalPosition(_data.interestedPlayer.playerLocation.localPosition, TurnSpeed.FASTEST);
		if (_sensors.CanGrabPlayer(_data.interestedPlayer))
		{
			DebugLog.DebugWrite($"Grab player {_data.interestedPlayer.player.PlayerId}!");
			GrabPlayer();
		}
		else
		{
			DebugLog.DebugWrite($"can't grab player {_data.interestedPlayer.player.PlayerId}");
		}

		return !_grabAnimComplete;
	}

	private void GrabPlayer()
	{
		_playerIsGrabbed = true;
		_controller.StopMovingInstantly();
		_controller.StopFacing();
		_controller.SetLanternConcealed(true, false);
		_controller.AttachedObject.GetGrabController().GrabPlayer(1f);
	}

	private void OnGrabComplete()
	{
		_grabAnimComplete = true;
	}

	public bool isPlayerGrabbed()
	{
		return _playerIsGrabbed;
	}
}
