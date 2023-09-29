using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts.Messages;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Actions;

public class QSBChaseAction : QSBGhostAction
{
	private float _lastScreamTime;

	public override GhostAction.Name GetName()
	{
		return GhostAction.Name.Chase;
	}

	public override float CalculateUtility()
	{
		if (_data.interestedPlayer == null)
		{
			return -100f;
		}

		if (_data.threatAwareness < GhostData.ThreatAwareness.IntruderConfirmed || (PlayerData.GetReducedFrights() && !_data.reducedFrights_allowChase))
		{
			return -100f;
		}

		var canSeePlayer = _data.interestedPlayer.sensor.isPlayerVisible
			|| _data.interestedPlayer.sensor.isPlayerHeldLanternVisible
			|| _data.interestedPlayer.sensor.inContactWithPlayer;

		if ((_running
			&& _data.interestedPlayer.timeSincePlayerLocationKnown < 5f)
			|| (canSeePlayer && _data.interestedPlayer.playerLocation.distance < _data.interestedPlayer.playerMinLanternRange + 0.5f))
		{
			return 95f;
		}

		return -100f;
	}

	protected override void OnEnterAction()
	{
		_controller.SetLanternConcealed(false, true);
		_effects.SetMovementStyle(GhostEffects.MovementStyle.Chase);
		if (Time.time > _lastScreamTime + 10f && !PlayerData.GetReducedFrights())
		{
			_effects.PlayVoiceAudioNear(global::AudioType.Ghost_Chase, 1f);
			_lastScreamTime = Time.time;
		}
	}

	public override bool Update_Action()
	{
		if (_data.interestedPlayer.playerLocation.distance > 10f
			&& !_controller.AttachedObject.GetNodeMap().CheckLocalPointInBounds(_data.interestedPlayer.lastKnownPlayerLocation.localPosition))
		{
			return false;
		}

		if (!QSBCore.IsHost)
		{
			return true;
		}

		_controller.PathfindToLocalPosition(_data.interestedPlayer.lastKnownPlayerLocation.localPosition, MoveType.CHASE);
		_controller.FaceLocalPosition(_data.interestedPlayer.lastKnownPlayerLocation.localPosition, TurnSpeed.FAST);
		return true;
	}
}
