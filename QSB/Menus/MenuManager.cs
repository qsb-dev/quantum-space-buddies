using Mirror;
using QSB.Localization;
using QSB.Messaging;
using QSB.Player.TransformSync;
using QSB.SaveSync;
using QSB.SaveSync.Messages;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace QSB.Menus;

internal class MenuManager : MonoBehaviour, IAddComponentOnStart
{
	public static MenuManager Instance;

	private PopupMenu OneButtonInfoPopup;
	private PopupMenu TwoButtonInfoPopup;
	private bool _addedPauseLock;

	// Pause menu only
	private GameObject QuitButton;
	private GameObject DisconnectButton;
	private PopupMenu DisconnectPopup;
	private Button CopyIdButton;

	// title screen only
	private GameObject ResumeGameButton;
	private GameObject NewGameButton;
	private Button HostButton;
	private GameObject ConnectButton;
	private PopupInputMenu ConnectPopup;
	private FourChoicePopupMenu ExistingNewCopyPopup;
	private ThreeChoicePopupMenu NewCopyPopup;
	private ThreeChoicePopupMenu ExistingNewPopup;
	private Text _loadingText;
	private StringBuilder _nowLoadingSB;
	private const int _titleButtonIndex = 2;
	private float _connectPopupOpenTime;

	private const string UpdateChangelog = "QSB Version 0.21.1\r\nFixed gamepass not working, and fixed a small bug with light sensors.";

	private Action<bool> PopupClose;

	private bool _intentionalDisconnect;

	private GameObject _choicePopupPrefab;

	public void Start()
	{
		Instance = this;

		if (!_choicePopupPrefab)
		{
			_choicePopupPrefab = Instantiate(Resources.FindObjectsOfTypeAll<PopupMenu>().First(x => x.name == "TwoButton-Popup" && x.transform.parent?.name == "PopupCanvas" && x.transform.parent?.parent?.name == "TitleMenu").gameObject);
			DontDestroyOnLoad(_choicePopupPrefab);
			_choicePopupPrefab.SetActive(false);
		}

		MakeTitleMenus();
		QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
		QSBNetworkManager.singleton.OnClientConnected += OnConnected;
		QSBNetworkManager.singleton.OnClientDisconnected += OnDisconnected;

		if (QSBCore.Storage.LastUsedVersion != QSBCore.QSBVersion)
		{
			// recently updated!
			QSBCore.Storage.LastUsedVersion = QSBCore.QSBVersion;
			QSBCore.Helper.Storage.Save(QSBCore.Storage, "storage.json");
			QSBCore.MenuApi.RegisterStartupPopup(UpdateChangelog);
		}

		QSBLocalization.LanguageChanged += OnLanguageChanged;

		if (QSBCore.DebugSettings.AutoStart)
		{
			// auto host/connect
			Delay.RunWhen(PlayerData.IsLoaded, () =>
			{
				if (DebugLog.ProcessInstanceId == 0)
				{
					Host(false);
				}
				else
				{
					QSBCore.DefaultServerIP = "localhost";
					Connect();
				}
			});
		}
	}

