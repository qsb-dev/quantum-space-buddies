using OWML.Common;
using OWML.Utils;
using QSB.DeathSync;
using QSB.Events;
using QSB.Inputs;
using QSB.TimeSync.Events;
using QSB.Utility;
using QuantumUNET;
using UnityEngine;

namespace QSB.TimeSync
{
	public class WakeUpSync : QNetworkBehaviour
	{
		public static WakeUpSync LocalInstance { get; private set; }

		private const float PauseOrFastForwardThreshold = 1.0f;
		private const float TimescaleBounds = 0.3f;

		private const float MaxFastForwardSpeed = 60f;
		private const float MaxFastForwardDiff = 20f;
		private const float MinFastForwardSpeed = 2f;

		private enum State { NotLoaded, Loaded, FastForwarding, Pausing, WaitingForServerToDie }

		private State _state = State.NotLoaded;

		private float _sendTimer;
		private float _serverTime;
		private bool _isFirstFastForward = true;
		private int _serverLoopCount;
		private bool _hasWokenUp;

		public override void OnStartLocalPlayer() => LocalInstance = this;

		public void Start()
		{
			if (!IsLocalPlayer)
			{
				return;
			}

			if (QSBSceneManager.IsInUniverse)
			{
				_isFirstFastForward = false;
				Init();
			}

			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;

			GlobalMessenger.AddListener(EventNames.WakeUp, OnWakeUp);
		}

		public float GetTimeDifference()
		{
			var myTime = Time.timeSinceLevelLoad;
			return myTime - _serverTime;
		}

		private void OnWakeUp()
		{
			DebugLog.DebugWrite($"OnWakeUp", MessageType.Info);
			if (QNetworkServer.active)
			{
				RespawnOnDeath.Instance.Init();
			}

			_hasWokenUp = true;
		}

		public void OnDestroy()
		{
			QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
			GlobalMessenger.RemoveListener(EventNames.WakeUp, OnWakeUp);
		}

		private void OnSceneLoaded(OWScene scene, bool isInUniverse)
		{
			_hasWokenUp = false;
			if (isInUniverse)
			{
				if (scene == OWScene.EyeOfTheUniverse)
				{
					_hasWokenUp = true;
				}

				Init();
			}
			else
			{
				_state = State.NotLoaded;
			}
		}

		private void Init()
		{
			QSBEventManager.FireEvent(EventNames.QSBPlayerStatesRequest);
			_state = State.Loaded;
			gameObject.GetAddComponent<PreserveTimeScale>();
			if (IsServer)
			{
				SendServerTime();
			}
			else
			{
				WakeUpOrSleep();
			}
		}

		private void SendServerTime()
			=> QSBEventManager.FireEvent(EventNames.QSBServerTime, _serverTime, PlayerData.LoadLoopCount());

		public void OnClientReceiveMessage(ServerTimeMessage message)
		{
			_serverTime = message.ServerTime;
			_serverLoopCount = message.LoopCount;
		}

		private void WakeUpOrSleep()
		{
			if (_state == State.NotLoaded)
			{
				return;
			}

			if (PlayerData.LoadLoopCount() != _serverLoopCount)
			{
				DebugLog.ToConsole($"Warning - ServerLoopCount is not the same as local loop count! local:{PlayerData.LoadLoopCount()} server:{_serverLoopCount}");
				StartWaitingForServerToDie();
				return;
			}

			var myTime = Time.timeSinceLevelLoad;
			var diff = myTime - _serverTime;

			if (diff > PauseOrFastForwardThreshold)
			{
				StartPausing();
				return;
			}

			if (diff < -PauseOrFastForwardThreshold)
			{
				StartFastForwarding();
			}
		}

		private void StartWaitingForServerToDie()
		{
			if (_state == State.WaitingForServerToDie)
			{
				return;
			}

			DebugLog.DebugWrite($"START WAITING FOR SERVER LOOP", MessageType.Info);
			OWTime.SetTimeScale(0f);
			_state = State.WaitingForServerToDie;
			TimeSyncUI.Start(TimeSyncType.WaitForServerLoop);
		}

		private void StartFastForwarding()
		{
			if (_state == State.FastForwarding)
			{
				TimeSyncUI.TargetTime = _serverTime;
				return;
			}

			DebugLog.DebugWrite($"START FASTFORWARD (Target:{_serverTime} Current:{Time.timeSinceLevelLoad})", MessageType.Info);
			if (Locator.GetActiveCamera() != null)
			{
				Locator.GetActiveCamera().enabled = false;
			}

			//OWInput.ChangeInputMode(InputMode.None);
			QSBInputManager.Instance.SetInputsEnabled(false);

			_state = State.FastForwarding;
			OWTime.SetMaxDeltaTime(0.033333335f);
			OWTime.SetFixedTimestep(0.033333335f);
			TimeSyncUI.TargetTime = _serverTime;
			TimeSyncUI.Start(TimeSyncType.Fastforwarding);
		}

