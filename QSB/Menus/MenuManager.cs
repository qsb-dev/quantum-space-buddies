using EpicTransport;
using Mirror;
using QSB.Messaging;
using QSB.Player.TransformSync;
using QSB.SaveSync.Messages;
using QSB.Utility;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace QSB.Menus;

internal class MenuManager : MonoBehaviour, IAddComponentOnStart
{
	public static MenuManager Instance;

	private PopupMenu IPPopup;
	private PopupMenu OneButtonInfoPopup;
	private PopupMenu TwoButtonInfoPopup;
	private bool _addedPauseLock;

	// Pause menu only
	private Button HostButton;
	private GameObject QuitButton;
	private GameObject DisconnectButton;
	private PopupMenu DisconnectPopup;

	// title screen only
	private GameObject ResumeGameButton;
	private GameObject NewGameButton;
	private GameObject ConnectButton;
	private StringBuilder _nowLoadingSB;
	private Text _loadingText;
	private const int _titleButtonIndex = 2;

	private const string HostString = "OPEN TO MULTIPLAYER";
	private const string ConnectString = "CONNECT TO MULTIPLAYER";
	private const string DisconnectString = "DISCONNECT";
	private const string StopHostingString = "STOP HOSTING";

	private Action PopupOK;

	private bool _intentionalDisconnect;

	public void Start()
	{
		Instance = this;
		MakeTitleMenus();
		QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
		QSBNetworkManager.singleton.OnClientConnected += OnConnected;
		QSBNetworkManager.singleton.OnClientDisconnected += OnDisconnected;
	}