	private void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool isUniverse)
	{
		if (isUniverse)
		{
			// wait a frame or else the changes won't actually happen
			Delay.RunNextFrame(InitPauseMenus);
			return;
		}

		if (newScene == OWScene.TitleScreen)
		{
			// wait a frame or else the changes won't actually happen
			Delay.RunNextFrame(MakeTitleMenus);
		}
	}

	private void ResetStringBuilder()
	{
		if (_nowLoadingSB == null)
		{
			_nowLoadingSB = new StringBuilder();
			return;
		}

		_nowLoadingSB.Length = 0;
	}

	private void OnLanguageChanged()
	{
		if (QSBSceneManager.CurrentScene != OWScene.TitleScreen)
		{
			DebugLog.ToConsole("Error - Language changed while not in title screen?! Should be impossible!", OWML.Common.MessageType.Error);
			return;
		}

		HostButton.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = QSBLocalization.Current.MainMenuHost;
		ConnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = QSBLocalization.Current.MainMenuConnect;
		var text = QSBCore.DebugSettings.UseKcpTransport ? QSBLocalization.Current.PublicIPAddress : QSBLocalization.Current.ProductUserID;
		ConnectPopup.SetUpPopup(text, InputLibrary.menuConfirm, InputLibrary.cancel, new ScreenPrompt(QSBLocalization.Current.Connect), new ScreenPrompt(QSBLocalization.Current.Cancel), false);
		ConnectPopup.SetInputFieldPlaceholderText(text);
		ExistingNewCopyPopup.SetUpPopup(QSBLocalization.Current.HostExistingOrNewOrCopy,
			InputLibrary.menuConfirm,
			InputLibrary.confirm2,
			InputLibrary.signalscope,
			InputLibrary.cancel,
			new ScreenPrompt(QSBLocalization.Current.ExistingSave),
			new ScreenPrompt(QSBLocalization.Current.NewSave),
			new ScreenPrompt(QSBLocalization.Current.CopySave),
			new ScreenPrompt(QSBLocalization.Current.Cancel));

		NewCopyPopup.SetUpPopup(QSBLocalization.Current.HostNewOrCopy,
			InputLibrary.menuConfirm,
			InputLibrary.confirm2,
			InputLibrary.cancel,
			new ScreenPrompt(QSBLocalization.Current.NewSave),
			new ScreenPrompt(QSBLocalization.Current.CopySave),
			new ScreenPrompt(QSBLocalization.Current.Cancel));

		ExistingNewPopup.SetUpPopup(QSBLocalization.Current.HostExistingOrNew,
			InputLibrary.menuConfirm,
			InputLibrary.confirm2,
			InputLibrary.cancel,
			new ScreenPrompt(QSBLocalization.Current.ExistingSave),
			new ScreenPrompt(QSBLocalization.Current.NewSave),
			new ScreenPrompt(QSBLocalization.Current.Cancel));
	}

	private void Update()
	{
		if ((LoadManager.GetLoadingScene() == OWScene.SolarSystem || LoadManager.GetLoadingScene() == OWScene.EyeOfTheUniverse)
			&& _loadingText != null)
		{
			var num = LoadManager.GetAsyncLoadProgress();
			num = num < 0.1f
				? Mathf.InverseLerp(0f, 0.1f, num) * 0.9f
				: 0.9f + Mathf.InverseLerp(0.1f, 1f, num) * 0.1f;
			ResetStringBuilder();
			_nowLoadingSB.Append(UITextLibrary.GetString(UITextType.LoadingMessage));
			_nowLoadingSB.Append(num.ToString("P0"));
			_loadingText.text = _nowLoadingSB.ToString();
		}
	}

	public ThreeChoicePopupMenu CreateThreeChoicePopup(string message, string confirm1Text, string confirm2Text, string cancelText)
	{
		var newPopup = Instantiate(_choicePopupPrefab);

		switch (LoadManager.GetCurrentScene())
		{
			case OWScene.TitleScreen:
				newPopup.transform.parent = GameObject.Find("/TitleMenu/PopupCanvas").transform;
				break;
			case OWScene.SolarSystem:
			case OWScene.EyeOfTheUniverse:
				newPopup.transform.parent = GameObject.Find("/PauseMenu/PopupCanvas").transform;
				break;
		}

		newPopup.transform.localPosition = Vector3.zero;
		newPopup.transform.localScale = Vector3.one;
		newPopup.GetComponentsInChildren<LocalizedText>().ForEach(Destroy);

		var originalPopup = newPopup.GetComponent<PopupMenu>();

		var ok1Button = originalPopup._confirmButton.gameObject;

		var ok2Button = Instantiate(ok1Button, ok1Button.transform.parent);
		ok2Button.transform.SetSiblingIndex(1);

		var popup = newPopup.AddComponent<ThreeChoicePopupMenu>();
		popup._labelText = originalPopup._labelText;
		popup._cancelAction = originalPopup._cancelAction;
		popup._ok1Action = originalPopup._okAction;
		popup._ok2Action = ok2Button.GetComponent<SubmitAction>();
		popup._cancelButton = originalPopup._cancelButton;
		popup._confirmButton1 = originalPopup._confirmButton;
		popup._confirmButton2 = ok2Button.GetComponent<ButtonWithHotkeyImageElement>();
		popup._rootCanvas = originalPopup._rootCanvas;
		popup._menuActivationRoot = originalPopup._menuActivationRoot;
		popup._startEnabled = originalPopup._startEnabled;
		popup._selectOnActivate = originalPopup._selectOnActivate;
		popup._selectableItemsRoot = originalPopup._selectableItemsRoot;
		popup._subMenus = originalPopup._subMenus;
		popup._menuOptions = originalPopup._menuOptions;
		popup.SetUpPopup(
			message,
			InputLibrary.menuConfirm,
			InputLibrary.confirm2,
			InputLibrary.cancel,
			new ScreenPrompt(confirm1Text),
			new ScreenPrompt(confirm2Text),
			new ScreenPrompt(cancelText));
		return popup;
	}

	public FourChoicePopupMenu CreateFourChoicePopup(string message, string confirm1Text, string confirm2Text, string confirm3Text, string cancelText)
	{
		var newPopup = Instantiate(_choicePopupPrefab);

		switch (LoadManager.GetCurrentScene())
		{
			case OWScene.TitleScreen:
				newPopup.transform.parent = GameObject.Find("/TitleMenu/PopupCanvas").transform;
				break;
			case OWScene.SolarSystem:
			case OWScene.EyeOfTheUniverse:
				newPopup.transform.parent = GameObject.Find("/PauseMenu/PopupCanvas").transform;
				break;
		}

		newPopup.transform.localPosition = Vector3.zero;
		newPopup.transform.localScale = Vector3.one;
		newPopup.GetComponentsInChildren<LocalizedText>().ForEach(Destroy);

		var originalPopup = newPopup.GetComponent<PopupMenu>();

		var ok1Button = originalPopup._confirmButton.gameObject;

		var ok2Button = Instantiate(ok1Button, ok1Button.transform.parent);
		ok2Button.transform.SetSiblingIndex(1);

		var ok3Button = Instantiate(ok1Button, ok1Button.transform.parent);
		ok3Button.transform.SetSiblingIndex(2);

		var popup = newPopup.AddComponent<FourChoicePopupMenu>();
		popup._labelText = originalPopup._labelText;
		popup._cancelAction = originalPopup._cancelAction;
		popup._ok1Action = originalPopup._okAction;
		popup._ok2Action = ok2Button.GetComponent<SubmitAction>();
		popup._ok3Action = ok3Button.GetComponent<SubmitAction>();
		popup._cancelButton = originalPopup._cancelButton;
		popup._confirmButton1 = originalPopup._confirmButton;
		popup._confirmButton2 = ok2Button.GetComponent<ButtonWithHotkeyImageElement>();
		popup._confirmButton3 = ok3Button.GetComponent<ButtonWithHotkeyImageElement>();
		popup._rootCanvas = originalPopup._rootCanvas;
		popup._menuActivationRoot = originalPopup._menuActivationRoot;
		popup._startEnabled = originalPopup._startEnabled;
		popup._selectOnActivate = originalPopup._selectOnActivate;
		popup._selectableItemsRoot = originalPopup._selectableItemsRoot;
		popup._subMenus = originalPopup._subMenus;
		popup._menuOptions = originalPopup._menuOptions;
		popup.SetUpPopup(
			message,
			InputLibrary.menuConfirm,
			InputLibrary.confirm2,
			InputLibrary.signalscope,
			InputLibrary.cancel,
			new ScreenPrompt(confirm1Text),
			new ScreenPrompt(confirm2Text),
			new ScreenPrompt(confirm3Text),
			new ScreenPrompt(cancelText));
		return popup;
	}

	public void LoadGame(bool inEye)
	{
		var sceneToLoad = inEye ? OWScene.EyeOfTheUniverse : OWScene.SolarSystem;
		LoadManager.LoadSceneAsync(sceneToLoad, true, LoadManager.FadeType.ToBlack, 1f, false);
		Locator.GetMenuInputModule().DisableInputs();
	}

	private void OpenInfoPopup(string message, string okButtonText)
	{
		OneButtonInfoPopup.SetUpPopup(message, InputLibrary.menuConfirm, InputLibrary.cancel, new ScreenPrompt(okButtonText), null, true, false);

		OWTime.Pause(OWTime.PauseType.Menu);
		OWInput.ChangeInputMode(InputMode.Menu);

		var pauseCommandListener = Locator.GetPauseCommandListener();
		if (pauseCommandListener != null)
		{
			pauseCommandListener.AddPauseCommandLock();
			_addedPauseLock = true;
		}

		OneButtonInfoPopup.EnableMenu(true);
	}

	private void OpenInfoPopup(string message, string okButtonText, string cancelButtonText)
	{
		TwoButtonInfoPopup.SetUpPopup(message, InputLibrary.menuConfirm, InputLibrary.cancel, new ScreenPrompt(okButtonText), new ScreenPrompt(cancelButtonText));

		OWTime.Pause(OWTime.PauseType.Menu);
		OWInput.ChangeInputMode(InputMode.Menu);

		var pauseCommandListener = Locator.GetPauseCommandListener();
		if (pauseCommandListener != null)
		{
			pauseCommandListener.AddPauseCommandLock();
			_addedPauseLock = true;
		}

		TwoButtonInfoPopup.EnableMenu(true);
	}

	private void OnCloseInfoPopup(bool confirm)
	{
		var pauseCommandListener = Locator.GetPauseCommandListener();
		if (pauseCommandListener != null && _addedPauseLock)
		{
			pauseCommandListener.RemovePauseCommandLock();
			_addedPauseLock = false;
		}

		OWTime.Unpause(OWTime.PauseType.Menu);
		OWInput.RestorePreviousInputs();

		PopupClose?.SafeInvoke(confirm);
		PopupClose = null;
	}

	private void CreateCommonPopups()
	{
		var text = QSBCore.DebugSettings.UseKcpTransport ? QSBLocalization.Current.PublicIPAddress : QSBLocalization.Current.ProductUserID;
		ConnectPopup = QSBCore.MenuApi.MakeInputFieldPopup(text, text, QSBLocalization.Current.Connect, QSBLocalization.Current.Cancel);
		ConnectPopup.CloseMenuOnOk(false);
		ConnectPopup.OnPopupConfirm += () =>
		{
			// fixes dumb thing with using keyboard to open popup
			if (OWMath.ApproxEquals(Time.time, _connectPopupOpenTime))
			{
				return;
			}

			ConnectPopup.EnableMenu(false);
			Connect();
		};
		ConnectPopup.OnActivateMenu += () => _connectPopupOpenTime = Time.time;

		OneButtonInfoPopup = QSBCore.MenuApi.MakeInfoPopup("", "");
		OneButtonInfoPopup.OnPopupConfirm += () => OnCloseInfoPopup(true);

		TwoButtonInfoPopup = QSBCore.MenuApi.MakeTwoChoicePopup("", "", "");
		TwoButtonInfoPopup.OnPopupConfirm += () => OnCloseInfoPopup(true);
		TwoButtonInfoPopup.OnPopupCancel += () => OnCloseInfoPopup(false);

		ExistingNewCopyPopup = CreateFourChoicePopup(QSBLocalization.Current.HostExistingOrNewOrCopy,
			QSBLocalization.Current.ExistingSave,
			QSBLocalization.Current.NewSave,
			QSBLocalization.Current.CopySave,
			QSBLocalization.Current.Cancel);
		ExistingNewCopyPopup.OnPopupConfirm1 += () => Host(false);
		ExistingNewCopyPopup.OnPopupConfirm2 += () => Host(true);
		ExistingNewCopyPopup.OnPopupConfirm3 += () =>
		{
			DebugLog.DebugWrite("Replacing multiplayer save with singleplayer save");
			QSBCore.IsInMultiplayer = true;

			if (QSBCore.IsStandalone)
			{
				var currentProfile = QSBStandaloneProfileManager.SharedInstance.currentProfile;
				QSBStandaloneProfileManager.SharedInstance.SaveGame(currentProfile.gameSave, null, null, null);
			}
			else
			{
				var gameSave = QSBMSStoreProfileManager.SharedInstance.currentProfileGameSave;
				QSBMSStoreProfileManager.SharedInstance.SaveGame(gameSave, null, null, null);
			}

			Host(false);
		};

		NewCopyPopup = CreateThreeChoicePopup(QSBLocalization.Current.HostNewOrCopy,
			QSBLocalization.Current.NewSave,
			QSBLocalization.Current.CopySave,
			QSBLocalization.Current.Cancel);
		NewCopyPopup.OnPopupConfirm1 += () => Host(true);
		NewCopyPopup.OnPopupConfirm2 += () =>
		{
			DebugLog.DebugWrite("Replacing multiplayer save with singleplayer save");
			QSBCore.IsInMultiplayer = true;

			if (QSBCore.IsStandalone)
			{
				var currentProfile = QSBStandaloneProfileManager.SharedInstance.currentProfile;
				QSBStandaloneProfileManager.SharedInstance.SaveGame(currentProfile.gameSave, null, null, null);
			}
			else
			{
				var gameSave = QSBMSStoreProfileManager.SharedInstance.currentProfileGameSave;
				QSBMSStoreProfileManager.SharedInstance.SaveGame(gameSave, null, null, null);
			}

			Host(false);
		};

		ExistingNewPopup = CreateThreeChoicePopup(QSBLocalization.Current.HostExistingOrNew,
			QSBLocalization.Current.ExistingSave,
			QSBLocalization.Current.NewSave,
			QSBLocalization.Current.Cancel);
		ExistingNewPopup.OnPopupConfirm1 += () => Host(false);
		ExistingNewPopup.OnPopupConfirm2 += () => Host(true);
	}

	private static void SetButtonActive(Button button, bool active)
		=> SetButtonActive(button ? button.gameObject : null, active);

	private static void SetButtonActive(GameObject button, bool active)
	{
		if (button == null)
		{
			DebugLog.DebugWrite($"Warning - Tried to set button to {active}, but it was null.", OWML.Common.MessageType.Warning);
			return;
		}

		button.SetActive(active);
		button.GetComponent<CanvasGroup>().alpha = active ? 1 : 0;
	}

	private void InitPauseMenus()
	{
		CreateCommonPopups();

		DisconnectPopup = QSBCore.MenuApi.MakeTwoChoicePopup(QSBLocalization.Current.DisconnectAreYouSure, QSBLocalization.Current.Yes, QSBLocalization.Current.No);
		DisconnectPopup.OnPopupConfirm += Disconnect;

		DisconnectButton = QSBCore.MenuApi.PauseMenu_MakeMenuOpenButton(QSBLocalization.Current.PauseMenuDisconnect, DisconnectPopup);
		CopyIdButton = QSBCore.MenuApi.PauseMenu_MakeSimpleButton(QSBLocalization.Current.CopyId);
		CopyIdButton.onClick.AddListener(() =>
		{
			var lobbyActivitySecret = ((DiscordMirror.DiscordTransport)Transport.activeTransport).GetConnectString();
			GUIUtility.systemCopyBuffer = lobbyActivitySecret;
		});

		QuitButton = FindObjectOfType<PauseMenuManager>()._exitToMainMenuAction.gameObject;

		if (QSBCore.IsInMultiplayer)
		{
			SetButtonActive(DisconnectButton, true);
			SetButtonActive(QuitButton, false);
			SetButtonActive(CopyIdButton, !QSBCore.DebugSettings.UseKcpTransport);
		}
		else
		{
			SetButtonActive(DisconnectButton, false);
			SetButtonActive(QuitButton, true);
			SetButtonActive(CopyIdButton, false);
		}

		var text = QSBCore.IsHost
			? QSBLocalization.Current.PauseMenuStopHosting
			: QSBLocalization.Current.PauseMenuDisconnect;
		DisconnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = text;

		var popupText = QSBCore.IsHost
			? QSBLocalization.Current.StopHostingAreYouSure
			: QSBLocalization.Current.DisconnectAreYouSure;
		DisconnectPopup._labelText.text = popupText;

		var langController = QSBWorldSync.GetUnityObject<PauseMenuManager>().transform.GetChild(0).GetComponent<FontAndLanguageController>();
		langController.AddTextElement(DisconnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>());
		langController.AddTextElement(CopyIdButton.transform.GetChild(0).GetChild(1).GetComponent<Text>());
		langController.AddTextElement(DisconnectPopup._labelText, false);
		langController.AddTextElement(DisconnectPopup._confirmButton._buttonText, false);
		langController.AddTextElement(DisconnectPopup._cancelButton._buttonText, false);
	}

	private void MakeTitleMenus()
	{
		CreateCommonPopups();

		HostButton = QSBCore.MenuApi.TitleScreen_MakeSimpleButton(QSBLocalization.Current.MainMenuHost, _titleButtonIndex);
		HostButton.onClick.AddListener(PreHost);

		ConnectButton = QSBCore.MenuApi.TitleScreen_MakeMenuOpenButton(QSBLocalization.Current.MainMenuConnect, _titleButtonIndex + 1, ConnectPopup);

		ResumeGameButton = GameObject.Find("MainMenuLayoutGroup/Button-ResumeGame");
		NewGameButton = GameObject.Find("MainMenuLayoutGroup/Button-NewGame");

		SetButtonActive(ConnectButton, true);
		Delay.RunWhen(PlayerData.IsLoaded, () => SetButtonActive(ResumeGameButton, PlayerData.LoadLoopCount() > 1));
		SetButtonActive(NewGameButton, true);

		if (QSBCore.DebugSettings.SkipTitleScreen)
		{
			Application.runInBackground = true;
			var titleScreenManager = FindObjectOfType<TitleScreenManager>();
			var titleScreenAnimation = titleScreenManager._cameraController;
			const float small = 1 / 1000f;
			titleScreenAnimation._gamepadSplash = false;
			titleScreenAnimation._introPan = false;
			titleScreenAnimation._fadeDuration = small;
			titleScreenAnimation.Start();
			var titleAnimationController = titleScreenManager._gfxController;
			titleAnimationController._logoFadeDelay = small;
			titleAnimationController._logoFadeDuration = small;
			titleAnimationController._echoesFadeDelay = small;
			titleAnimationController._optionsFadeDelay = small;
			titleAnimationController._optionsFadeDuration = small;
			titleAnimationController._optionsFadeSpacing = small;
		}

		var mainMenuFontController = GameObject.Find("MainMenu").GetComponent<FontAndLanguageController>();
		mainMenuFontController.AddTextElement(HostButton.transform.GetChild(0).GetChild(1).GetComponent<Text>());
		mainMenuFontController.AddTextElement(ConnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>());

		mainMenuFontController.AddTextElement(OneButtonInfoPopup._labelText, false);
		mainMenuFontController.AddTextElement(OneButtonInfoPopup._confirmButton._buttonText, false);

		mainMenuFontController.AddTextElement(TwoButtonInfoPopup._labelText, false);
		mainMenuFontController.AddTextElement(TwoButtonInfoPopup._confirmButton._buttonText, false);
		mainMenuFontController.AddTextElement(TwoButtonInfoPopup._cancelButton._buttonText, false);

		mainMenuFontController.AddTextElement(ConnectPopup._labelText, false);
		mainMenuFontController.AddTextElement(ConnectPopup._confirmButton._buttonText, false);
		mainMenuFontController.AddTextElement(ConnectPopup._cancelButton._buttonText, false);

		mainMenuFontController.AddTextElement(ExistingNewCopyPopup._labelText, false);
		mainMenuFontController.AddTextElement(ExistingNewCopyPopup._confirmButton1._buttonText, false);
		mainMenuFontController.AddTextElement(ExistingNewCopyPopup._confirmButton2._buttonText, false);
		mainMenuFontController.AddTextElement(ExistingNewCopyPopup._confirmButton3._buttonText, false);
		mainMenuFontController.AddTextElement(ExistingNewCopyPopup._cancelButton._buttonText, false);

		mainMenuFontController.AddTextElement(NewCopyPopup._labelText, false);
		mainMenuFontController.AddTextElement(NewCopyPopup._confirmButton1._buttonText, false);
		mainMenuFontController.AddTextElement(NewCopyPopup._confirmButton2._buttonText, false);
		mainMenuFontController.AddTextElement(NewCopyPopup._cancelButton._buttonText, false);

		mainMenuFontController.AddTextElement(ExistingNewPopup._labelText, false);
		mainMenuFontController.AddTextElement(ExistingNewPopup._confirmButton1._buttonText, false);
		mainMenuFontController.AddTextElement(ExistingNewPopup._confirmButton2._buttonText, false);
		mainMenuFontController.AddTextElement(ExistingNewPopup._cancelButton._buttonText, false);
	}

	private void Disconnect()
	{
		_intentionalDisconnect = true;

		QSBNetworkManager.singleton.StopHost();

		SetButtonActive(DisconnectButton, false);
		SetButtonActive(CopyIdButton, false);

		Locator.GetSceneMenuManager().pauseMenu._pauseMenu.EnableMenu(false);
		Locator.GetSceneMenuManager().pauseMenu._isPaused = false;
		OWInput.RestorePreviousInputs();

		LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack, 2f);
	}

	private void PreHost()
	{
		var doesSingleplayerSaveExist = false;
		var doesMultiplayerSaveExist = false;
		if (!QSBCore.IsStandalone)
		{
			var manager = QSBMSStoreProfileManager.SharedInstance;
			doesSingleplayerSaveExist = manager.currentProfileGameSave.loopCount > 1;
			doesMultiplayerSaveExist = manager.currentProfileMultiplayerGameSave.loopCount > 1;
		}
		else
		{
			var profile = QSBStandaloneProfileManager.SharedInstance.currentProfile;
			doesSingleplayerSaveExist = profile.gameSave.loopCount > 1;
			doesMultiplayerSaveExist = profile.multiplayerGameSave.loopCount > 1;
		}

		if (doesSingleplayerSaveExist && doesMultiplayerSaveExist)
		{
			DebugLog.DebugWrite("CASE 1 - Both singleplayer and multiplayer saves exist.");
			// ask if we want to use the existing multiplayer save,
			// start a new multiplayer save, or copy the singleplayer save
			ExistingNewCopyPopup.EnableMenu(true);
		}
		else if (doesSingleplayerSaveExist && !doesMultiplayerSaveExist)
		{
			DebugLog.DebugWrite("CASE 2 - Only a singleplayer save exists.");
			// ask if we want to start a new multiplayer save or copy the singleplayer save
			NewCopyPopup.EnableMenu(true);
		}
		else if (!doesSingleplayerSaveExist && doesMultiplayerSaveExist)
		{
			DebugLog.DebugWrite("CASE 3 - Only multiplayer save exists.");
			// ask if we want to use the existing multiplayer save or start a new one
			ExistingNewPopup.EnableMenu(true);
		}
		else if (!doesSingleplayerSaveExist && !doesMultiplayerSaveExist)
		{
			DebugLog.DebugWrite("CASE 4 - Neither a singleplayer or a multiplayer save exists.");
			// create a new multiplayer save - nothing to copy or load
			Host(true);
		}
	}

	private void Host(bool newMultiplayerSave)
	{
		QSBCore.IsInMultiplayer = true;

		if (newMultiplayerSave)
		{
			DebugLog.DebugWrite("Resetting game...");
			PlayerData.ResetGame();
		}
		else
		{
			DebugLog.DebugWrite("Loading multiplayer game...");
			if (QSBCore.IsStandalone)
			{
				var profile = QSBStandaloneProfileManager.SharedInstance.currentProfile;
				PlayerData.Init(profile.multiplayerGameSave, profile.settingsSave, profile.graphicsSettings, profile.inputJSON);
			}
			else
			{
				var manager = QSBMSStoreProfileManager.SharedInstance;
				PlayerData.Init(manager.currentProfileMultiplayerGameSave, manager.currentProfileGameSettings, manager.currentProfileGraphicsSettings, manager.currentProfileInputJSON);
			}
		}

		_intentionalDisconnect = false;

		SetButtonActive(ConnectButton, false);
		SetButtonActive(ResumeGameButton, false);
		SetButtonActive(NewGameButton, false);
		_loadingText = HostButton.transform.GetChild(0).GetChild(1).GetComponent<Text>();

		LoadGame(PlayerData.GetWarpedToTheEye());
		Delay.RunWhen(() => TimeLoop._initialized, QSBNetworkManager.singleton.StartHost);
	}

	private void Connect()
	{
		QSBCore.IsInMultiplayer = true;
		_intentionalDisconnect = false;

		if (QSBCore.IsStandalone)
		{
			var profile = QSBStandaloneProfileManager.SharedInstance.currentProfile;
			PlayerData.Init(profile.multiplayerGameSave, profile.settingsSave, profile.graphicsSettings, profile.inputJSON);
		}
		else
		{
			var manager = QSBMSStoreProfileManager.SharedInstance;
			PlayerData.Init(manager.currentProfileMultiplayerGameSave, manager.currentProfileGameSettings, manager.currentProfileGraphicsSettings, manager.currentProfileInputJSON);
		}

		var address = ConnectPopup.GetInputText().Trim();
		if (address == string.Empty)
		{
			address = QSBCore.DefaultServerIP;
		}

		SetButtonActive(HostButton, false);
		SetButtonActive(ResumeGameButton, false);
		SetButtonActive(NewGameButton, false);
		_loadingText = ConnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>();
		_loadingText.text = QSBLocalization.Current.Connecting;
		Locator.GetMenuInputModule().DisableInputs();

		QSBNetworkManager.singleton.networkAddress = address;
		// hack to get disconnect call if start client fails immediately (happens on kcp transport when failing to resolve host name)
		typeof(NetworkClient).GetProperty(nameof(NetworkClient.connection))!.SetValue(null, new NetworkConnectionToServer());
		QSBNetworkManager.singleton.StartClient();
	}

	private static void OnConnected()
	{
		if (!QSBCore.IsHost)
		{
			Delay.RunWhen(() => PlayerTransformSync.LocalInstance,
				() => new RequestGameStateMessage().Send());
		}
	}

	public void OnKicked(string reason)
	{
		QSBCore.IsInMultiplayer = false;
		_intentionalDisconnect = true;

		PopupClose += _ =>
		{
			if (QSBSceneManager.IsInUniverse)
			{
				LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack, 2f);
			}
		};

		OpenInfoPopup(string.Format(QSBLocalization.Current.ServerRefusedConnection, reason), QSBLocalization.Current.OK);
	}

	private void OnDisconnected(string error)
	{
		QSBCore.IsInMultiplayer = false;

		if (_intentionalDisconnect)
		{
			DebugLog.DebugWrite("intentional disconnect. dont show popup");
			_intentionalDisconnect = false;
		}
		else
		{
			PopupClose += _ =>
			{
				if (QSBSceneManager.IsInUniverse)
				{
					LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack, 2f);
				}
			};

			OpenInfoPopup(string.Format(QSBLocalization.Current.ClientDisconnectWithError, error), QSBLocalization.Current.OK);
		}

		SetButtonActive(DisconnectButton, false);
		SetButtonActive(ConnectButton, true);
		SetButtonActive(QuitButton, true);
		SetButtonActive(HostButton, true);
		SetButtonActive(ResumeGameButton, PlayerData.LoadLoopCount() > 1);
		SetButtonActive(NewGameButton, true);
		SetButtonActive(CopyIdButton, false);
		if (ConnectButton)
		{
			ConnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = QSBLocalization.Current.MainMenuConnect;
		}

		if (HostButton)
		{
			HostButton.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = QSBLocalization.Current.MainMenuHost;
		}

		_loadingText = null;
		Locator.GetMenuInputModule().EnableInputs();
	}
}
