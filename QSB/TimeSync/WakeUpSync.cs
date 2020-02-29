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
        private const float TimeThreshold = 0.5f;

        private enum State { NotLoaded, EyesClosed, Awake, FastForwarding, Pausing }
        private State _state = State.NotLoaded;

        private MessageHandler<WakeUpMessage> _wakeUpHandler;

        private float _sendTimer;
        private float _serverTime;

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
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "SolarSystem")
            {
                _state = State.EyesClosed;
            }
        }

        private void OnWakeUp()
        {
            _state = State.Awake;
            gameObject.AddComponent<PreserveTimeScale>();
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
            DebugLog.Screen("Sending server time to all my friends: " + Time.timeSinceLevelLoad);
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
            WakeUpOrSleep();
        }

        private void WakeUpOrSleep()
        {
            if (_state == State.NotLoaded)
            {
                return;
            }

            if (_state == State.EyesClosed)
            {
                OpenEyes();
                _state = State.Awake;
            }

            var myTime = Time.timeSinceLevelLoad;
            var diff = myTime - _serverTime;

            if (diff > TimeThreshold)
            {
                DebugLog.Screen($"My time ({myTime}) is {diff} ahead server ({_serverTime})");
                StartPausing();
                return;
            }

            if (diff < -TimeThreshold)
            {
                DebugLog.Screen($"My time ({myTime}) is {-diff} behind server ({_serverTime})");
                StartFastForwarding();
                return;
            }

            DebugLog.Screen($"My time ({myTime}) is within threshold of server time ({_serverTime})");
        }

        private void OpenEyes()
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

        private void StartFastForwarding()
        {
            if (_state == State.FastForwarding)
            {
                return;
            }
            DebugLog.Screen("Starting sleeping");
            Time.timeScale = 10f;
            _state = State.FastForwarding;
        }

        private void StopFastForwarding()
        {
            if (_state != State.FastForwarding)
            {
                return;
            }
            DebugLog.Screen("Stopping sleeping");
            Time.timeScale = 1f;
            _state = State.Awake;
        }

        private void StartPausing()
        {
            if (_state == State.Pausing)
            {
                return;
            }
            Time.timeScale = 0f;
            _state = State.Pausing;
        }

        private void StopPausing()
        {
            if (_state != State.Pausing)
            {
                return;
            }
            Time.timeScale = 1f;
            _state = State.Awake;
        }

        private void Update()
        {
            if (isServer)
            {
                UpdateServer();
            }
            else if (isLocalPlayer)
            {
                UpdateLocal();
            }
        }

        private void UpdateServer()
        {
            if (_state != State.Awake)
            {
                return;
            }

            _sendTimer += Time.unscaledDeltaTime;
            if (_sendTimer > 1)
            {
                SendServerTime();
                _sendTimer = 0;
            }
        }

        private void UpdateLocal()
        {
            _serverTime += Time.unscaledDeltaTime;

            if (_state == State.NotLoaded || _state == State.EyesClosed)
            {
                return;
            }

            if (_state == State.FastForwarding && Time.timeSinceLevelLoad >= _serverTime)
            {
                StopFastForwarding();
            }
            else if (_state == State.Pausing && Time.timeSinceLevelLoad < _serverTime)
            {
                StopPausing();
            }
        }

    }
}
