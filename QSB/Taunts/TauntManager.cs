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
	private BanjoTaunt _banjoTaunt = new();

	public static ITaunt CurrentTaunt;
	public static float TauntStartTime;

	private bool _setUpStateEvents;

	private void Start()
	{
		QSBInputManager.ThumbsUpTaunt += () => StartTaunt(_thumpsUpTaunt);
		QSBInputManager.DefaultDanceTaunt += () => StartTaunt(_defaultDanceTaunt);
		QSBInputManager.BanjoTaunt += () => StartTaunt(_banjoTaunt);
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
		TauntStartTime = Time.time;
		DebugLog.DebugWrite($"TauntStartTime is {Time.time}. Enabling at {TauntStartTime + CurrentTaunt.EnableCancelTime}");
		if (taunt.CameraMode == CameraMode.ThirdPerson)
		{
			CameraManager.Instance.SwitchTo3rdPerson();
		}

		var animator = QSBPlayerManager.LocalPlayer.AnimationSync.VisibleAnimator;
		animator.SetTrigger(taunt.TriggerName);

		CurrentTaunt.StartTaunt();

		if (!_setUpStateEvents)
		{
			var animatorStateEvents = animator.GetBehaviours<AnimatorStateEvents>();
			foreach (var item in animatorStateEvents)
			{
				item.OnExitState += (AnimatorStateInfo stateInfo, int layerIndex) =>
				{
					var layerName = CurrentTaunt.BodyGroup switch
					{
						TauntBodyGroup.WholeBody => "Base Layer",
						TauntBodyGroup.RightArm => "Additive Right Arm Layer",
						_ => throw new System.NotImplementedException()
					};

					var name = $"{layerName}.{CurrentTaunt.StateName}";
					DebugLog.DebugWrite($"Matches {name} : {stateInfo.IsName(name)}");

					if (stateInfo.IsName(name))
					{
						StopTaunt();
					}
				};
			}

			_setUpStateEvents = true;
		}
	}

	public static void StopTaunt()
	{
		if (CurrentTaunt == null
			|| CurrentTaunt.EnableCancelTime == -1
			|| TauntStartTime + CurrentTaunt.EnableCancelTime > Time.time)
		{
			return;
		}

		DebugLog.DebugWrite($"StopTaunt");
		
		if (CurrentTaunt.CameraMode == CameraMode.ThirdPerson)
		{
			CameraManager.Instance.SwitchTo1stPerson();
		}

		var animator = QSBPlayerManager.LocalPlayer.AnimationSync.VisibleAnimator;
		animator.SetTrigger("Land");

		CurrentTaunt.StopTaunt();

		CurrentTaunt = null;
		TauntStartTime = -1;
	}
}
