using UnityEngine;
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

		public void Start()
		{
			MakeTitleMenus();
		}

		private void MakeTitleMenus()
		{
			PopupMenu = MenuApi.MakeInputFieldPopup("IP Address", "IP Address", "Connect", "Cancel");
			PopupMenu.OnPopupConfirm += Connect;

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
			DisconnectButton.gameObject.SetActive(true);
			DisconnectButton.GetComponent<CanvasGroup>().alpha = 1f;
			ClientButton.SetActive(false);
			HostButton.gameObject.SetActive(false);
		}
	}
}