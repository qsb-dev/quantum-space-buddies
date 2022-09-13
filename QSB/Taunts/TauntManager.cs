using QSB.Inputs;
using QSB.Player;
using QSB.Taunts.ThirdPersonCamera;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.Taunts;

internal class TauntManager : MonoBehaviour, IAddComponentOnStart
{
	private ThumbsUpTaunt _thumpsUpTaunt = new();
	private DefaultDanceTaunt _defaultDanceTaunt = new();

	public static ITaunt CurrentTaunt;

	private bool _setUpStateEvents;

	private void Start()
	{
		QSBInputManager.ThumbsUpTaunt += () => StartTaunt(_thumpsUpTaunt);
		QSBInputManager.DefaultDanceTaunt += () => StartTaunt(_defaultDanceTaunt);
		QSBInputManager.ExitTaunt += StopTaunt;

		QSBSceneManager.OnUniverseSceneLoaded += (OWScene oldScene, OWScene newScene) => _setUpStateEvents = false;
	}

	private bool CheckCanTaunt()
	{
		var currentlyMoving = OWInput.GetAxisValue(InputLibrary.moveXZ, InputMode.None) != Vector2.zero
			|| OWInput.IsPressed(InputLibrary.jump, InputMode.None);
		var inHazardVolume = Locator.GetPlayerDetector().GetComponent<HazardDetector>()._activeVolumes.Any();
		var grounded = Locator.GetPlayerController().IsGrounded();
		var inZeroG = Locator.GetPlayerController()._isZeroGMovementEnabled;

		return !currentlyMoving && !inHazardVolume && grounded && !inZeroG;
	}

	private void StartTaunt(ITaunt taunt)
	{
		if (!CheckCanTaunt())
		{
			return;
		}

		DebugLog.DebugWrite($"Start taunt {taunt.GetType().Name}");

		CurrentTaunt = taunt;
		if (taunt.CameraMode == CameraMode.ThirdPerson)
		{
			CameraManager.Instance.SwitchTo3rdPerson();
		}

		var animator = QSBPlayerManager.LocalPlayer.AnimationSync.VisibleAnimator;
		animator.SetTrigger(taunt.TriggerName);

		if (!_setUpStateEvents)
		{
			var animatorStateEvents = animator.GetBehaviours<AnimatorStateEvents>();
			foreach (var item in animatorStateEvents)
			{
				item.OnExitState += (AnimatorStateInfo stateInfo, int layerIndex) => StopTaunt();
			}

			_setUpStateEvents = true;
		}
	}

	public static void StopTaunt()
	{
		DebugLog.DebugWrite($"StopTaunt");
		CurrentTaunt = null;
		CameraManager.Instance.SwitchTo1stPerson();
	}
}
