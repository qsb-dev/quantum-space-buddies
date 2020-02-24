using System.Linq;
using OWML.ModHelper.Events;
using QSB.Messaging;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace QSB.TimeSync
{
    public class WakeUpSync : NetworkBehaviour
    {
        private MessageHandler<WakeUpMessage> _wakeUpHandler;

        private bool _hasServerTime;
        private float _serverTime;
        private bool _isSleeping;
        private bool _isLoaded;
        private Campfire _campfire;

        private void Start()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            DebugLog.Screen("Start WakeUpSync");
            GlobalMessenger.AddListener("WakeUp", OnWakeUp);
            SceneManager.sceneLoaded += OnSceneLoaded;

            _wakeUpHandler = new MessageHandler<WakeUpMessage>();
            _wakeUpHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _wakeUpHandler.OnServerReceiveMessage += OnServerReceiveMessage;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "SolarSystem")
            {
                _campfire = GameObject.FindObjectsOfType<Campfire>().Single(x => x.GetValue<Sector>("_sector").name == "Sector_Village");
                _isLoaded = true;
            }
        }

        private void OnWakeUp()
        {
            if (isServer)
            {
                SendServerTime();
            }
            else
            {
                WakeUpOrSleep();
            }
        }

        private void SendServerTime()
        {
            DebugLog.Screen("Sending wakeup to all my friends");
            var message = new WakeUpMessage
            {
                ServerTime = Time.timeSinceLevelLoad
            };
            _wakeUpHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(WakeUpMessage message)
        {
            if (isServer)
            {
                return;
            }
            _serverTime = message.ServerTime;
            _hasServerTime = true;
            WakeUpOrSleep();
        }

        private void OnServerReceiveMessage(WakeUpMessage obj)
        {
            DebugLog.Screen("Someone (should identify who) requested server time");
            SendServerTime();
        }

        private void WakeUpOrSleep()
        {
            if (!_hasServerTime)
            {
                DebugLog.Screen("I joined after server woke up, requesting server time");
                _wakeUpHandler.SendToServer(new WakeUpMessage());
                return;
            }

            if (!_isLoaded)
            {
                DebugLog.Screen("I haven't loaded!");
                return;
            }

            var myTime = Time.timeSinceLevelLoad;
            var diff = _serverTime - myTime;

            DebugLog.Screen($"My time ({myTime}) is {diff} behind server ({_serverTime})");

            if (diff < 0)
            {
                DebugLog.Screen("Somehow I'm AHEAD of server... how? :(");
                return;
            }

            if (diff < 1)
            {
                DebugLog.Screen("Less than a sec behind the server, waking up now!");
                WakeUp();
                return;
            }

            StartSleeping();
        }

        private void WakeUp()
        {
            // I copied all of this from my AutoResume mod, since that already wakes up the player instantly.
            // There must be a simpler way to do this though, I just couldn't find it.

            // Skip wake up animation.
            var cameraEffectController = FindObjectOfType<PlayerCameraEffectController>();
            cameraEffectController.OpenEyes(0, true);
            cameraEffectController.SetValue("_wakeLength", 0f);
            cameraEffectController.SetValue("_waitForWakeInput", false);

            // Skip wake up prompt.
            LateInitializerManager.pauseOnInitialization = false;
            Locator.GetPauseCommandListener().RemovePauseCommandLock();
            Locator.GetPromptManager().RemoveScreenPrompt(cameraEffectController.GetValue<ScreenPrompt>("_wakePrompt"));
            OWTime.Unpause(OWTime.PauseType.Sleeping);
            cameraEffectController.Invoke("WakeUp");

            // Enable all inputs immediately.
            OWInput.ChangeInputMode(InputMode.Character);
            typeof(OWInput).SetValue("_inputFadeFraction", 0f);
            GlobalMessenger.FireEvent("TakeFirstFlashbackSnapshot");
        }

        private void StartSleeping()
        {
            DebugLog.Screen("Starting sleeping");
            _campfire.Invoke("StartSleeping");
            _isSleeping = true;
        }

        private void StopSleeping()
        {
            DebugLog.Screen("Stopping sleeping");
            _campfire.StopSleeping();
            _isSleeping = false;
        }

        private void Update()
        {
            _serverTime += Time.unscaledDeltaTime;

            if (!_isLoaded)
            {
                return;
            }

            DebugLog.Screen(Time.timeSinceLevelLoad);

            if (_isSleeping)
            {
                if (Time.timeSinceLevelLoad > _serverTime)
                {
                    StopSleeping();
                }
            }
        }

    }
}
