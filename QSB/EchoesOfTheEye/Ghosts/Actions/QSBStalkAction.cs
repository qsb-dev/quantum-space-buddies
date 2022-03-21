using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts;
using QSB.Utility;
using UnityEngine;

public class QSBStalkAction : QSBGhostAction
{
	private bool _wasPlayerLanternConcealed;
	private bool _isFocusingLight;
	private bool _shouldFocusLightOnPlayer;
	private float _changeFocusTime;

	public override GhostAction.Name GetName()
		=> GhostAction.Name.Stalk;

	public override float CalculateUtility()
	{
		if (_data.threatAwareness < GhostData.ThreatAwareness.IntruderConfirmed)
		{
			return -100f;
		}

		if ((_running && _data.timeSincePlayerLocationKnown < 4f) || _data.isPlayerLocationKnown)
		{
			return 85f;
		}

		return -100f;
	}

	protected override void OnEnterAction()
	{
		var flag = Locator.GetDreamWorldController().GetPlayerLantern().GetLanternController().IsConcealed();
		_wasPlayerLanternConcealed = flag;
		_isFocusingLight = flag;
		_shouldFocusLightOnPlayer = flag;
		_changeFocusTime = 0f;
		_controller.ChangeLanternFocus(_isFocusingLight ? 1f : 0f, 2f);
		_controller.SetLanternConcealed(!_isFocusingLight, true);
		_controller.FaceVelocity();
		_effects.AttachedObject.SetMovementStyle(GhostEffects.MovementStyle.Stalk);
		_effects.AttachedObject.PlayVoiceAudioNear(_data.fastStalkUnlocked ? AudioType.Ghost_Stalk_Fast : AudioType.Ghost_Stalk, 1f);
	}

	public override bool Update_Action()
	{
		if (!_data.fastStalkUnlocked && _data.illuminatedByPlayerMeter > 4f)
		{
			DebugLog.DebugWrite($"{_brain.AttachedObject._name} Fast stalk unlocked.");
			_data.fastStalkUnlocked = true;
			_effects.AttachedObject.PlayVoiceAudioNear(AudioType.Ghost_Stalk_Fast, 1f);
		}

		return true;
	}

	public override void FixedUpdate_Action()
	{
		var stalkSpeed = GhostConstants.GetMoveSpeed(MoveType.SEARCH);
		if (_data.fastStalkUnlocked)
		{
			stalkSpeed += 1.5f;
		}

		if (_controller.GetNodeMap().CheckLocalPointInBounds(_data.lastKnownPlayerLocation.localPosition))
		{
			_controller.PathfindToLocalPosition(_data.lastKnownPlayerLocation.localPosition, stalkSpeed, GhostConstants.GetMoveAcceleration(MoveType.SEARCH));
		}

		_controller.FaceLocalPosition(_data.lastKnownPlayerLocation.localPosition, TurnSpeed.MEDIUM);

		var isPlayerLanternConcealed = Locator.GetDreamWorldController().GetPlayerLantern().GetLanternController().IsConcealed();
		var sawPlayerLanternConceal = !_wasPlayerLanternConcealed
			&& isPlayerLanternConcealed
			&& _data.wasPlayerLocationKnown;

		_wasPlayerLanternConcealed = isPlayerLanternConcealed;
		if (sawPlayerLanternConceal && !_shouldFocusLightOnPlayer)
		{
			_shouldFocusLightOnPlayer = true;
			_changeFocusTime = Time.time + 1f;
		}
		else if (_data.sensor.isPlayerHeldLanternVisible && _shouldFocusLightOnPlayer)
		{
			_shouldFocusLightOnPlayer = false;
			_changeFocusTime = Time.time + 1f;
		}

		if (_isFocusingLight != _shouldFocusLightOnPlayer && Time.time > _changeFocusTime)
		{
			if (_shouldFocusLightOnPlayer)
			{
				DebugLog.DebugWrite($"{_brain.AttachedObject._name} : Un-concealing lantern and focusing on player.");
				_controller.SetLanternConcealed(false, true);
				_controller.ChangeLanternFocus(1f, 2f);
			}
			else
			{
				DebugLog.DebugWrite($"{_brain.AttachedObject._name} : Concealing lantern.");
				_controller.SetLanternConcealed(true, true);
			}

			_isFocusingLight = _shouldFocusLightOnPlayer;
		}
	}
}
