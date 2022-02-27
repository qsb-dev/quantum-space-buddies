using QSB.Utility;
using UnityEngine;

namespace QSB.Inputs
{
	public class QSBInputManager : MonoBehaviour, IAddComponentOnStart
	{
		// TODO : finish instruments - disabled for 0.7.0 release
		/*
		public static event Action ChertTaunt;
		public static event Action EskerTaunt;
		public static event Action RiebeckTaunt;
		public static event Action GabbroTaunt;
		public static event Action FeldsparTaunt;
		public static event Action ExitTaunt;
	
		public void Update()
		{
			if (Input.GetKey(KeyCode.T))
			{
				// Listed order is from sun to dark bramble
				if (Input.GetKeyDown(KeyCode.Alpha1))
				{
					ChertTaunt?.Invoke();
				}
				else if (Input.GetKeyDown(KeyCode.Alpha2))
				{
					EskerTaunt?.Invoke();
				}
				else if (Input.GetKeyDown(KeyCode.Alpha5))
				{
					RiebeckTaunt?.Invoke();
				}
				else if (Input.GetKeyDown(KeyCode.Alpha4))
				{
					GabbroTaunt?.Invoke();
				}
				else if (Input.GetKeyDown(KeyCode.Alpha3))
				{
					FeldsparTaunt?.Invoke();
				}
			}
	
			if (OWInput.GetValue(InputLibrary.moveXZ, InputMode.None) != Vector2.zero
				|| OWInput.GetValue(InputLibrary.jump, InputMode.None) != 0f)
			{
				ExitTaunt?.Invoke();
			}
		}
		*/

		public static QSBInputManager Instance { get; private set; }

		public void Start()
			=> Instance = this;

		public bool InputsEnabled { get; private set; } = true;

		public void SetInputsEnabled(bool enabled) => InputsEnabled = enabled;
	}
}