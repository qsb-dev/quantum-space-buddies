using GhostEnums;
using QSB.Player;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.WorldObjects;

public class QSBGhostController : WorldObject<GhostController>, IGhostObject
{
	public override void SendInitialState(uint to)
	{

	}

	public QSBGhostEffects _effects;

	public void Initialize(GhostNode.NodeLayer layer, QSBGhostEffects effects)
	{
		_effects = effects;
		AttachedObject._nodeRoot = AttachedObject.transform.parent = AttachedObject._nodeMap.transform;
		AttachedObject._nodeLayer = layer;
		AttachedObject._grabController.Initialize(effects.AttachedObject);
		AttachedObject._lantern.SetLit(true);
		AttachedObject.MoveLanternToCarrySocket(false, 0.1f);
		AttachedObject._playerCollider = Locator.GetPlayerBody().GetComponent<CapsuleCollider>();
	}

	public void SetLanternConcealed(bool concealed, bool playAudio = true)
	{
		if (playAudio && AttachedObject._lantern.IsConcealed() != concealed)
		{
			_effects.PlayLanternAudio(concealed ? global::AudioType.Artifact_Conceal : global::AudioType.Artifact_Unconceal);
		}

		AttachedObject._lantern.SetConcealed(concealed);
		if (concealed)
		{
			AttachedObject._lantern.SetFocus(0f);
			AttachedObject._updateLantern = false;
		}
	}

	public void ChangeLanternFocus(float focus, float focusRate = 2f)
	{
		if (focus > 0f)
		{
			AttachedObject._lantern.SetConcealed(false);
		}

		if (focus > AttachedObject._targetLanternFocus)
		{
			_effects.PlayLanternAudio(global::AudioType.Artifact_Focus);
		}
		else if (focus < AttachedObject._targetLanternFocus)
		{
			_effects.PlayLanternAudio(global::AudioType.Artifact_Unfocus);
		}

		AttachedObject._updateLantern = true;
		AttachedObject._targetLanternFocus = focus;
		AttachedObject._lanternFocusRate = focusRate;
	}

	public void FacePlayer(PlayerInfo player, TurnSpeed turnSpeed)
	{
		AttachedObject._facingState = GhostController.FacingState.FaceTransform;
		AttachedObject._faceTransform = player.Camera.transform;
		AttachedObject._targetDegreesPerSecond = GhostConstants.GetTurnSpeed(turnSpeed);
		AttachedObject._angularAcceleration = GhostConstants.GetTurnAcceleration(turnSpeed);
	}
}
