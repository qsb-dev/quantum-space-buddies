using QSB.Localization;
using UnityEngine;

namespace QSB.ShipSync;

public class ShipCustomAttach : MonoBehaviour
{
	private readonly ScreenPrompt _attachPrompt = new(
		InputLibrary.interactSecondary,
		InputLibrary.interact,
		QSBLocalization.Current.AttachToShip + "   <CMD>",
		ScreenPrompt.MultiCommandType.HOLD_ONE_AND_PRESS_2ND
	);

	private readonly ScreenPrompt _detachPrompt = new(
		InputLibrary.cancel,
		QSBLocalization.Current.DetachFromShip + "   <CMD>"
	);

	private PlayerAttachPoint _playerAttachPoint;

	/// <summary>
	/// uses a static field instead of a persistent condition cuz those are synced
	/// </summary>
	private static bool _tutorialPrompt = true;

	private void Awake()
	{
		Locator.GetPromptManager().AddScreenPrompt(_attachPrompt, _tutorialPrompt ? PromptPosition.Center : PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_detachPrompt, PromptPosition.UpperRight);

		_playerAttachPoint = gameObject.AddComponent<PlayerAttachPoint>();
		_playerAttachPoint._lockPlayerTurning = false;
		_playerAttachPoint._matchRotation = false;
		_playerAttachPoint._centerCamera = false;
	}

	private void OnDestroy()
	{
		if (Locator.GetPromptManager())
		{
			Locator.GetPromptManager().RemoveScreenPrompt(_attachPrompt);
			Locator.GetPromptManager().RemoveScreenPrompt(_detachPrompt);
		}
	}

	private void Update()
	{
		_attachPrompt.SetVisibility(false);
		_detachPrompt.SetVisibility(false);
		// dont show prompt if paused or something
		if (!OWInput.IsInputMode(InputMode.Character))
		{
			return;
		}

		var attachedToUs = _playerAttachPoint.enabled;
		_detachPrompt.SetVisibility(attachedToUs);
		if (attachedToUs && OWInput.IsNewlyPressed(InputLibrary.cancel, InputMode.Character))
		{
			_playerAttachPoint.DetachPlayer();
			ShipManager.Instance.CockpitController._shipAudioController.PlayUnbuckle();
		}

		if (!PlayerState.IsInsideShip())
		{
			return;
		}

		if (!attachedToUs)
		{
			if (PlayerState.IsAttached())
			{
				return;
			}

			if (Locator.GetPlayerController() && !Locator.GetPlayerController().IsGrounded())
			{
				return;
			}
		}

		_attachPrompt.SetVisibility(!attachedToUs);
		if (!attachedToUs &&
			OWInput.IsPressed(InputLibrary.interactSecondary, InputMode.Character) &&
			OWInput.IsNewlyPressed(InputLibrary.interact, InputMode.Character))
		{
			transform.position = Locator.GetPlayerTransform().position;
			_playerAttachPoint.AttachPlayer();
			ShipManager.Instance.CockpitController._shipAudioController.PlayBuckle();

			if (_tutorialPrompt)
			{
				_tutorialPrompt = false;
				Locator.GetPromptManager().RemoveScreenPrompt(_attachPrompt);
				Locator.GetPromptManager().AddScreenPrompt(_attachPrompt, PromptPosition.UpperRight);
			}
		}
	}
}