		private void StartPausing()
		{
			if (_state == State.Pausing)
			{
				return;
			}

			DebugLog.DebugWrite($"START PAUSING (Target:{_serverTime} Current:{Time.timeSinceLevelLoad})", MessageType.Info);
			Locator.GetActiveCamera().enabled = false;

			//OWInput.ChangeInputMode(InputMode.None);
			QSBInputManager.Instance.SetInputsEnabled(false);

			OWTime.SetTimeScale(0f);
			_state = State.Pausing;
			SpinnerUI.Show();
			TimeSyncUI.Start(TimeSyncType.Pausing);
		}

		private void ResetTimeScale()
		{
			OWTime.SetTimeScale(1f);
			OWTime.SetMaxDeltaTime(0.06666667f);
			OWTime.SetFixedTimestep(0.01666667f);
			Locator.GetActiveCamera().enabled = true;
			_state = State.Loaded;

			DebugLog.DebugWrite($"RESET TIMESCALE", MessageType.Info);
			_isFirstFastForward = false;
			Physics.SyncTransforms();
			SpinnerUI.Hide();
			TimeSyncUI.Stop();
			QSBEventManager.FireEvent(EventNames.QSBPlayerStatesRequest);
			RespawnOnDeath.Instance.Init();

			QSBInputManager.Instance.SetInputsEnabled(true);

			if (!_hasWokenUp)
			{
				WakeUp();
			}
		}

		private void WakeUp()
			=> Locator.GetPlayerCamera().GetComponent<PlayerCameraEffectController>().Invoke("WakeUp");

		public void Update()
		{
			if (IsServer)
			{
				UpdateServer();
			}
			else if (IsLocalPlayer)
			{
				UpdateLocal();
			}
		}

		private void UpdateServer()
		{
			_serverTime = Time.timeSinceLevelLoad;
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
				if (Locator.GetPlayerCamera() != null && !Locator.GetPlayerCamera().enabled)
				{
					Locator.GetPlayerCamera().enabled = false;
				}

				var diff = _serverTime - Time.timeSinceLevelLoad;
				OWTime.SetTimeScale(Mathf.SmoothStep(MinFastForwardSpeed, MaxFastForwardSpeed, Mathf.Abs(diff) / MaxFastForwardDiff));

				TimeSyncUI.TargetTime = _serverTime;

				if (QSBSceneManager.CurrentScene == OWScene.SolarSystem && _isFirstFastForward)
				{
					var spawnPoint = Locator.GetPlayerBody().GetComponent<PlayerSpawner>().GetInitialSpawnPoint().transform;
					Locator.GetPlayerTransform().position = spawnPoint.position;
					Locator.GetPlayerTransform().rotation = spawnPoint.rotation;
					Physics.SyncTransforms();
				}
			}

			var isDoneFastForwarding = _state == State.FastForwarding && Time.timeSinceLevelLoad >= _serverTime;
			var isDonePausing = _state == State.Pausing && Time.timeSinceLevelLoad < _serverTime;
			var serverMatchesLoop = _state == State.WaitingForServerToDie && _serverLoopCount == PlayerData.LoadLoopCount();

			if (isDoneFastForwarding || isDonePausing || serverMatchesLoop)
			{
				ResetTimeScale();
			}

			if (_state == State.Loaded)
			{
				CheckTimeDifference();
			}
		}

		private void CheckTimeDifference()
		{
			var diff = GetTimeDifference();

			if (diff > PauseOrFastForwardThreshold || diff < -PauseOrFastForwardThreshold)
			{
				WakeUpOrSleep();
				return;
			}

			var mappedTimescale = diff.Map(-PauseOrFastForwardThreshold, PauseOrFastForwardThreshold, 1 + TimescaleBounds, 1 - TimescaleBounds);
			if (mappedTimescale > 100f)
			{
				DebugLog.ToConsole($"Warning - CheckTimeDifference() returned over 100 - should have switched into fast-forward!", MessageType.Warning);
				mappedTimescale = 0f;
			}

			if (mappedTimescale < 0)
			{
				DebugLog.ToConsole($"Warning - CheckTimeDifference() returned below 0 - should have switched into pausing!", MessageType.Warning);
				mappedTimescale = 0f;
			}

			OWTime.SetTimeScale(mappedTimescale);
		}
	}
}