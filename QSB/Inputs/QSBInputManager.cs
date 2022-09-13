using QSB.Utility;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace QSB.Inputs;

public class QSBInputManager : MonoBehaviour, IAddComponentOnStart
{
	public static event Action ThumbsUpTaunt;
	public static event Action DefaultDanceTaunt;
	public static event Action ExitTaunt;

	public void Update()
	{
		if (!QSBCore.IsInMultiplayer || !QSBSceneManager.IsInUniverse)
		{
			return;
		}

		if (Keyboard.current[Key.T].isPressed)
		{
			if (Keyboard.current[Key.Digit1].wasPressedThisFrame)
			{
				ThumbsUpTaunt?.Invoke();
			}

			if (Keyboard.current[Key.Digit2].wasPressedThisFrame)
			{
				DefaultDanceTaunt?.Invoke();
			}
		}

		if (OWInput.GetAxisValue(InputLibrary.moveXZ, InputMode.None) != Vector2.zero
			|| OWInput.IsPressed(InputLibrary.jump, InputMode.None))
		{
			ExitTaunt?.Invoke();
		}
	}

	public static QSBInputManager Instance { get; private set; }

	public void Start()
		=> Instance = this;

	public bool InputsEnabled { get; private set; } = true;

	public void SetInputsEnabled(bool enabled) => InputsEnabled = enabled;
}