using QSB.Localization;
using QSB.Messaging;
using QSB.ShipSync;
using QSB.Utility;
using UnityEngine;

namespace QSB.RespawnSync;

public class ShipRecoveryPoint : MonoBehaviour
{
	private MultipleInteractionVolume _interactVolume;
	private PlayerResources _playerResources;
	private PlayerAudioController _playerAudioController;
	private bool _recovering;
	private int _refillIndex;
	private int _respawnIndex;
	private bool _wearingSuit;
	private UITextType _respawnPlayerText;

	private void Awake()
	{
		_respawnPlayerText = UIHelper.AddToUITable(QSBLocalization.Current.RespawnPlayer);

		_interactVolume = this.GetRequiredComponent<MultipleInteractionVolume>();
		_interactVolume.OnPressInteract += OnPressInteract;
		_interactVolume.OnGainFocus += OnGainFocus;

		_refillIndex = _interactVolume.AddInteraction(InputLibrary.interact, InputMode.Character, UITextType.None, true, true);
		_respawnIndex = _interactVolume.AddInteraction(InputLibrary.interactSecondary, InputMode.Character, _respawnPlayerText, true, true);

		GlobalMessenger.AddListener(OWEvents.SuitUp, OnSuitUp);
		GlobalMessenger.AddListener(OWEvents.RemoveSuit, OnRemoveSuit);
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
		GlobalMessenger.RemoveListener(OWEvents.SuitUp, OnSuitUp);
		GlobalMessenger.RemoveListener(OWEvents.RemoveSuit, OnRemoveSuit);
	}

	private void OnSuitUp()
		=> _wearingSuit = true;

	private void OnRemoveSuit()
		=> _wearingSuit = false;

	private void OnGainFocus()
	{
		if (_playerResources == null)
		{
			_playerResources = Locator.GetPlayerTransform().GetComponent<PlayerResources>();
		}

		if (RespawnManager.Instance.RespawnNeeded && !ShipManager.Instance.IsShipWrecked)
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

		UITextType uiTextType;
		bool keyCommandVisible;
		if (needsHealing && needsRefueling)
		{
			uiTextType = UITextType.RefillPrompt_0;
			keyCommandVisible = true;
		}
		else if (needsHealing)
		{
			uiTextType = UITextType.RefillPrompt_2;
			keyCommandVisible = true;
		}
		else if (needsRefueling)
		{
			uiTextType = UITextType.RefillPrompt_4;
			keyCommandVisible = true;
		}
		else
		{
			uiTextType = UITextType.RefillPrompt_7;
			keyCommandVisible = false;
		}

		_interactVolume.ChangePrompt(uiTextType, _refillIndex);

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
		if (inputCommand == _interactVolume.GetInteractionAt(_refillIndex).inputCommand)
		{
			if (!_wearingSuit)
			{
				return;
			}

			HandleRecovery();
		}
		else if (inputCommand == _interactVolume.GetInteractionAt(_respawnIndex).inputCommand)
		{
			if (!RespawnManager.Instance.RespawnNeeded || ShipManager.Instance.IsShipWrecked)
			{
				return;
			}

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
		var needsRefill = false;

		if (needsRefueling)
		{
			needsRefill = true;
		}

		if (needsHealing)
		{
			needsRefill = true;
		}

		if (needsRefill)
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