	private void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool isUniverse)
	{
		if (newScene == OWScene.EyeOfTheUniverse)
		{
			GlobalMessenger<EyeState>.AddListener(OWEvents.EyeStateChanged, OnEyeStateChanged);
		}
		else
		{
			GlobalMessenger<EyeState>.RemoveListener(OWEvents.EyeStateChanged, OnEyeStateChanged);
		}

		if (isUniverse)
		{
			InitPauseMenus();
			return;
		}

		if (newScene == OWScene.TitleScreen)
		{
			MakeTitleMenus();
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

	private void Update()
	{
		if (QSBCore.IsInMultiplayer
			&& LoadManager.GetLoadingScene() is OWScene.SolarSystem or OWScene.EyeOfTheUniverse
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

	public void JoinGame(bool inEye)
	{
		if (inEye)
		{
			LoadManager.LoadSceneAsync(OWScene.EyeOfTheUniverse, true, LoadManager.FadeType.ToBlack, 1f, false);
			Locator.GetMenuInputModule().DisableInputs();
		}
		else
		{
			LoadManager.LoadSceneAsync(OWScene.SolarSystem, true, LoadManager.FadeType.ToBlack, 1f, false);
			Locator.GetMenuInputModule().DisableInputs();
		}
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

	private void OnCloseInfoPopup()
	{
		var pauseCommandListener = Locator.GetPauseCommandListener();
		if (pauseCommandListener != null && _addedPauseLock)
		{
			pauseCommandListener.RemovePauseCommandLock();
			_addedPauseLock = false;
		}

		OWTime.Unpause(OWTime.PauseType.Menu);
		OWInput.RestorePreviousInputs();

		PopupOK?.SafeInvoke();
		PopupOK = null;
	}

	private void CreateCommonPopups()
	{
		var text = QSBCore.DebugSettings.UseKcpTransport ? "Public IP Address" : "Product User ID";
		IPPopup = QSBCore.MenuApi.MakeInputFieldPopup(text, text, "Connect", "Cancel");
		IPPopup.OnPopupConfirm += Connect;

		OneButtonInfoPopup = QSBCore.MenuApi.MakeInfoPopup("", "");
		OneButtonInfoPopup.OnDeactivateMenu += OnCloseInfoPopup;

		TwoButtonInfoPopup = QSBCore.MenuApi.MakeTwoChoicePopup("", "", "");
		TwoButtonInfoPopup.OnDeactivateMenu += OnCloseInfoPopup;
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

		HostButton = QSBCore.MenuApi.PauseMenu_MakeSimpleButton(HostString);
		HostButton.onClick.AddListener(Host);

		DisconnectPopup = QSBCore.MenuApi.MakeTwoChoicePopup("Are you sure you want to disconnect?\r\nThis will send you back to the main menu.", "YES", "NO");
		DisconnectPopup.OnPopupConfirm += Disconnect;

		DisconnectButton = QSBCore.MenuApi.PauseMenu_MakeMenuOpenButton(DisconnectString, DisconnectPopup);

		QuitButton = FindObjectOfType<PauseMenuManager>()._exitToMainMenuAction.gameObject;

		if (QSBCore.IsInMultiplayer)
		{
			SetButtonActive(HostButton, false);
			SetButtonActive(DisconnectButton, true);
			SetButtonActive(QuitButton, false);
		}
		else
		{
			SetButtonActive(HostButton, true);
			SetButtonActive(DisconnectButton, false);
			SetButtonActive(QuitButton, true);
		}

		var text = QSBCore.IsHost
			? StopHostingString
			: DisconnectString;
		DisconnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = text;

		var popupText = QSBCore.IsHost
			? "Are you sure you want to stop hosting?\r\nThis will disconnect all clients and send everyone back to the main menu."
			: "Are you sure you want to disconnect?\r\nThis will send you back to the main menu.";
		DisconnectPopup._labelText.text = popupText;
	}

	private void OnEyeStateChanged(EyeState state)
	{
		if (state >= EyeState.Observatory)
		{
			SetButtonActive(HostButton, false);
		}
	}

	private void MakeTitleMenus()
	{
		CreateCommonPopups();

		ConnectButton = QSBCore.MenuApi.TitleScreen_MakeMenuOpenButton(ConnectString, _titleButtonIndex, IPPopup);
		_loadingText = ConnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>();

		ResumeGameButton = GameObject.Find("MainMenuLayoutGroup/Button-ResumeGame");
		NewGameButton = GameObject.Find("MainMenuLayoutGroup/Button-NewGame");

		if (QSBCore.IsInMultiplayer)
		{
			SetButtonActive(ConnectButton, false);

			if (QSBCore.IsHost)
			{
				Delay.RunWhen(PlayerData.IsLoaded, () => SetButtonActive(ResumeGameButton, PlayerData.LoadLoopCount() > 1));
				SetButtonActive(NewGameButton, true);
			}
			else
			{
				SetButtonActive(ResumeGameButton, false);
				SetButtonActive(NewGameButton, false);
			}
		}
		else
		{
			SetButtonActive(ConnectButton, true);
			Delay.RunWhen(PlayerData.IsLoaded, () => SetButtonActive(ResumeGameButton, PlayerData.LoadLoopCount() > 1));
			SetButtonActive(NewGameButton, true);
		}

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
	}

	private void Disconnect()
	{
		_intentionalDisconnect = true;

		QSBNetworkManager.singleton.StopHost();
		SetButtonActive(DisconnectButton.gameObject, false);

		Locator.GetSceneMenuManager().pauseMenu._pauseMenu.EnableMenu(false);
		Locator.GetSceneMenuManager().pauseMenu._isPaused = false;

		OWInput.RestorePreviousInputs();

		LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack, 2f);
	}

	private void Host()
	{
		_intentionalDisconnect = false;

		SetButtonActive(DisconnectButton, true);
		SetButtonActive(HostButton, false);
		SetButtonActive(QuitButton, false);

		QSBNetworkManager.singleton.StartHost();

		var text = QSBCore.IsHost
			? StopHostingString
			: DisconnectString;
		DisconnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = text;

		var popupText = QSBCore.IsHost
			? "Are you sure you want to stop hosting?\r\nThis will disconnect all clients and send everyone back to the main menu."
			: "Are you sure you want to disconnect?\r\nThis will send you back to the main menu.";
		DisconnectPopup._labelText.text = popupText;

		if (!QSBCore.DebugSettings.UseKcpTransport)
		{
			var productUserId = EOSSDKComponent.LocalUserProductIdString;

			PopupOK += () => GUIUtility.systemCopyBuffer = productUserId;

			OpenInfoPopup("Hosting server.\r\nClients will connect using your product user id, which is :\r\n" +
				$"{productUserId}\r\n" +
				"Do you want to copy this to the clipboard?"
				, "YES"
				, "NO");
		}
	}

	private void Connect()
	{
		_intentionalDisconnect = false;

		var address = ((PopupInputMenu)IPPopup).GetInputText();
		if (address == string.Empty)
		{
			address = QSBCore.DefaultServerIP;
		}

		if (QSBSceneManager.CurrentScene == OWScene.TitleScreen)
		{
			SetButtonActive(ResumeGameButton, false);
			SetButtonActive(NewGameButton, false);
		}

		if (QSBSceneManager.IsInUniverse)
		{
			SetButtonActive(QuitButton, false);
		}

		QSBNetworkManager.singleton.networkAddress = address;
		// hack to get disconnect call if start client fails immediately
		typeof(NetworkClient).GetProperty(nameof(NetworkClient.connection))!.SetValue(null, new NetworkConnectionToServer());
		QSBNetworkManager.singleton.StartClient();
	}

	private static void OnConnected()
	{
		if (QSBCore.IsHost || !QSBCore.IsInMultiplayer)
		{
			return;
		}

		Delay.RunWhen(() => PlayerTransformSync.LocalInstance,
			() => new RequestGameStateMessage().Send());
	}

	public void OnKicked(string reason)
	{
		_intentionalDisconnect = true;

		PopupOK += () =>
		{
			if (QSBSceneManager.IsInUniverse)
			{
				LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack, 2f);
			}
		};

		OpenInfoPopup($"Server refused connection.\r\n{reason}", "OK");

		SetButtonActive(DisconnectButton, false);
		SetButtonActive(ConnectButton, true);
		SetButtonActive(QuitButton, true);
		SetButtonActive(HostButton, true);
		Delay.RunWhen(PlayerData.IsLoaded, () => SetButtonActive(ResumeGameButton, PlayerData.LoadLoopCount() > 1));
		SetButtonActive(NewGameButton, true);
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
			PopupOK += () =>
			{
				if (QSBSceneManager.IsInUniverse)
				{
					LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack, 2f);
				}
			};

			OpenInfoPopup($"Client disconnected with error!\r\n{error}", "OK");
		}

		SetButtonActive(DisconnectButton, false);
		SetButtonActive(ConnectButton, true);
		SetButtonActive(QuitButton, true);
		SetButtonActive(HostButton, true);
		Delay.RunWhen(PlayerData.IsLoaded, () => SetButtonActive(ResumeGameButton, PlayerData.LoadLoopCount() > 1));
		SetButtonActive(NewGameButton, true);
	}
}
