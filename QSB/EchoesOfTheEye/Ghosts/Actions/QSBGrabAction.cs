using GhostEnums;
using QSB.Player;
using QSB.Utility;

namespace QSB.EchoesOfTheEye.Ghosts.Actions;

internal class QSBGrabAction : QSBGhostAction
{
	private bool _playerIsGrabbed;
	private bool _grabAnimComplete;
	private PlayerInfo _grabbedPlayer;

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

		if (_playerIsGrabbed || !_data.interestedPlayer.sensor.inContactWithPlayer)
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
		_grabbedPlayer = null;
	}

	public override bool Update_Action()
	{
		if (_grabbedPlayer != null && !_grabbedPlayer.InDreamWorld)
		{
			return false;
		}

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
			GrabPlayer(_data.interestedPlayer);
		}
		else
		{
			DebugLog.DebugWrite($"can't grab player!" +
				$"\r\nIn Grab Distance:{_data.interestedPlayer.playerLocation.distanceXZ < 2f + _sensors.AttachedObject._grabDistanceBuff}" +
				$"\r\nIn Grab Angle:{_data.interestedPlayer.playerLocation.degreesToPositionXZ < 20f + _sensors.AttachedObject._grabAngleBuff}" +
				$"\r\nIn Grab Window:{_sensors.AttachedObject._animator.GetFloat("GrabWindow") > 0.5f}");
		}

		return !_grabAnimComplete;
	}

	private void GrabPlayer(GhostPlayer player)
	{
		_grabbedPlayer = player.player;
		_playerIsGrabbed = true;
		_controller.StopMovingInstantly();
		_controller.StopFacing();
		_controller.SetLanternConcealed(true, false);
		_controller.GetGrabController().GrabPlayer(1f, player);
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
