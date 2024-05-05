using QSB.Utility;
using UnityEngine;

namespace QSB.Inputs;

public class QSBInputManager : MonoBehaviour, IAddComponentOnStart
{
	public static QSBInputManager Instance { get; private set; }

	public void Start()
		=> Instance = this;

	public bool InputsEnabled { get; private set; } = true;

	public void SetInputsEnabled(bool enabled) => InputsEnabled = enabled;
}