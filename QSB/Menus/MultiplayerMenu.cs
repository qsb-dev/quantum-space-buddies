using System.Linq;
using OWML.Common;
using OWML.Common.Menus;
using OWML.ModHelper.Menus;
using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.UI;

namespace QSB.Menus
{
  

    public class MultiplayerMenuController
    {
        private QSBNetworkManager networkManager;

        private IModTabMenu serverTab;

        private IModButton _buttonTemplate;

        private IModButton hostButton;
        private IModButton clientButton;
        private IModButton serverButton;

        private IModTextInput textInput;

        private enum SERVER_STATE
        {
            NONE,
            CLIENT,
            HOST,
            SERVER //lol
        };

        SERVER_STATE serverState = SERVER_STATE.NONE;

        private IModButton cancelButton;

        private IModHelper Helper;

        public MultiplayerMenuController(IModHelper helper, QSBNetworkManager networkmanager)
        {
            Helper = helper;
            networkManager = networkmanager;

            Helper.Menus.MainMenu.OnInit += CreateMenuTab;
        }

        private void CreateMenuTab()
        {
            var options = Helper.Menus.MainMenu.OptionsMenu;
            serverTab = options.InputTab.Copy("MULTIPLAYER");
            options.AddTab(serverTab);

            serverTab.Buttons.ForEach(x => x.Hide());
            serverTab.Menu.GetComponentsInChildren<Selectable>().ToList().ForEach(x => x.gameObject.SetActive(false));
            serverTab.Menu.GetValue<TooltipDisplay>("_tooltipDisplay").GetComponent<Text>().color = Color.clear;

            _buttonTemplate = options.InputTab.GetButton("UIElement-RemapControls");

            hostButton = _buttonTemplate.Copy("Host Server and Join");
            hostButton.Button.enabled = true;
            hostButton.OnClick += () => StartHost();
            
            clientButton = _buttonTemplate.Copy("Join Server");
            clientButton.Button.enabled = true;
            clientButton.OnClick += () => StartClient();
            
            serverButton = _buttonTemplate.Copy("Host Server Only");
            serverButton.Button.enabled = true;
            serverButton.OnClick += StartServer;
            
            cancelButton = _buttonTemplate.Copy("Cancel");
            cancelButton.Hide();
            cancelButton.OnClick += OnCancel;

            serverTab.AddButton(serverButton);
            serverTab.AddButton(clientButton);
            serverTab.AddButton(hostButton);
            serverTab.AddButton(cancelButton);

            ModTextInput _inputTemplate = new ModTextInput(options.InputTab.ToggleInputs[0].Toggle, serverTab, Helper.Menus.InputMenu);
            textInput = _inputTemplate.Copy("Name: ");
            textInput.Value = networkManager._playerName;
            serverTab.AddTextInput(textInput);

            textInput.OnChange += (value) => networkManager._playerName = value;

            var button = Helper.Menus.MainMenu.ResumeExpeditionButton.Duplicate("SET UP MULTIPLAYER", 1);
            Helper.Menus.MainMenu.SelectFirst();
            Helper.Menus.MainMenu.UpdateNavigation();
            button.OnClick += () => serverTab.Open();

            serverTab.SelectFirst();
            serverTab.UpdateNavigation();
        }


        private void StartHost()
        {
            networkManager.StartHost();
            DebugLog.Console("Starting Host");
            cancelButton.Title = "Stop Host";

            HideMainServerButtons();
            serverState = SERVER_STATE.HOST;
        }

        private void StartClient()
        {
            networkManager.StartClient();
            DebugLog.Console("Starting Client");
            cancelButton.Title = "Stop Client";

            HideMainServerButtons();
            serverState = SERVER_STATE.CLIENT;
        }

        private void StartServer()
        {
            networkManager.StartServer();
            DebugLog.Console("Starting Server");
            cancelButton.Title = "Stop Server";

            HideMainServerButtons();
            serverState = SERVER_STATE.SERVER;
        }

        private void OnCancel()
        {
            switch (serverState)
            {
                case SERVER_STATE.HOST:
                    {
                        networkManager.StopHost();
                        break;
                    }
                case SERVER_STATE.CLIENT:
                    {
                        networkManager.StopHost();
                        break;
                    }
                case SERVER_STATE.SERVER:
                    {
                        networkManager.StopServer();
                        break;
                    }
                default:
                    {
                        DebugLog.Console("Something went wrong. Cancel button clicked with no server state.");
                        break;
                    }
            }
            ShowMainServerButtons();
            serverState = SERVER_STATE.NONE;
        }

        private void HideMainServerButtons()
        {
            hostButton.Hide();
            clientButton.Hide();
            serverButton.Hide();
            textInput.Hide();

            cancelButton.Show();

            serverTab.SelectFirst();
            serverTab.UpdateNavigation();
        }

        private void ShowMainServerButtons()
        {
            hostButton.Show();
            clientButton.Show();
            serverButton.Show();
            textInput.Show();

            cancelButton.Hide();

            serverTab.SelectFirst();
            serverTab.UpdateNavigation();
        }
    }
}
