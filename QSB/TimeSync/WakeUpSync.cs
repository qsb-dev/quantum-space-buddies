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
        private const float MaxFastForwardSpeed = 60f;
        private const float MaxFastForwardDiff = 20f;
        private const float MinFastForwardSpeed = 2f;

        private enum State { NotLoaded, Loaded, FastForwarding, Pausing }
        private State _state = State.NotLoaded;

        private MessageHandler<WakeUpMessage> _wakeUpHandler;

        private float _sendTimer;
        private float _serverTime;
        private float _timeScale;
        private bool _isInputEnabled = true;
        private int _localLoopCount;
        private int _serverLoopCount;

        private void Start()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            _wakeUpHandler = new MessageHandler<WakeUpMessage>();
            _wakeUpHandler.OnClientReceiveMessage += OnClientReceiveMessage;

            var sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "SolarSystem" || sceneName == "EyeOfTheUniverse")
            {
                Init();
            }
            else
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
            }

            GlobalMessenger.AddListener("RestartTimeLoop", OnLoopStart);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "SolarSystem" || scene.name == "EyeOfTheUniverse")
            {
                Init();
            }
            else
            {
                Reset();
            }
        }

        private void OnLoopStart()
        {
            _localLoopCount++;
        }

        private void Init()
        {
            Marshmallow.Main.CreatePlanet();

            _state = State.Loaded;
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

        private void Reset()
        {
            _state = State.NotLoaded;
        }

        private void SendServerTime()
        {
            var message = new WakeUpMessage
            {
                ServerTime = Time.timeSinceLevelLoad,
                LoopCount = _localLoopCount
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
            _serverLoopCount = message.LoopCount;
            WakeUpOrSleep();
        }

        private void WakeUpOrSleep()
        {
            if (_state == State.NotLoaded || _localLoopCount != _serverLoopCount)
            {
                return;
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
                StartFastForwarding(diff);
                return;
            }
        }

        private void StartFastForwarding(float diff)
        {
            if (_state == State.FastForwarding)
            {
                return;
            }
            _timeScale = MaxFastForwardSpeed;
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
            _state = State.Loaded;

            if (!_isInputEnabled)
            {
                EnableInput();
            }
        }

        private void DisableInput()
        {
            _isInputEnabled = false;
            OWInput.ChangeInputMode(InputMode.None);
        }

        private void EnableInput()
        {
            _isInputEnabled = true;
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
            if (_state != State.Loaded)
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

            if (_state == State.NotLoaded)
            {
                return;
            }

            if (_state == State.FastForwarding)
            {
                var diff = _serverTime - Time.timeSinceLevelLoad;
                Time.timeScale = Mathf.Lerp(MinFastForwardSpeed, MaxFastForwardSpeed, Mathf.Abs(diff) / MaxFastForwardDiff);
            }
            else
            {
                Time.timeScale = _timeScale;
            }

            bool isDoneFastForwarding = _state == State.FastForwarding && Time.timeSinceLevelLoad >= _serverTime;
            bool isDonePausing = _state == State.Pausing && Time.timeSinceLevelLoad < _serverTime;

            if (isDoneFastForwarding || isDonePausing)
            {
                ResetTimeScale();
            }

            if (!_isInputEnabled && OWInput.GetInputMode() != InputMode.None)
            {
                DisableInput();
            }
        }

    }
}
