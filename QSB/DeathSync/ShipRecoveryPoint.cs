using QSB.Utility;
using UnityEngine;

namespace QSB.DeathSync
{
	internal class ShipRecoveryPoint : MonoBehaviour
	{
		private MultipleInteractionVolume _interactVolume;
		private PlayerResources _playerResources;
		private PlayerAudioController _playerAudioController;
		private bool _recovering;
		private int _refillIndex;
		private int _respawnIndex;
		private bool _wearingSuit;

		private void Awake()
		{
			DebugLog.DebugWrite($"AWAKE");
			_interactVolume = this.GetRequiredComponent<MultipleInteractionVolume>();
			_interactVolume.OnPressInteract += OnPressInteract;
			_interactVolume.OnGainFocus += OnGainFocus;

			var respawnPlayerText = UIHelper.AddToUITable("Respawn Player");

			_refillIndex = _interactVolume.AddInteraction(InputLibrary.interact, InputMode.Character, UITextType.None, true, true);
			_respawnIndex = _interactVolume.AddInteraction(InputLibrary.interactSecondary, InputMode.Character, (UITextType)respawnPlayerText, true, true);

			GlobalMessenger.AddListener("SuitUp", new Callback(OnSuitUp));
			GlobalMessenger.AddListener("RemoveSuit", new Callback(OnRemoveSuit));
		}

		private void Start()
		{
			_playerResources = Locator.GetPlayerTransform().GetComponent<PlayerResources>();
			_playerAudioController = Locator.GetPlayerAudioController();
		}

		private void OnDestroy()
		{
			_interactVolume.OnPressInteract -= OnPressInteract;
			_interactVolume.OnGainFocus -= OnGainFocus;
			GlobalMessenger.RemoveListener("SuitUp", new Callback(OnSuitUp));
			GlobalMessenger.RemoveListener("RemoveSuit", new Callback(OnRemoveSuit));
		}

		private void OnSuitUp()
			=> _wearingSuit = true;

		private void OnRemoveSuit()
			=> _wearingSuit = false;

		private void OnGainFocus()
		{
			DebugLog.DebugWrite($"OnGainFocus");

			if (_playerResources == null)
			{
				_playerResources = Locator.GetPlayerTransform().GetComponent<PlayerResources>();
			}

			if (RespawnManager.Instance.RespawnNeeded)
			{
				_interactVolume.EnableSingleInteraction(true, _respawnIndex);
				_interactVolume.SetKeyCommandVisible(true, _respawnIndex);
				_interactVolume.GetInteractionAt(_respawnIndex).cachedScreenPrompt.SetDisplayState(ScreenPrompt.DisplayState.Attention);
			}
			else
			{
				_interactVolume.EnableSingleInteraction(false, _respawnIndex);
				_interactVolume.SetKeyCommandVisible(false, _respawnIndex);
				_interactVolume.GetInteractionAt(_respawnIndex).cachedScreenPrompt.SetDisplayState(ScreenPrompt.DisplayState.GrayedOut);
			}

			var needsHealing = _playerResources.GetHealthFraction() != 1f;
			var needsRefueling = _playerResources.GetFuelFraction() != 1f;

			UITextType uitextType;
			bool keyCommandVisible;
			if (needsHealing && needsRefueling)
			{
				uitextType = UITextType.RefillPrompt_0;
				keyCommandVisible = true;
			}
			else if (needsHealing)
			{
				uitextType = UITextType.RefillPrompt_2;
				keyCommandVisible = true;
			}
			else if (needsRefueling)
			{
				uitextType = UITextType.RefillPrompt_4;
				keyCommandVisible = true;
			}
			else
			{
				uitextType = UITextType.RefillPrompt_7;
				keyCommandVisible = false;
			}

			if (uitextType != UITextType.None)
			{
				_interactVolume.ChangePrompt(uitextType, _refillIndex);
			}

			if (_wearingSuit)
			{
				_interactVolume.EnableSingleInteraction(true, _refillIndex);
				_interactVolume.SetKeyCommandVisible(keyCommandVisible, _refillIndex);
				_interactVolume.GetInteractionAt(_refillIndex).cachedScreenPrompt.SetDisplayState(ScreenPrompt.DisplayState.Normal);
			}
			else
			{
				_interactVolume.EnableSingleInteraction(false, _refillIndex);
				_interactVolume.SetKeyCommandVisible(false, _refillIndex);
				_interactVolume.GetInteractionAt(_refillIndex).cachedScreenPrompt.SetDisplayState(ScreenPrompt.DisplayState.GrayedOut);
			}
		}

		private void OnPressInteract(IInputCommands inputCommand)
		{
			DebugLog.DebugWrite($"OnPressInteract");

			if (inputCommand == _interactVolume.GetInteractionAt(_refillIndex).inputCommand)
			{
				DebugLog.DebugWrite($"recovery");
				HandleRecovery();
			}
			else if (inputCommand == _interactVolume.GetInteractionAt(_respawnIndex).inputCommand)
			{
				DebugLog.DebugWrite($"respawn");
				RespawnManager.Instance.RespawnSomePlayer();
			}
			else
			{
				// the fuck????
			}
		}

		private void HandleRecovery()
		{
			var needsRefueling = _playerResources.GetFuelFraction() != 1f;
			var needsHealing = _playerResources.GetHealthFraction() != 1f;
			var flag4 = false;

			if (needsRefueling)
			{
				flag4 = true;
			}

			if (needsHealing)
			{
				flag4 = true;
			}

			if (flag4)
			{
				_playerResources.StartRefillResources(true, true);

				if (_playerAudioController != null)
				{
					if (needsRefueling)
					{
						_playerAudioController.PlayRefuel();
					}

					if (needsHealing)
					{
						_playerAudioController.PlayMedkit();
					}
				}

				_recovering = true;
				enabled = true;
				return;
			}

			_interactVolume.ResetInteraction();
		}

		private void Update()
		{
			if (_recovering)
			{
				var doneRecovering = true;
				if (_playerResources.GetFuelFraction() < 1f)
				{
					doneRecovering = false;
				}

				if (_playerResources.GetHealthFraction() < 1f)
				{
					doneRecovering = false;
				}

				if (doneRecovering)
				{
					_playerResources.StopRefillResources();
					_recovering = false;
				}
			}

			if (!_recovering)
			{
				enabled = false;
			}
		}
	}
}
