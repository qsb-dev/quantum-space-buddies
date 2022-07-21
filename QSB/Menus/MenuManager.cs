using EpicTransport;
using Mirror;
using QSB.Localization;
using QSB.Messaging;
using QSB.Player.TransformSync;
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

	// title screen only
	private GameObject ResumeGameButton;
	private GameObject NewGameButton;
	private Button HostButton;
	private GameObject ConnectButton;
	private PopupInputMenu ConnectPopup;
	private ThreeChoicePopupMenu HostGameTypePopup;
	private Text _loadingText;
	private StringBuilder _nowLoadingSB;
	private const int _titleButtonIndex = 2;
	private float _connectPopupOpenTime;

	private const string UpdateChangelog = $"QSB Version 0.20.2\r\nFixed an issue when the Little Scout was deployed by someone before joining a server.";

	private Action<bool> PopupClose;

	private bool _intentionalDisconnect;

	private GameObject _threeChoicePopupBase;

	public void Start()
	{
		Instance = this;

		_threeChoicePopupBase = Instantiate(Resources.FindObjectsOfTypeAll<PopupMenu>().First(x => x.name == "TwoButton-Popup" && x.transform.parent.name == "PopupCanvas" && x.transform.parent.parent.name == "TitleMenu").gameObject);
		DontDestroyOnLoad(_threeChoicePopupBase);
		_threeChoicePopupBase.SetActive(false);

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
			DebugLog.ToConsole($"Error - Language changed while not in title screen?! Should be impossible!", OWML.Common.MessageType.Error);
			return;
		}

		HostButton.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = QSBLocalization.Current.MainMenuHost;
		ConnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = QSBLocalization.Current.MainMenuConnect;
		var text = QSBCore.DebugSettings.UseKcpTransport ? QSBLocalization.Current.PublicIPAddress : QSBLocalization.Current.ProductUserID;
		ConnectPopup.SetUpPopup(text, InputLibrary.menuConfirm, InputLibrary.cancel, new ScreenPrompt(QSBLocalization.Current.Connect), new ScreenPrompt(QSBLocalization.Current.Cancel), false);
		ConnectPopup.SetInputFieldPlaceholderText(text);
		HostGameTypePopup.SetUpPopup(QSBLocalization.Current.HostExistingOrNew,
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
		var newPopup = Instantiate(_threeChoicePopupBase);

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
		newPopup.GetComponentsInChildren<LocalizedText>().ToList().ForEach(x => Destroy(x));

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
			new ScreenPrompt(cancelText),
			true,
			true);
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

		HostGameTypePopup = CreateThreeChoicePopup(QSBLocalization.Current.HostExistingOrNew, QSBLocalization.Current.ExistingSave, QSBLocalization.Current.NewSave, QSBLocalization.Current.Cancel);
		HostGameTypePopup.OnPopupConfirm1 += () => Host(false);
		HostGameTypePopup.OnPopupConfirm2 += () => Host(true);
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

		QuitButton = FindObjectOfType<PauseMenuManager>()._exitToMainMenuAction.gameObject;

		if (QSBCore.IsInMultiplayer)
		{
			SetButtonActive(DisconnectButton, true);
			SetButtonActive(QuitButton, false);
		}
		else
		{
			SetButtonActive(DisconnectButton, false);
			SetButtonActive(QuitButton, true);
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
		langController.AddTextElement(DisconnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>(), true, true, false);
		langController.AddTextElement(DisconnectPopup._labelText, false, true, false);
		langController.AddTextElement(DisconnectPopup._confirmButton._buttonText, false, true, false);
		langController.AddTextElement(DisconnectPopup._cancelButton._buttonText, false, true, false);
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
		mainMenuFontController.AddTextElement(HostButton.transform.GetChild(0).GetChild(1).GetComponent<Text>(), true, true, false);
		mainMenuFontController.AddTextElement(ConnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>(), true, true, false);

		mainMenuFontController.AddTextElement(OneButtonInfoPopup._labelText, false, true, false);
		mainMenuFontController.AddTextElement(OneButtonInfoPopup._confirmButton._buttonText, false, true, false);

		mainMenuFontController.AddTextElement(TwoButtonInfoPopup._labelText, false, true, false);
		mainMenuFontController.AddTextElement(TwoButtonInfoPopup._confirmButton._buttonText, false, true, false);
		mainMenuFontController.AddTextElement(TwoButtonInfoPopup._cancelButton._buttonText, false, true, false);

		mainMenuFontController.AddTextElement(ConnectPopup._labelText, false, true, false);
		mainMenuFontController.AddTextElement(ConnectPopup._confirmButton._buttonText, false, true, false);
		mainMenuFontController.AddTextElement(ConnectPopup._cancelButton._buttonText, false, true, false);

		mainMenuFontController.AddTextElement(HostGameTypePopup._labelText, false, true, false);
		mainMenuFontController.AddTextElement(HostGameTypePopup._confirmButton1._buttonText, false, true, false);
		mainMenuFontController.AddTextElement(HostGameTypePopup._confirmButton2._buttonText, false, true, false);
		mainMenuFontController.AddTextElement(HostGameTypePopup._cancelButton._buttonText, false, true, false);
	}

	private void Disconnect()
	{
		_intentionalDisconnect = true;

		QSBNetworkManager.singleton.StopHost();

		SetButtonActive(DisconnectButton, false);

		Locator.GetSceneMenuManager().pauseMenu._pauseMenu.EnableMenu(false);
		Locator.GetSceneMenuManager().pauseMenu._isPaused = false;
		OWInput.RestorePreviousInputs();

		LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack, 2f);
	}

	private void PreHost()
	{
		var doesSaveExist = PlayerData.LoadLoopCount() > 1;

		if (!doesSaveExist)
		{
			Host(true);
			return;
		}

		HostGameTypePopup.EnableMenu(true);
	}

	private void Host(bool newSave)
	{
		if (newSave)
		{
			PlayerData.ResetGame();
		}

		_intentionalDisconnect = false;

		SetButtonActive(ConnectButton, false);
		SetButtonActive(ResumeGameButton, false);
		SetButtonActive(NewGameButton, false);
		_loadingText = HostButton.transform.GetChild(0).GetChild(1).GetComponent<Text>();

		if (!QSBCore.DebugSettings.UseKcpTransport)
		{
			var productUserId = EOSSDKComponent.LocalUserProductIdString;

			PopupClose += confirm =>
			{
				if (confirm)
				{
					GUIUtility.systemCopyBuffer = productUserId;
				}

				LoadGame(PlayerData.GetWarpedToTheEye());
				Delay.RunWhen(() => TimeLoop._initialized, QSBNetworkManager.singleton.StartHost);
			};

			OpenInfoPopup(string.Format(QSBLocalization.Current.CopyProductUserIDToClipboard, productUserId)
				, QSBLocalization.Current.Yes
				, QSBLocalization.Current.No);
		}
		else
		{
			LoadGame(PlayerData.GetWarpedToTheEye());
			Delay.RunWhen(() => TimeLoop._initialized, QSBNetworkManager.singleton.StartHost);
		}
	}

	private void Connect()
	{
		_intentionalDisconnect = false;

		var address = ConnectPopup.GetInputText();
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
		// hack to get disconnect call if start client fails immediately
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
