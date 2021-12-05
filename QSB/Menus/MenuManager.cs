using QSB.Player;
using QSB.Utility;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace QSB.Menus
{
	internal class MenuManager : MonoBehaviour
	{
		public static MenuManager Instance;

		private IMenuAPI MenuApi => QSBCore.MenuApi;
		private PopupMenu PopupMenu;
		private PopupMenu InfoPopup;
		private bool _addedPauseLock;

		private Button HostButton;
		private GameObject ClientButton;
		private Button DisconnectButton;

		private GameObject ResumeGameButton;
		private GameObject NewGameButton;

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
		}

		private void CreateCommonPopups()
		{
			PopupMenu = MenuApi.MakeInputFieldPopup("IP Address", "IP Address", "Connect", "Cancel");
			PopupMenu.OnPopupConfirm += Connect;

			InfoPopup = MenuApi.MakeInfoPopup("", "");
			InfoPopup.OnDeactivateMenu += OnCloseInfoPopup;
		}

		private void SetButtonActive(Button button, bool active)
			=> SetButtonActive(button.gameObject, active);

		private void SetButtonActive(GameObject button, bool active)
		{
			if (button == null)
			{
				return;
			}

			DebugLog.DebugWrite($"Set {button.name} to {active}");
			button.SetActive(active);
			button.GetComponent<CanvasGroup>().alpha = active ? 1 : 0;
		}

		private void InitPauseMenus()
		{
			CreateCommonPopups();

			HostButton = MenuApi.PauseMenu_MakeSimpleButton("MULTIPLAYER (HOST)");
			HostButton.onClick.AddListener(Host);

			DisconnectButton = MenuApi.PauseMenu_MakeSimpleButton("DISCONNECT");
			DisconnectButton.onClick.AddListener(Disconnect);

			if (QSBCore.IsInMultiplayer)
			{
				ClientButton.SetActive(false);
				HostButton.gameObject.SetActive(false);
				SetButtonActive(DisconnectButton, true);
			}
			else
			{
				SetButtonActive(DisconnectButton, false);
			}

			OnConnected();
		}

		private void MakeTitleMenus()
		{
			CreateCommonPopups();

			ClientButton = MenuApi.TitleScreen_MakeMenuOpenButton("MULTIPLAYER (CONNECT)", _ClientButtonIndex, PopupMenu);

			DisconnectButton = MenuApi.TitleScreen_MakeSimpleButton("DISCONNECT", _DisconnectIndex);
			DisconnectButton.onClick.AddListener(Disconnect);

			ResumeGameButton = GameObject.Find("MainMenuLayoutGroup/Button-ResumeGame");
			NewGameButton = GameObject.Find("MainMenuLayoutGroup/Button-NewGame");

			if (QSBCore.IsInMultiplayer)
			{
				ClientButton.SetActive(false);
				SetButtonActive(DisconnectButton, true);

				if (QSBCore.IsHost)
				{
					SetButtonActive(ResumeGameButton, true);
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
				SetButtonActive(DisconnectButton, false);
				SetButtonActive(ResumeGameButton, true);
				SetButtonActive(NewGameButton, true);
			}

			OnConnected();

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

		private void Join()
		{
			DebugLog.DebugWrite($"Join");
		}

		private void Disconnect()
		{
			QSBNetworkManager.Instance.StopHost();
			SetButtonActive(DisconnectButton.gameObject, false);
			SetButtonActive(ClientButton, true);
			SetButtonActive(HostButton, true);
		}

		private void Host()
		{
			DebugLog.DebugWrite($"Host");

			if (QSBNetworkManager.Instance.StartHost() != null)
			{
				SetButtonActive(DisconnectButton, true);
				SetButtonActive(ClientButton, false);
				SetButtonActive(HostButton, false);
			}
			else
			{
				OpenInfoPopup($"Failed to start server.", "OK");
			}
		}

		private void Connect()
		{
			QSBNetworkManager.Instance.networkAddress = string.Concat((PopupMenu as PopupInputMenu).GetInputText().Where(c => !char.IsWhiteSpace(c)));
			QSBNetworkManager.Instance.StartClient();
			DisconnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = "CONNECTING... (STOP)";

			SetButtonActive(DisconnectButton, true);
			SetButtonActive(ClientButton, false);

			if (QSBSceneManager.CurrentScene == OWScene.TitleScreen)
			{
				SetButtonActive(ResumeGameButton, false);
				SetButtonActive(NewGameButton, false);
			}
		}

		private void OnConnected()
		{
			var text = QSBCore.IsHost
				? "STOP HOSTING"
				: "DISCONNECT";
			DisconnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = text;
		}

		public void OnKicked(KickReason reason)
		{
			var text = reason switch
			{
				KickReason.QSBVersionNotMatching => "Server refused connection as QSB version does not match.",
				KickReason.GameVersionNotMatching => "Server refused connection as Outer Wilds version does not match.",
				KickReason.GamePlatformNotMatching => "Server refused connection as Outer Wilds platform does not match. (Steam/Epic)",
				KickReason.None => "Kicked from server. No reason given.",
				_ => $"Kicked from server. KickReason:{reason}",
			};
			OpenInfoPopup(text, "OK");

			DisconnectButton.gameObject.SetActive(false);
			ClientButton.SetActive(true);
			HostButton?.gameObject.SetActive(true);
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

			DisconnectButton.gameObject.SetActive(false);
			ClientButton.SetActive(true);
			HostButton?.gameObject.SetActive(true);
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
					DisconnectButton.gameObject.SetActive(false);
					ClientButton.SetActive(true);
					HostButton?.gameObject.SetActive(true);
					break;
				default:
					text = $"Internal QNet client error!\n\nNetworkError:{error}";
					break;
			}

			OpenInfoPopup(text, "OK");
		}
	}
}
