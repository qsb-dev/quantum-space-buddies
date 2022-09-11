using QSB.Utility;
using System;
using UnityEngine;

namespace QSB.Inputs;

public class QSBInputManager : MonoBehaviour, IAddComponentOnStart
{
	public static event Action ThumbsUpTaunt;
	public static event Action ExitTaunt;

	public void Update()
	{
		if (Input.GetKey(KeyCode.T))
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				ThumbsUpTaunt?.Invoke();
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