using QSB.Utility;
using UnityEngine;

namespace QSB.DeathSync
{
	internal class ShipRecoveryPoint : MonoBehaviour
	{
		private MultipleInteractionVolume _interactVolume;
		private PlayerResources _playerResources;
		private VisorEffectController _playerVisor;
		private PlayerAudioController _playerAudioController;
		private bool _recovering;
		private int _refillIndex;
		private int _respawnIndex;

		private void Awake()
		{
			DebugLog.DebugWrite($"AWAKE");
			_interactVolume = this.GetRequiredComponent<MultipleInteractionVolume>();
			//_interactVolume.OnPressInteract += OnPressInteract;
			_interactVolume.OnGainFocus += OnGainFocus;

			_refillIndex = _interactVolume.AddInteraction(InputLibrary.interact, InputMode.Character, UITextType.None, true, true);
			_respawnIndex = _interactVolume.AddInteraction(InputLibrary.interactSecondary, InputMode.Character, UITextType.YouEscapeOnRingWorld, false, true);
		}

		private void OnGainFocus()
		{
			DebugLog.DebugWrite($"OnGainFocus");

			if (_playerResources == null)
			{
				_playerResources = Locator.GetPlayerTransform().GetComponent<PlayerResources>();
			}

			_interactVolume.EnableSingleInteraction(RespawnManager.Instance.RespawnNeeded, _respawnIndex);

			var needsHealing = _playerResources.GetHealthFraction() != 1f;
			var needsRefueling = _playerResources.GetFuelFraction() != 1f;

			UITextType uitextType;
			if (needsHealing && needsRefueling)
			{
				uitextType = UITextType.RefillPrompt_0;
				_interactVolume.SetKeyCommandVisible(true, _refillIndex);
			}
			else if (needsHealing)
			{
				uitextType = UITextType.RefillPrompt_2;
				_interactVolume.SetKeyCommandVisible(true, _refillIndex);
			}
			else if (needsRefueling)
			{
				uitextType = UITextType.RefillPrompt_4;
				_interactVolume.SetKeyCommandVisible(true, _refillIndex);
			}
			else
			{
				uitextType = UITextType.RefillPrompt_7;
				_interactVolume.SetKeyCommandVisible(false, _refillIndex);
			}

			if (uitextType != UITextType.None)
			{
				_interactVolume.ChangePrompt(uitextType, _refillIndex);
			}
		}
	}
}
