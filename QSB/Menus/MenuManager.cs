using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.SaveSync.Messages;
using QSB.Utility;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace QSB.Menus
{
	internal class MenuManager : MonoBehaviour
	{
		public static MenuManager Instance;

		private IMenuAPI MenuApi => QSBCore.MenuApi;

		private PopupMenu IPPopup;
		private PopupMenu InfoPopup;
		private bool _addedPauseLock;

		// Pause menu only
		private Button HostButton;
		private GameObject QuitButton;
		private GameObject DisconnectButton;
		private PopupMenu DisconnectPopup;
		private StringBuilder _nowLoadingSB;
		protected Text _loadingText;

		// title screen only
		private GameObject ResumeGameButton;
		private GameObject NewGameButton;
		private GameObject ClientButton;

		private const int _ClientButtonIndex = 2;
		private const int _DisconnectIndex = 3;

		public void Start()
		{
			Instance = this;
			MakeTitleMenus();
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
			QSBNetworkManager.Instance.OnClientConnected += OnConnected;
			QSBNetworkManager.Instance.OnClientDisconnected += OnDisconnected;
			QSBNetworkManager.Instance.OnClientErrorThrown += OnClientError;
		}

		private void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool isUniverse)
		{
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
			    && (LoadManager.GetLoadingScene() == OWScene.SolarSystem || LoadManager.GetLoadingScene() == OWScene.EyeOfTheUniverse)
			    && _loadingText != null)
			{
				var num = LoadManager.GetAsyncLoadProgress();
				num = num < 0.1f
					? Mathf.InverseLerp(0f, 0.1f, num) * 0.9f
					: 0.9f + (Mathf.InverseLerp(0.1f, 1f, num) * 0.1f);
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

		private void OpenInfoPopup(string message, string buttonText)
		{
			InfoPopup.SetUpPopup(message, InputLibrary.menuConfirm, InputLibrary.cancel, new ScreenPrompt(buttonText), null, true, false);

			OWTime.Pause(OWTime.PauseType.System);
			OWInput.ChangeInputMode(InputMode.Menu);

			var pauseCommandListener = Locator.GetPauseCommandListener();
			if (pauseCommandListener != null)
			{
				pauseCommandListener.AddPauseCommandLock();
				_addedPauseLock = true;
			}

			InfoPopup.EnableMenu(true);
		}

		private void OnCloseInfoPopup()
		{
			var pauseCommandListener = Locator.GetPauseCommandListener();
			if (pauseCommandListener != null && _addedPauseLock)
			{
				pauseCommandListener.RemovePauseCommandLock();
				_addedPauseLock = false;
			}

			OWTime.Unpause(OWTime.PauseType.System);
			OWInput.RestorePreviousInputs();

			if (QSBSceneManager.IsInUniverse)
			{
				LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack, 2f, true);
			}
		}

		private void CreateCommonPopups()
		{
			IPPopup = MenuApi.MakeInputFieldPopup("IP Address", "IP Address", "Connect", "Cancel");
			IPPopup.OnPopupConfirm += Connect;

			InfoPopup = MenuApi.MakeInfoPopup("", "");
			InfoPopup.OnDeactivateMenu += OnCloseInfoPopup;
		}

		private void SetButtonActive(Button button, bool active)
			=> SetButtonActive(button?.gameObject, active);

		private void SetButtonActive(GameObject button, bool active)
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

			HostButton = MenuApi.PauseMenu_MakeSimpleButton("OPEN TO MULTIPLAYER");
			HostButton.onClick.AddListener(Host);

			DisconnectPopup = MenuApi.MakeTwoChoicePopup("Are you sure you want to disconnect?\r\nThis will send you back to the main menu.", "YES", "NO");
			DisconnectPopup.OnPopupConfirm += Disconnect;

			DisconnectButton = MenuApi.PauseMenu_MakeMenuOpenButton("DISCONNECT", DisconnectPopup);

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
				? "STOP HOSTING"
				: "DISCONNECT";
			DisconnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = text;

			var popupText = QSBCore.IsHost
				? "Are you sure you want to stop hosting?\r\nThis will disconnect all clients and send everyone back to the main menu."
				: "Are you sure you want to disconnect?\r\nThis will send you back to the main menu.";
			DisconnectPopup._labelText.text = popupText;
		}

		private void MakeTitleMenus()
		{
			CreateCommonPopups();

			ClientButton = MenuApi.TitleScreen_MakeMenuOpenButton("CONNECT TO MULTIPLAYER", _ClientButtonIndex, IPPopup);

			_loadingText = ClientButton.transform.GetChild(0).GetChild(1).GetComponent<Text>();

			ResumeGameButton = GameObject.Find("MainMenuLayoutGroup/Button-ResumeGame");
			NewGameButton = GameObject.Find("MainMenuLayoutGroup/Button-NewGame");

			if (QSBCore.IsInMultiplayer)
			{
				SetButtonActive(ClientButton, false);

				if (QSBCore.IsHost)
				{
					SetButtonActive(ResumeGameButton, StandaloneProfileManager.SharedInstance.currentProfileGameSave.loopCount > 1);
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
				SetButtonActive(ClientButton, true);
				SetButtonActive(ResumeGameButton, StandaloneProfileManager.SharedInstance.currentProfileGameSave.loopCount > 1);
				SetButtonActive(NewGameButton, true);
			}

			if (QSBCore.SkipTitleScreen)
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
			QSBNetworkManager.Instance.StopHost();
			SetButtonActive(DisconnectButton.gameObject, false);

			Locator.GetSceneMenuManager().pauseMenu._pauseMenu.EnableMenu(false);
			Locator.GetSceneMenuManager().pauseMenu._isPaused = false;

			OWInput.RestorePreviousInputs();

			LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack, 2f, true);
		}

		private void Host()
		{
			if (QSBNetworkManager.Instance.StartHost() != null)
			{
				SetButtonActive(DisconnectButton, true);
				SetButtonActive(HostButton, false);
				SetButtonActive(QuitButton, false);
			}
			else
			{
				OpenInfoPopup($"Failed to start server.", "OK");
			}

			var text = QSBCore.IsHost
				? "STOP HOSTING"
				: "DISCONNECT";
			DisconnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = text;

			var popupText = QSBCore.IsHost
				? "Are you sure you want to stop hosting?\r\nThis will disconnect all clients and send everyone back to the main menu."
				: "Are you sure you want to disconnect?\r\nThis will send you back to the main menu.";
			DisconnectPopup._labelText.text = popupText;
		}

		private void Connect()
		{
			QSBNetworkManager.Instance.networkAddress = string.Concat((IPPopup as PopupInputMenu).GetInputText().Where(c => !char.IsWhiteSpace(c)));
			QSBNetworkManager.Instance.StartClient();

			if (QSBSceneManager.CurrentScene == OWScene.TitleScreen)
			{
				SetButtonActive(ResumeGameButton, false);
				SetButtonActive(NewGameButton, false);
			}

			if (QSBSceneManager.IsInUniverse)
			{
				SetButtonActive(QuitButton, false);
			}
		}

		private void OnConnected()
		{
			if (QSBCore.IsHost || !QSBCore.IsInMultiplayer)
			{
				return;
			}

			QSBCore.UnityEvents.RunWhen(() => PlayerTransformSync.LocalInstance,
				() => new RequestGameStateMessage().Send());
		}

		public void OnKicked(KickReason reason)
		{
			var text = reason switch
			{
				KickReason.QSBVersionNotMatching => "Server refused connection as QSB version does not match.",
				KickReason.GameVersionNotMatching => "Server refused connection as Outer Wilds version does not match.",
				KickReason.GamePlatformNotMatching => "Server refused connection as Outer Wilds platform does not match. (Steam/Epic)",
				KickReason.DLCNotMatching => "Server refused connection as DLC installation state does not match.",
				KickReason.None => "Kicked from server. No reason given.",
				_ => $"Kicked from server. KickReason:{reason}",
			};
			OpenInfoPopup(text, "OK");

			SetButtonActive(DisconnectButton, false);
			SetButtonActive(ClientButton, true);
			SetButtonActive(HostButton, true);
			SetButtonActive(QuitButton, true);
		}

		private void OnDisconnected(NetworkError error)
		{
			if (error == NetworkError.Ok)
			{
				return;
			}

			var text = error switch
			{
				NetworkError.Timeout => "Client disconnected with error!\r\nConnection timed out.",
				_ => $"Client disconnected with error!\r\nNetworkError:{error}",
			};
			OpenInfoPopup(text, "OK");

			SetButtonActive(DisconnectButton, false);
			SetButtonActive(ClientButton, true);
			SetButtonActive(QuitButton, true);
			SetButtonActive(HostButton, true);
			SetButtonActive(ResumeGameButton, StandaloneProfileManager.SharedInstance.currentProfileGameSave.loopCount > 1);
			SetButtonActive(NewGameButton, true);
		}

		private void OnClientError(NetworkError error)
		{
			if (error == NetworkError.Ok)
			{
				// lol wut
				return;
			}

			string text;
			switch (error)
			{
				case NetworkError.DNSFailure:
					text = "Internal QNet client error!\r\nDNS Faliure. Address was invalid or could not be resolved.";
					DebugLog.DebugWrite($"dns failure");
					SetButtonActive(DisconnectButton, false);
					SetButtonActive(ClientButton, true);
					SetButtonActive(HostButton, true);
					SetButtonActive(ResumeGameButton, StandaloneProfileManager.SharedInstance.currentProfileGameSave.loopCount > 1);
					SetButtonActive(NewGameButton, true);
					SetButtonActive(QuitButton, true);
					break;
				default:
					text = $"Internal QNet client error!\n\nNetworkError:{error}";
					break;
			}

			OpenInfoPopup(text, "OK");
		}
	}
}