using OWML.Common;
using OWML.Common.Menus;
using OWML.ModHelper;
using OWML.ModHelper.Menus;
using QSB.TimeSync;
using QSB.Menus;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;
using OWML.ModHelper.Events;
using System.Linq;
using System;



namespace QSB
{
    public class QSB : ModBehaviour
    {
        public static IModHelper Helper;
        public static string DefaultServerIP;
        public static bool DebugMode;

        private QSBNetworkManager networkManager;

        private void Awake()
        {
            Application.runInBackground = true;
        }

        private void Start()
        {
#if DEBUG
            // Debug, remove flash screen etc
            // Skip flash screen.
            var titleScreenAnimation = FindObjectOfType<TitleScreenAnimation>();
            titleScreenAnimation.SetValue("_fadeDuration", 0);
            titleScreenAnimation.SetValue("_gamepadSplash", false);
            titleScreenAnimation.SetValue("_introPan", false);
            titleScreenAnimation.Invoke("FadeInTitleLogo");

            // Skip menu fade.
            var titleAnimationController = FindObjectOfType<TitleAnimationController>();
            titleAnimationController.SetValue("_logoFadeDelay", 0.001f);
            titleAnimationController.SetValue("_logoFadeDuration", 0.001f);
            titleAnimationController.SetValue("_optionsFadeDelay", 0.001f);
            titleAnimationController.SetValue("_optionsFadeDuration", 0.001f);
            titleAnimationController.SetValue("_optionsFadeSpacing", 0.001f);
#endif


            Helper = ModHelper;

            gameObject.AddComponent<DebugLog>();
            networkManager = gameObject.AddComponent<QSBNetworkManager>();
#if DEBUG
            gameObject.AddComponent<NetworkManagerHUD>();

#endif
            gameObject.AddComponent<DebugActions>();
            var multiplayerMenuController = new MultiplayerMenuController(Helper, networkManager);

            //var multMenu = new LoadMultiplayerMenu();// (Helper.Logger, Helper.Console, Helper.Events);
            //DoMainMenuStuff();
            //ModHelper.Menus.PauseMenu.OnInit += DoPauseMenuStuff;
        }


        public override void Configure(IModConfig config)
        {
            DefaultServerIP = config.GetSettingsValue<string>("defaultServerIP");
            DebugMode = config.GetSettingsValue<bool>("debugMode");
        }

    }
}
