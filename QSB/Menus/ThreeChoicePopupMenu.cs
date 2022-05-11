using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QSB.Menus;

[RequireComponent(typeof(Canvas))]
public class ThreeChoicePopupMenu : Menu
{
	public Text _labelText;
	public SubmitAction _cancelAction;
	public SubmitAction _ok1Action;
	public SubmitAction _ok2Action;
	public ButtonWithHotkeyImageElement _cancelButton;
	public ButtonWithHotkeyImageElement _confirmButton1;
	public ButtonWithHotkeyImageElement _confirmButton2;
	public Canvas _rootCanvas;

	protected Canvas _popupCanvas;
	protected GameObject _blocker;
	protected bool _closeMenuOnOk = true;
	protected IInputCommands _ok1Command;
	protected IInputCommands _ok2Command;
	protected IInputCommands _cancelCommand;
	protected bool _usingGamepad;

	public event PopupConfirmEvent OnPopupConfirm1;
	public event PopupConfirmEvent OnPopupConfirm2;
	public event PopupValidateEvent OnPopupValidate;
	public event PopupCancelEvent OnPopupCancel;

	public override Selectable GetSelectOnActivate()
	{
		_usingGamepad = OWInput.UsingGamepad();
		return _usingGamepad ? null : _selectOnActivate;
	}

	public virtual void SetUpPopup(
		string message,
		IInputCommands ok1Command,
		IInputCommands ok2Command,
		IInputCommands cancelCommand,
		ScreenPrompt ok1Prompt,
		ScreenPrompt ok2Prompt,
		ScreenPrompt cancelPrompt,
		bool closeMenuOnOk = true,
		bool setCancelButtonActive = true)
	{
		_labelText.text = message;
		SetUpPopupCommands(ok1Command, ok2Command, cancelCommand, ok1Prompt, ok2Prompt, cancelPrompt);
		if (!(_cancelAction != null))
		{
			Debug.LogWarning("PopupMenu.SetUpPopup Cancel button not set!");
			return;
		}

		_cancelAction.gameObject.SetActive(setCancelButtonActive);
		if (setCancelButtonActive)
		{
			_selectOnActivate = _cancelAction.GetRequiredComponent<Selectable>();
			return;
		}

		_selectOnActivate = _ok1Action.GetRequiredComponent<Selectable>();
	}

	public virtual void SetUpPopupCommands(
		IInputCommands ok1Command,
		IInputCommands ok2Command,
		IInputCommands cancelCommand,
		ScreenPrompt ok1Prompt,
		ScreenPrompt ok2Prompt,
		ScreenPrompt cancelPrompt)
	{
		_ok1Command = ok1Command;
		_ok2Command = ok2Command;
		_cancelCommand = cancelCommand;
		_confirmButton1.SetPrompt(ok1Prompt, InputMode.Menu);
		_confirmButton2.SetPrompt(ok2Prompt, InputMode.Menu);
		_cancelButton.SetPrompt(cancelPrompt, InputMode.Menu);
	}

	public virtual void ResetPopup()
	{
		_labelText.text = "";
		_ok1Command = null;
		_ok2Command = null;
		_cancelCommand = null;
		_cancelButton.SetPrompt(null, InputMode.Menu);
		_confirmButton1.SetPrompt(null, InputMode.Menu);
		_confirmButton2.SetPrompt(null, InputMode.Menu);
		_selectOnActivate = null;
	}

	public virtual void CloseMenuOnOk(bool value)
	{
		_closeMenuOnOk = value;
	}

	public virtual bool EventsHaveListeners()
	{
		return OnPopupCancel != null || OnPopupConfirm1 != null || OnPopupConfirm2 != null;
	}

	public override void InitializeMenu()
	{
		base.InitializeMenu();
		if (_cancelAction != null)
		{
			_cancelAction.OnSubmitAction += InvokeCancel;
		}

		_ok1Action.OnSubmitAction += InvokeOk1;
		_ok2Action.OnSubmitAction += InvokeOk2;
		_popupCanvas = gameObject.GetAddComponent<Canvas>();
		_popupCanvas.overrideSorting = true;
		_popupCanvas.sortingOrder = 30000;
		gameObject.GetAddComponent<GraphicRaycaster>();
		gameObject.GetAddComponent<CanvasGroup>();
	}

	protected virtual void Update()
	{
		if (_cancelCommand != null && OWInput.IsNewlyPressed(_cancelCommand, InputMode.All))
		{
			InvokeCancel();
			return;
		}

		if (_ok1Command != null && OWInput.IsNewlyPressed(_ok1Command, InputMode.All))
		{
			InvokeOk1();
			return;
		}

		if (_ok2Command != null && OWInput.IsNewlyPressed(_ok2Command, InputMode.All))
		{
			InvokeOk2();
			return;
		}

		if (_usingGamepad != OWInput.UsingGamepad())
		{
			_usingGamepad = OWInput.UsingGamepad();
			if (_usingGamepad)
			{
				Locator.GetMenuInputModule().SelectOnNextUpdate(null);
				return;
			}

			Locator.GetMenuInputModule().SelectOnNextUpdate(_selectOnActivate);
		}
	}

