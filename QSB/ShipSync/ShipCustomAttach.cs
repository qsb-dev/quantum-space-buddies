using QSB.Player;
using UnityEngine;

namespace QSB.ShipSync
{
	public class ShipCustomAttach : MonoBehaviour
	{
		private static readonly ScreenPrompt _attachPrompt = new(InputLibrary.interactSecondary, InputLibrary.interact,
			"Attach to ship" + "   <CMD>", ScreenPrompt.MultiCommandType.HOLD_ONE_AND_PRESS_2ND);
		private static readonly ScreenPrompt _detachPrompt = new(InputLibrary.cancel, "Detach from ship" + "   <CMD>");
		private PlayerAttachPoint _playerAttachPoint;

		private void Awake()
		{
			Locator.GetPromptManager().AddScreenPrompt(_attachPrompt, PromptPosition.UpperRight);
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
				Locator.GetPromptManager().RemoveScreenPrompt(_attachPrompt, PromptPosition.UpperRight);
				Locator.GetPromptManager().RemoveScreenPrompt(_detachPrompt, PromptPosition.UpperRight);
			}
		}

		private void Update()
		{
			_attachPrompt.SetVisibility(false);
			_detachPrompt.SetVisibility(false);
			if (!PlayerState.IsInsideShip())
			{
				return;
			}

			var attachedToUs = PlayerAttachWatcher.Current == _playerAttachPoint;
			if (!attachedToUs)
			{
				if (_playerAttachPoint.enabled)
				{
					// attached to us, then attached to something else
					_playerAttachPoint.enabled = false;
				}

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
			_detachPrompt.SetVisibility(attachedToUs);

			if (!attachedToUs &&
				OWInput.IsPressed(InputLibrary.interactSecondary, InputMode.Character) &&
				OWInput.IsNewlyPressed(InputLibrary.interact, InputMode.Character))
			{
				transform.position = Locator.GetPlayerTransform().position;
				_playerAttachPoint.AttachPlayer();
				ShipManager.Instance.CockpitController._shipAudioController.PlayBuckle();
			}
			else if (attachedToUs && OWInput.IsNewlyPressed(InputLibrary.cancel, InputMode.Character))
			{
				_playerAttachPoint.DetachPlayer();
				ShipManager.Instance.CockpitController._shipAudioController.PlayUnbuckle();
			}
		}
	}
}
