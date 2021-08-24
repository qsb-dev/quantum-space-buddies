using UnityEngine;
using UnityEngine.UI;

namespace QSB.Menus
{
	class MenuManager : MonoBehaviour
	{
		private IMenuAPI MenuApi => QSBCore.MenuApi;
		private PopupMenu PopupMenu;
		private GameObject MultiplayerButton;
		private Button DisconnectButton;

		public void Start()
		{
			MakeTitleMenus();
		}

		private void MakeTitleMenus()
		{
			PopupMenu = MenuApi.MakeInputFieldPopup("IP Address", "IP Address", "Host a server", "Connect to server");
			PopupMenu.OnPopupConfirm += Host;
			PopupMenu.OnPopupCancel += Connect;

			MultiplayerButton = MenuApi.TitleScreen_MakeMenuOpenButton("MULTIPLAYER", PopupMenu);
			DisconnectButton = MenuApi.TitleScreen_MakeSimpleButton("DISCONNECT");
			DisconnectButton.gameObject.SetActive(false);
			DisconnectButton.onClick.AddListener(Disconnect);
		}

		private void Disconnect()
		{
			QSBNetworkManager.Instance.StopHost();
			DisconnectButton.gameObject.SetActive(false);
			MultiplayerButton.SetActive(true);
		}

		private void Host()
		{
			QSBNetworkManager.Instance.StartHost();
			DisconnectButton.gameObject.SetActive(true);
			MultiplayerButton.SetActive(false);
		}

		private void Connect()
		{
			QSBNetworkManager.Instance.networkAddress = (PopupMenu as PopupInputMenu).GetInputText();
			QSBNetworkManager.Instance.StartClient();
			DisconnectButton.gameObject.SetActive(true);
			MultiplayerButton.SetActive(false);
		}
	}
}