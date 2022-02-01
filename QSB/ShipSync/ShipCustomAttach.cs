using UnityEngine;

namespace QSB.ShipSync
{
	public class ShipCustomAttach : MonoBehaviour
	{
		private const string ATTACH_TEXT = "Attach to ship";
		private const string DETACH_TEXT = "Detach from ship";
		private static readonly ScreenPrompt _prompt = new(InputLibrary.interactSecondary, ATTACH_TEXT + "   <CMD>");
		private PlayerAttachPoint _playerAttachPoint;

		private void Awake()
		{
			Locator.GetPromptManager().AddScreenPrompt(_prompt, PromptPosition.UpperRight);

			_playerAttachPoint = gameObject.AddComponent<PlayerAttachPoint>();
			_playerAttachPoint._lockPlayerTurning = false;
			_playerAttachPoint._matchRotation = false;
			_playerAttachPoint._centerCamera = false;
		}

		private void OnDestroy() =>
			Locator.GetPromptManager().RemoveScreenPrompt(_prompt, PromptPosition.UpperRight);

		private void Update()
		{
			_prompt.SetVisibility(false);
			if (!PlayerState.IsInsideShip())
			{
				return;
			}

			var attachedToUs = _playerAttachPoint.enabled;
			if (PlayerState.IsAttached() && !attachedToUs)
			{
				return;
			}

			_prompt.SetVisibility(true);
			if (OWInput.IsNewlyPressed(InputLibrary.interactSecondary, InputMode.Character))
			{
				if (!attachedToUs)
				{
					transform.position = Locator.GetPlayerTransform().position;
					_playerAttachPoint.AttachPlayer();
					_prompt.SetText(DETACH_TEXT + "   <CMD>");
				}
				else
				{
					_playerAttachPoint.DetachPlayer();
					_prompt.SetText(ATTACH_TEXT + "   <CMD>");
				}
			}
		}
	}
}
