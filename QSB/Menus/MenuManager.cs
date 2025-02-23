using Mirror;
using OWML.Common;
using OWML.Utils;
using QSB.Localization;
using QSB.Messaging;
using QSB.Player.TransformSync;
using QSB.SaveSync;
using QSB.SaveSync.Messages;
using QSB.Utility;
using QSB.WorldSync;
using Steamworks;
using System;
using System.Linq;
using System.Text;
using OWML.Common.Interfaces.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace QSB.Menus;

public class MenuManager : MonoBehaviour, IAddComponentOnStart
{
	public static MenuManager Instance;

	private PopupMenu OneButtonInfoPopup;
	private PopupMenu TwoButtonInfoPopup;
	private bool _addedPauseLock;

	// Pause menu only
	private GameObject QuitButton;
	private SubmitAction DisconnectButton;
	private PopupMenu DisconnectPopup;

	// title screen only
	private GameObject ResumeGameButton;
	private GameObject NewGameButton;
	private SubmitAction HostButton;
	private SubmitAction ConnectButton;
	private IOWMLPopupInputMenu ConnectPopup;
	private IOWMLFourChoicePopupMenu ExistingNewCopyPopup;
	private IOWMLThreeChoicePopupMenu NewCopyPopup;
	private IOWMLThreeChoicePopupMenu ExistingNewPopup;
	private Text _loadingText;
	private StringBuilder _nowLoadingSB;
	private const int _titleButtonIndex = 2;
	private float _connectPopupOpenTime;

	private Action<bool> PopupClose;

	private bool _intentionalDisconnect;

	private GameObject _choicePopupPrefab;

	public bool WillBeHost;

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

