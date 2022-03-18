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
		if (PlayerState.IsAttached() || !this._data.sensor.inContactWithPlayer)
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
		this._effects.SetMovementStyle(GhostEffects.MovementStyle.Chase);
		this._effects.PlayGrabAnimation();
		this._effects.OnGrabComplete += this.OnGrabComplete;
		this._controller.SetLanternConcealed(false, true);
		this._controller.ChangeLanternFocus(0f, 2f);
		if (this._data.previousAction != GhostAction.Name.Chase)
		{
			this._effects.PlayVoiceAudioNear((this._data.sensor.isPlayerVisible || PlayerData.GetReducedFrights()) ? AudioType.Ghost_Grab_Shout : AudioType.Ghost_Grab_Scream, 1f);
		}
	}

	protected override void OnExitAction()
	{
		this._effects.PlayDefaultAnimation();
		this._playerIsGrabbed = false;
		this._grabAnimComplete = false;
		this._effects.OnGrabComplete -= this.OnGrabComplete;
	}

	public override bool Update_Action()
	{
		if (this._playerIsGrabbed)
		{
			return true;
		}
		if (this._data.playerLocation.distanceXZ > 1.7f)
		{
			this._controller.MoveToLocalPosition(this._data.playerLocation.localPosition, MoveType.GRAB);
		}
		this._controller.FaceLocalPosition(this._data.playerLocation.localPosition, TurnSpeed.FASTEST);
		if (this._sensors.CanGrabPlayer())
		{
			this.GrabPlayer();
		}
		return !this._grabAnimComplete;
	}

	private void GrabPlayer()
	{
		this._playerIsGrabbed = true;
		this._controller.StopMovingInstantly();
		this._controller.StopFacing();
		this._controller.SetLanternConcealed(true, false);
		this._controller.GetGrabController().GrabPlayer(1f);
	}

	private void OnGrabComplete()
	{
		this._grabAnimComplete = true;
	}

	public bool isPlayerGrabbed()
	{
		return this._playerIsGrabbed;
	}
}
