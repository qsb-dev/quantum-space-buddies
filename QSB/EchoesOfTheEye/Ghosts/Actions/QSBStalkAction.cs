using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts;
using UnityEngine;

public class QSBStalkAction : QSBGhostAction
{
	private bool _wasPlayerLanternConcealed;

	private bool _isFocusingLight;

	private bool _shouldFocusLightOnPlayer;

	private float _changeFocusTime;

	public override GhostAction.Name GetName()
	{
		return GhostAction.Name.Stalk;
	}

	public override float CalculateUtility()
	{
		if (this._data.threatAwareness < GhostData.ThreatAwareness.IntruderConfirmed)
		{
			return -100f;
		}
		if ((this._running && this._data.timeSincePlayerLocationKnown < 4f) || this._data.isPlayerLocationKnown)
		{
			return 85f;
		}
		return -100f;
	}

	protected override void OnEnterAction()
	{
		bool flag = Locator.GetDreamWorldController().GetPlayerLantern().GetLanternController().IsConcealed();
		this._wasPlayerLanternConcealed = flag;
		this._isFocusingLight = flag;
		this._shouldFocusLightOnPlayer = flag;
		this._changeFocusTime = 0f;
		this._controller.ChangeLanternFocus(this._isFocusingLight ? 1f : 0f, 2f);
		this._controller.SetLanternConcealed(!this._isFocusingLight, true);
		this._controller.FaceVelocity();
		this._effects.SetMovementStyle(GhostEffects.MovementStyle.Stalk);
		this._effects.PlayVoiceAudioNear(this._data.fastStalkUnlocked ? global::AudioType.Ghost_Stalk_Fast : global::AudioType.Ghost_Stalk, 1f);
	}

	public override bool Update_Action()
	{
		if (!this._data.fastStalkUnlocked && this._data.illuminatedByPlayerMeter > 4f)
		{
			this._data.fastStalkUnlocked = true;
			this._effects.PlayVoiceAudioNear(global::AudioType.Ghost_Stalk_Fast, 1f);
		}
		return true;
	}

	public override void FixedUpdate_Action()
	{
		float num = GhostConstants.GetMoveSpeed(MoveType.SEARCH);
		if (this._data.fastStalkUnlocked)
		{
			num += 1.5f;
		}
		if (this._controller.GetNodeMap().CheckLocalPointInBounds(this._data.lastKnownPlayerLocation.localPosition))
		{
			this._controller.PathfindToLocalPosition(this._data.lastKnownPlayerLocation.localPosition, num, GhostConstants.GetMoveAcceleration(MoveType.SEARCH));
		}
		this._controller.FaceLocalPosition(this._data.lastKnownPlayerLocation.localPosition, TurnSpeed.MEDIUM);
		bool flag = Locator.GetDreamWorldController().GetPlayerLantern().GetLanternController().IsConcealed();
		bool flag2 = !this._wasPlayerLanternConcealed && flag && this._data.wasPlayerLocationKnown;
		this._wasPlayerLanternConcealed = flag;
		if (flag2 && !this._shouldFocusLightOnPlayer)
		{
			this._shouldFocusLightOnPlayer = true;
			this._changeFocusTime = Time.time + 1f;
		}
		else if (this._data.sensor.isPlayerHeldLanternVisible && this._shouldFocusLightOnPlayer)
		{
			this._shouldFocusLightOnPlayer = false;
			this._changeFocusTime = Time.time + 1f;
		}
		if (this._isFocusingLight != this._shouldFocusLightOnPlayer && Time.time > this._changeFocusTime)
		{
			if (this._shouldFocusLightOnPlayer)
			{
				this._controller.SetLanternConcealed(false, true);
				this._controller.ChangeLanternFocus(1f, 2f);
			}
			else
			{
				this._controller.SetLanternConcealed(true, true);
			}
			this._isFocusingLight = this._shouldFocusLightOnPlayer;
		}
	}
}
