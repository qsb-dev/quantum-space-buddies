using GhostEnums;
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
		if (PlayerState.IsAttached() || !_data.sensor.inContactWithPlayer)
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
		_effects.AttachedObject.SetMovementStyle(GhostEffects.MovementStyle.Chase);
		_effects.AttachedObject.PlayGrabAnimation();
		_effects.AttachedObject.OnGrabComplete += OnGrabComplete;
		_controller.SetLanternConcealed(false, true);
		_controller.ChangeLanternFocus(0f, 2f);
		if (_data.previousAction != GhostAction.Name.Chase)
		{
			_effects.AttachedObject.PlayVoiceAudioNear((_data.sensor.isPlayerVisible || PlayerData.GetReducedFrights()) ? AudioType.Ghost_Grab_Shout : AudioType.Ghost_Grab_Scream, 1f);
		}
	}

	protected override void OnExitAction()
	{
		_effects.AttachedObject.PlayDefaultAnimation();
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
		if (_data.playerLocation.distanceXZ > 1.7f)
		{
			_controller.MoveToLocalPosition(_data.playerLocation.localPosition, MoveType.GRAB);
		}
		_controller.FaceLocalPosition(_data.playerLocation.localPosition, TurnSpeed.FASTEST);
		if (_sensors.CanGrabPlayer())
		{
			GrabPlayer();
		}
		return !_grabAnimComplete;
	}

	private void GrabPlayer()
	{
		_playerIsGrabbed = true;
		_controller.StopMovingInstantly();
		_controller.StopFacing();
		_controller.SetLanternConcealed(true, false);
		_controller.GetGrabController().GrabPlayer(1f);
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
