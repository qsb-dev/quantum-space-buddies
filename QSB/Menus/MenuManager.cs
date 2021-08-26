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

		private void InitPauseMenus()
		{
			PopupMenu = MenuApi.MakeInputFieldPopup("IP Address", "IP Address", "Connect", "Cancel");
			PopupMenu.OnPopupConfirm += Connect;

			InfoPopup = MenuApi.MakeInfoPopup("you forgot to set the text!!!", "you dumpty!!!");

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
			PopupMenu = MenuApi.MakeInputFieldPopup("IP Address", "IP Address", "Connect", "Cancel");
			PopupMenu.OnPopupConfirm += Connect;

			InfoPopup = MenuApi.MakeInfoPopup("you forgot to set the text!!!", "you dumpty!!!");

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

			InfoPopup.SetUpPopup($"Client Disconnected. Reason : {error}", InputLibrary.menuConfirm, InputLibrary.cancel, new ScreenPrompt("OK"), null, true, false);
			InfoPopup.EnableMenu(true);

			DisconnectButton.gameObject.SetActive(false);
			ClientButton.SetActive(true);
			HostButton.gameObject.SetActive(true);
		}
	}
}