	public override void EnableMenu(bool value)
	{
		if (value == _enabledMenu)
		{
			return;
		}

		_enabledMenu = value;
		if (_enabledMenu && !_initialized)
		{
			InitializeMenu();
		}

		if (!_addToMenuStackManager)
		{
			if (_enabledMenu)
			{
				Activate();
				if (_selectOnActivate != null)
				{
					var component = _selectOnActivate.GetComponent<SelectableAudioPlayer>();
					if (component != null)
					{
						component.SilenceNextSelectEvent();
					}

					Locator.GetMenuInputModule().SelectOnNextUpdate(_selectOnActivate);
					return;
				}
			}
			else
			{
				Deactivate(false);
			}

			return;
		}

		if (_enabledMenu)
		{
			MenuStackManager.SharedInstance.Push(this, true);
			return;
		}

		if (MenuStackManager.SharedInstance.Peek() == this)
		{
			MenuStackManager.SharedInstance.Pop(false);
			return;
		}

		Debug.LogError("Cannot disable Menu unless it is on the top the MenuLayerManager stack. Current menu on top: " + MenuStackManager.SharedInstance.Peek().gameObject.name);
	}

	public override void Activate()
	{
		base.Activate();
		if (_rootCanvas != null)
		{
			_blocker = CreateBlocker(_rootCanvas);
		}
	}

	public override void Deactivate(bool keepPreviousMenuVisible = false)
	{
		if (_rootCanvas != null)
		{
			DestroyBlocker(_blocker);
		}

		var component = _cancelAction.GetComponent<UIStyleApplier>();
		if (component != null)
		{
			component.ChangeState(UIElementState.NORMAL, true);
		}

		component = _ok1Action.GetComponent<UIStyleApplier>();
		if (component != null)
		{
			component.ChangeState(UIElementState.NORMAL, true);
		}

		component = _ok2Action.GetComponent<UIStyleApplier>();
		if (component != null)
		{
			component.ChangeState(UIElementState.NORMAL, true);
		}

		base.Deactivate(keepPreviousMenuVisible);
	}

	public override void OnCancelEvent(GameObject selectedObj, BaseEventData eventData)
	{
		base.OnCancelEvent(selectedObj, eventData);
		OnPopupCancel?.Invoke();
	}

	protected virtual void InvokeCancel()
	{
		EnableMenu(false);
		OnPopupCancel?.Invoke();
	}

	protected virtual bool Validate()
	{
		var flag = true;
		if (OnPopupValidate != null)
		{
			var invocationList = OnPopupValidate.GetInvocationList();
			for (var i = 0; i < invocationList.Length; i++)
			{
				var flag2 = (bool)invocationList[i].DynamicInvoke(Array.Empty<object>());
				flag = flag && flag2;
			}
		}

		return flag;
	}

	protected virtual void InvokeOk1()
	{
		if (!Validate())
		{
			return;
		}

		if (_closeMenuOnOk)
		{
			EnableMenu(false);
		}

		OnPopupConfirm1?.Invoke();
	}

	protected virtual void InvokeOk2()
	{
		if (!Validate())
		{
			return;
		}

		if (_closeMenuOnOk)
		{
			EnableMenu(false);
		}

		OnPopupConfirm2?.Invoke();
	}

	protected virtual GameObject CreateBlocker(Canvas rootCanvas)
	{
		var gameObject = new GameObject("Blocker");
		var rectTransform = gameObject.AddComponent<RectTransform>();
		rectTransform.SetParent(rootCanvas.transform, false);
		rectTransform.anchorMin = Vector3.zero;
		rectTransform.anchorMax = Vector3.one;
		rectTransform.sizeDelta = Vector2.zero;
		var canvas = gameObject.AddComponent<Canvas>();
		canvas.overrideSorting = true;
		canvas.sortingLayerID = _popupCanvas.sortingLayerID;
		canvas.sortingOrder = _popupCanvas.sortingOrder - 1;
		gameObject.AddComponent<GraphicRaycaster>();
		var image = gameObject.AddComponent<Image>();
		if (Locator.GetUIStyleManager() != null)
		{
			image.color = Locator.GetUIStyleManager().GetPopupBlockerColor();
			return gameObject;
		}

		image.color = Color.clear;
		return gameObject;
	}

	protected virtual void DestroyBlocker(GameObject blocker)
	{
		Destroy(blocker);
	}

	public delegate void PopupConfirmEvent();

	public delegate bool PopupValidateEvent();

	public delegate void PopupCancelEvent();
}
