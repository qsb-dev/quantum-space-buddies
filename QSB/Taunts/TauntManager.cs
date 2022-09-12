using QSB.Inputs;
using QSB.Taunts.ThirdPersonCamera;
using QSB.Utility;
using UnityEngine;

namespace QSB.Taunts;

internal class TauntManager : MonoBehaviour, IAddComponentOnStart
{
	private ThumbsUpTaunt _thumpsUpTaunt = new();

	private ITaunt _currentTaunt;

	private void Start()
	{
		QSBInputManager.ThumbsUpTaunt += () => StartTaunt(_thumpsUpTaunt);
		QSBInputManager.ExitTaunt += StopTaunt;
	}

	private void StartTaunt(ITaunt taunt)
	{
		DebugLog.DebugWrite($"Start taunt {taunt.GetType().Name}");

		_currentTaunt = taunt;
		if (taunt.CameraMode == CameraMode.ThirdPerson)
		{
			CameraManager.Instance.SwitchTo3rdPerson();
		}
	}

	private void StopTaunt()
	{
		DebugLog.DebugWrite($"StopTaunt");
		CameraManager.Instance.SwitchTo1stPerson();
	}
}
