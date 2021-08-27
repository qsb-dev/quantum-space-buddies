using QSB.Utility;
using QuantumUNET;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace QSB.Menus
{
	class MenuManager : MonoBehaviour
	{
		private IMenuAPI MenuApi => QSBCore.MenuApi;
		private PopupMenu PopupMenu;
		private Button HostButton;
		private GameObject ClientButton;
		private Button DisconnectButton;
		private PopupMenu InfoPopup;
		private bool _addedPauseLock;

		public void Start()
		{
			MakeTitleMenus();
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
			QSBNetworkManager.Instance.OnClientConnected += OnConnected;
			QSBNetworkManager.Instance.OnClientDisconnected += OnDisconnected;
		}

		void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool isUniverse)
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

		private void InitPauseMenus()
		{
			CreateCommonPopups();

			HostButton = MenuApi.PauseMenu_MakeSimpleButton("MULTIPLAYER (HOST)");
			HostButton.onClick.AddListener(Host);

			ClientButton = MenuApi.PauseMenu_MakeMenuOpenButton("MULTIPLAYER (CONNECT)", PopupMenu);

			DisconnectButton = MenuApi.PauseMenu_MakeSimpleButton("DISCONNECT");
			DisconnectButton.onClick.AddListener(Disconnect);
			DisconnectButton.gameObject.SetActive(false);
			DisconnectButton.GetComponent<CanvasGroup>().alpha = 1f;
		}

		private void MakeTitleMenus()
		{
			CreateCommonPopups();

			HostButton = MenuApi.TitleScreen_MakeSimpleButton("MULTIPLAYER (HOST)");
			HostButton.onClick.AddListener(Host);

			ClientButton = MenuApi.TitleScreen_MakeMenuOpenButton("MULTIPLAYER (CONNECT)", PopupMenu);

			DisconnectButton = MenuApi.TitleScreen_MakeSimpleButton("DISCONNECT");
			DisconnectButton.onClick.AddListener(Disconnect);
			DisconnectButton.gameObject.SetActive(false);
			DisconnectButton.GetComponent<CanvasGroup>().alpha = 1f;
		}

		private void Disconnect()
		{
			QSBNetworkManager.Instance.StopHost();
			DisconnectButton.gameObject.SetActive(false);
			ClientButton.SetActive(true);
			HostButton.gameObject.SetActive(true);
		}

		private void Host()
		{
			QSBNetworkManager.Instance.StartHost();
			DisconnectButton.gameObject.SetActive(true);
			DisconnectButton.GetComponent<CanvasGroup>().alpha = 1f;
			ClientButton.SetActive(false);
			HostButton.gameObject.SetActive(false);
		}

		private void Connect()
		{
			QSBNetworkManager.Instance.networkAddress = (PopupMenu as PopupInputMenu).GetInputText();
			QSBNetworkManager.Instance.StartClient();
			DisconnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = "CONNECTING... (STOP)";
			DisconnectButton.gameObject.SetActive(true);
			DisconnectButton.GetComponent<CanvasGroup>().alpha = 1f;
			ClientButton.SetActive(false);
			HostButton.gameObject.SetActive(false);
		}

		private void OnConnected()
		{
			DebugLog.DebugWrite($"ON CONNECTED");
			DisconnectButton.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = "DISCONNECT";
		}

		private void OnDisconnected(NetworkError error)
		{
			if (error == NetworkError.Ok)
			{
				return;
			}

			OpenInfoPopup($"Client Disconnected. Reason : {error}", "OK");

			DisconnectButton.gameObject.SetActive(false);
			ClientButton.SetActive(true);
			HostButton.gameObject.SetActive(true);
		}
	}
}