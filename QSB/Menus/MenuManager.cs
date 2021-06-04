using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Menus
{
	class MenuManager : MonoBehaviour
	{
		private IMenuAPI MenuApi => QSBCore.MenuApi;
		private PopupMenu HostWaitingPopup;
		private PopupMenu ClientWaitingPopup;

		public void Start()
		{
			MakeTitleMenus();
		}

		private void MakeTitleMenus()
		{
			HostWaitingPopup = MenuApi.MakeTwoChoicePopup("Waiting for players to join...", "Start Multiplayer Game", "Stop Server");
			HostWaitingPopup.OnPopupCancel += StopServerOrLeaveServer;
			ClientWaitingPopup = MenuApi.MakeTwoChoicePopup("Waiting for game to start...", "uhhhhh", "Disconnect");
			ClientWaitingPopup.OnPopupCancel += StopServerOrLeaveServer;

			var hostButton = MenuApi.TitleScreen_MakeSimpleButton("HOST SERVER");
			hostButton.onClick.AddListener(HostServer);
			var connectButton = MenuApi.TitleScreen_MakeSimpleButton("CONNECT TO SERVER");
			connectButton.onClick.AddListener(ConnectToServer);

			var menu = MenuApi.OptionsMenu_MakeNonScrollingOptionsTab("MULTIPLAYER");
			//MenuApi.OptionsMenu_MakeLabel("Connection Information", menu);
			MenuApi.OptionsMenu_MakeTextInput("IP Address", "IP Address", QSBCore.DefaultServerIP, menu);
			MenuApi.OptionsMenu_MakeTextInput("Port", "Port", $"{QSBCore.Port}", menu);
		}

		private void HostServer()
		{
			HostWaitingPopup.EnableMenu(true);
			QSBNetworkManager.Instance.StartHost();
		}

		private void StopServerOrLeaveServer()
		{
			QSBNetworkManager.Instance.StopHost();
		}

		private void ConnectToServer()
		{
			ClientWaitingPopup.EnableMenu(true);
			QSBNetworkManager.Instance.StartClient();
		}
	}
}