	public void OnLanguageChanged()
	{
		if (QSBSceneManager.CurrentScene != OWScene.TitleScreen)
		{
			DebugLog.ToConsole("Error - Language changed while not in title screen?! Should be impossible!", MessageType.Error);
			return;
		}

		HostButton.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = QSBLocalization.Current.MainMenuHost;
		ConnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = QSBLocalization.Current.MainMenuConnect;
		var text = QSBCore.UseKcpTransport ? QSBLocalization.Current.PublicIPAddress : QSBLocalization.Current.SteamID;
		ConnectPopup.SetText(text, text, QSBLocalization.Current.Connect, QSBLocalization.Current.Cancel);
		ExistingNewCopyPopup.SetText(QSBLocalization.Current.HostExistingOrNewOrCopy, QSBLocalization.Current.ExistingSave, QSBLocalization.Current.NewSave, QSBLocalization.Current.CopySave, QSBLocalization.Current.Cancel);
		NewCopyPopup.SetText(QSBLocalization.Current.HostNewOrCopy, QSBLocalization.Current.NewSave, QSBLocalization.Current.CopySave, QSBLocalization.Current.Cancel);
		ExistingNewPopup.SetText(QSBLocalization.Current.HostExistingOrNew, QSBLocalization.Current.ExistingSave, QSBLocalization.Current.NewSave, QSBLocalization.Current.Cancel);
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
		var text = QSBCore.UseKcpTransport ? QSBLocalization.Current.PublicIPAddress : QSBLocalization.Current.SteamID;
		ConnectPopup = QSBCore.Helper.MenuHelper.PopupMenuManager.CreateInputFieldPopup(text, text, QSBLocalization.Current.Connect, QSBLocalization.Current.Cancel);
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

		ConnectPopup.OnActivateMenu += () =>
		{
			_connectPopupOpenTime = Time.time;
			if (QSBCore.Helper.Interaction.ModExists("Raicuparta.NomaiVR"))
			{
				// ClearInputTextField is called AFTER OnActivateMenu
				Delay.RunNextFrame(() => ConnectPopup.GetInputField().SetTextWithoutNotify(GUIUtility.systemCopyBuffer));
			}
		};

		OneButtonInfoPopup = QSBCore.Helper.MenuHelper.PopupMenuManager.CreateInfoPopup("", "");
		OneButtonInfoPopup.OnPopupConfirm += () => OnCloseInfoPopup(true);

		TwoButtonInfoPopup = QSBCore.Helper.MenuHelper.PopupMenuManager.CreateTwoChoicePopup("", "", "");
		TwoButtonInfoPopup.OnPopupConfirm += () => OnCloseInfoPopup(true);
		TwoButtonInfoPopup.OnPopupCancel += () => OnCloseInfoPopup(false);

		ExistingNewCopyPopup = QSBCore.Helper.MenuHelper.PopupMenuManager.CreateFourChoicePopup(
			QSBLocalization.Current.HostExistingOrNewOrCopy,
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

		NewCopyPopup = QSBCore.Helper.MenuHelper.PopupMenuManager.CreateThreeChoicePopup(
			QSBLocalization.Current.HostNewOrCopy,
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

		ExistingNewPopup = QSBCore.Helper.MenuHelper.PopupMenuManager.CreateThreeChoicePopup(
			QSBLocalization.Current.HostExistingOrNew,
			QSBLocalization.Current.ExistingSave,
			QSBLocalization.Current.NewSave,
			QSBLocalization.Current.Cancel);
		ExistingNewPopup.OnPopupConfirm1 += () => Host(false);
		ExistingNewPopup.OnPopupConfirm2 += () => Host(true);
	}

	private static void SetButtonActive(SubmitAction button, bool active)
		=> SetButtonActive(button ? button.gameObject : null, active);

	private static void SetButtonActive(GameObject button, bool active)
	{
		if (button == null)
		{
			DebugLog.DebugWrite($"Warning - Tried to set button to {active}, but it was null.", MessageType.Warning);
			return;
		}

		var activeAlpha = 1;

		if (QSBSceneManager.CurrentScene == OWScene.TitleScreen)
		{
			var titleAnimationController = QSBWorldSync.GetUnityObject<TitleScreenManager>()._gfxController;
			activeAlpha = titleAnimationController.IsTitleAnimationComplete() ? 1 : 0;
		}

		button.SetActive(active);
		button.GetComponent<CanvasGroup>().alpha = active ? activeAlpha : 0;
	}

	private void InitPauseMenus()
	{
		CreateCommonPopups();

		DisconnectPopup = QSBCore.Helper.MenuHelper.PopupMenuManager.CreateTwoChoicePopup(QSBLocalization.Current.DisconnectAreYouSure, QSBLocalization.Current.Yes, QSBLocalization.Current.No);
		DisconnectPopup.OnPopupConfirm += Disconnect;

		DisconnectButton = QSBCore.Helper.MenuHelper.PauseMenuManager.MakeMenuOpenButton(QSBLocalization.Current.PauseMenuDisconnect, DisconnectPopup, 0, true);

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
		QSBCore.Helper.MenuHelper.PauseMenuManager.SetButtonText(DisconnectButton, text);

		var popupText = QSBCore.IsHost
			? QSBLocalization.Current.StopHostingAreYouSure
			: QSBLocalization.Current.DisconnectAreYouSure;
		DisconnectPopup._labelText.text = popupText;

		var langController = QSBWorldSync.GetUnityObject<PauseMenuManager>().transform.GetChild(0).GetComponent<FontAndLanguageController>();
		langController.AddTextElement(DisconnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>());
		langController.AddTextElement(DisconnectPopup._labelText, false);
		langController.AddTextElement(DisconnectPopup._confirmButton._buttonText, false);
		langController.AddTextElement(DisconnectPopup._cancelButton._buttonText, false);
		langController.AddTextElement(OneButtonInfoPopup._labelText, false);
		langController.AddTextElement(OneButtonInfoPopup._confirmButton._buttonText, false);
		langController.AddTextElement(TwoButtonInfoPopup._labelText, false);
		langController.AddTextElement(TwoButtonInfoPopup._confirmButton._buttonText, false);
		langController.AddTextElement(TwoButtonInfoPopup._cancelButton._buttonText, false);
	}

	private void MakeTitleMenus()
	{
		CreateCommonPopups();

		HostButton = QSBCore.Helper.MenuHelper.TitleMenuManager.CreateTitleButton(QSBLocalization.Current.MainMenuHost, _titleButtonIndex, true);
		HostButton.OnSubmitAction += PreHost;

		ConnectButton = QSBCore.Helper.MenuHelper.TitleMenuManager.CreateTitleButton(QSBLocalization.Current.MainMenuConnect, _titleButtonIndex + 1, true);
		ConnectButton.OnSubmitAction += () => ConnectPopup.EnableMenu(true);

		ResumeGameButton = GameObject.Find("MainMenuLayoutGroup/Button-ResumeGame");
		NewGameButton = GameObject.Find("MainMenuLayoutGroup/Button-NewGame");

		SetButtonActive(ConnectButton, true);
		Delay.RunWhen(PlayerData.IsLoaded, () => SetButtonActive(ResumeGameButton, PlayerData.LoadLoopCount() > 1));
		SetButtonActive(NewGameButton, true);
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
		WillBeHost = true;

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

		if (!QSBCore.UseKcpTransport)
		{
			var steamId = SteamUser.GetSteamID().ToString();

			PopupClose += confirm =>
			{
				if (confirm)
				{
					GUIUtility.systemCopyBuffer = steamId;
				}

				LoadGame(PlayerData.GetWarpedToTheEye());
				// wait until scene load and then wait until Start has ran
				// why is this done? GameStateMessage etc works on title screen since nonhost has to deal with that
				Delay.RunWhen(() => TimeLoop._initialized, () =>
				{
					QSBNetworkManager.singleton.StartHost();
					Delay.RunWhen(() => NetworkServer.active, () => WillBeHost = false);
				});
			};

			OpenInfoPopup(string.Format(QSBLocalization.Current.CopySteamIDToClipboard, steamId)
				, QSBLocalization.Current.Yes
				, QSBLocalization.Current.No);
		}
		else
		{
			LoadGame(PlayerData.GetWarpedToTheEye());
			// wait until scene load and then wait until Start has ran
			// why is this done? GameStateMessage etc works on title screen since nonhost has to deal with that
			Delay.RunWhen(() => TimeLoop._initialized, () =>
			{
				QSBNetworkManager.singleton.StartHost();
				Delay.RunWhen(() => NetworkServer.active, () => WillBeHost = false);
			});
		}
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

	private void OnDisconnected(TransportError error, string reason)
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

			OpenInfoPopup(string.Format(QSBLocalization.Current.ClientDisconnectWithError, reason), QSBLocalization.Current.OK);
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
