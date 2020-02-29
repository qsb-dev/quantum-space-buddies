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
        private const float FastForwardSpeed = 10f;

        private enum State { NotLoaded, EyesClosed, Awake, FastForwarding, Pausing }
        private State _state = State.NotLoaded;

        private MessageHandler<WakeUpMessage> _wakeUpHandler;

        private float _sendTimer;
        private float _serverTime;
        private float _timeScale;
        private bool _isInputDisabled;

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

                if (!isServer)
                {
                    DisableInput();
                }
            }

            var myTime = Time.timeSinceLevelLoad;
            var diff = myTime - _serverTime;

            if (diff > TimeThreshold)
            {
                StartPausing();
                return;
            }

            if (diff < -TimeThreshold)
            {
                StartFastForwarding();
                return;
            }
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
        }

        private void StartFastForwarding()
        {
            if (_state == State.FastForwarding)
            {
                return;
            }
            _timeScale = FastForwardSpeed;
            _state = State.FastForwarding;
        }

        private void StartPausing()
        {
            if (_state == State.Pausing)
            {
                return;
            }
            _timeScale = 0f;
            _state = State.Pausing;
        }

        private void ResetTimeScale()
        {
            _timeScale = 1f;
            _state = State.Awake;

            if (_isInputDisabled)
            {
                EnableInput();
            }
        }

        private void DisableInput()
        {
            _isInputDisabled = true;
            OWInput.ChangeInputMode(InputMode.None);
        }

        private void EnableInput()
        {
            _isInputDisabled = false;
            OWInput.ChangeInputMode(InputMode.Character);
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

            bool isDoneFastForwarding = _state == State.FastForwarding && Time.timeSinceLevelLoad >= _serverTime;
            bool isDonePausing = _state == State.Pausing && Time.timeSinceLevelLoad < _serverTime;

            if (isDoneFastForwarding || isDonePausing)
            {
                ResetTimeScale();
            }

            Time.timeScale = _timeScale;

            if (_isInputDisabled && OWInput.GetInputMode() != InputMode.None)
            {
                DisableInput();
            }
        }

    }
}
