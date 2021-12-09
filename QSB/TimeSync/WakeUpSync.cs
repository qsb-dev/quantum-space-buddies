using OWML.Common;
using OWML.Utils;
using QSB.ClientServerStateSync;
using QSB.DeathSync;
using QSB.Events;
using QSB.Inputs;
using QSB.Player;
using QSB.TimeSync.Events;
using QSB.Utility;
using QuantumUNET;
using System;
using QSB.Messaging;
using QSB.Player.Events;
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

		public enum State { NotLoaded, Loaded, FastForwarding, Pausing }

		public State CurrentState { get; private set; } = State.NotLoaded;
		public Enum CurrentReason { get; private set; }

		private float _sendTimer;
		private float _serverTime;
		private int _serverLoopCount;
		private bool _hasWokenUp;

		public override void OnStartLocalPlayer() => LocalInstance = this;

		public void OnDisconnect()
		{
			OWTime.SetTimeScale(1f);
			OWTime.SetMaxDeltaTime(0.06666667f);
			OWTime.SetFixedTimestep(0.01666667f);
			Locator.GetActiveCamera().enabled = true;
			CurrentState = State.NotLoaded;
			CurrentReason = null;

			Physics.SyncTransforms();
			SpinnerUI.Hide();
			TimeSyncUI.Stop();

			QSBInputManager.Instance.SetInputsEnabled(true);
		}

		public void Start()
		{
			if (!IsLocalPlayer)
			{
				return;
			}

			if (QSBSceneManager.IsInUniverse)
			{
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

		private void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool isInUniverse)
		{
			_hasWokenUp = false;
			if (isInUniverse)
			{
				if (newScene == OWScene.EyeOfTheUniverse)
				{
					_hasWokenUp = true;
				}

				Init();
			}
			else
			{
				CurrentState = State.NotLoaded;
			}
		}

		private void Init()
		{
			new RequestStateResyncMessage().Send();
			CurrentState = State.Loaded;
			gameObject.GetAddComponent<PreserveTimeScale>();
			if (IsServer)
			{
				SendServerTime();
			}
			else
			{
				if (!QSBCore.SkipTitleScreen)
				{
					WakeUpOrSleep();
				}
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
			if (CurrentState == State.NotLoaded)
			{
				return;
			}

			if (PlayerData.LoadLoopCount() != _serverLoopCount && !IsServer)
			{
				DebugLog.ToConsole($"Warning - ServerLoopCount is not the same as local loop count! local:{PlayerData.LoadLoopCount()} server:{_serverLoopCount}");
				return;
			}

			var myTime = Time.timeSinceLevelLoad;
			var diff = myTime - _serverTime;

			if (diff > PauseOrFastForwardThreshold)
			{
				StartPausing(PauseReason.TooFarAhead);
				return;
			}

			if (diff < -PauseOrFastForwardThreshold)
			{
				StartFastForwarding(FastForwardReason.TooFarBehind);
			}
		}

		private void StartFastForwarding(FastForwardReason reason)
		{
			if (CurrentState == State.FastForwarding)
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

			CurrentState = State.FastForwarding;
			CurrentReason = reason;
			OWTime.SetMaxDeltaTime(0.033333335f);
			OWTime.SetFixedTimestep(0.033333335f);
			TimeSyncUI.TargetTime = _serverTime;
			TimeSyncUI.Start(TimeSyncType.Fastforwarding, FastForwardReason.TooFarBehind);
		}

		private void StartPausing(PauseReason reason)
		{
			if (CurrentState == State.Pausing)
			{
				return;
			}

			DebugLog.DebugWrite($"START PAUSING (Target:{_serverTime} Current:{Time.timeSinceLevelLoad})", MessageType.Info);
			Locator.GetActiveCamera().enabled = false;

			//OWInput.ChangeInputMode(InputMode.None);
			QSBInputManager.Instance.SetInputsEnabled(false);

			OWTime.SetTimeScale(0f);
			CurrentState = State.Pausing;
			CurrentReason = reason;
			SpinnerUI.Show();
			TimeSyncUI.Start(TimeSyncType.Pausing, reason);
		}

		private void ResetTimeScale()
		{
			OWTime.SetTimeScale(1f);
			OWTime.SetMaxDeltaTime(0.06666667f);
			OWTime.SetFixedTimestep(0.01666667f);
			Locator.GetActiveCamera().enabled = true;
			CurrentState = State.Loaded;
			CurrentReason = null;

			DebugLog.DebugWrite($"RESET TIMESCALE", MessageType.Info);
			Physics.SyncTransforms();
			SpinnerUI.Hide();
			TimeSyncUI.Stop();
			new RequestStateResyncMessage().Send();
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
			else if (IsLocalPlayer && !QSBCore.AvoidTimeSync)
			{
				UpdateClient();
			}
		}

		private void UpdateServer()
		{
			_serverTime = Time.timeSinceLevelLoad;

			if (ServerStateManager.Instance == null)
			{
				DebugLog.ToConsole($"Warning - ServerStateManager.Instance is null!", MessageType.Warning);
				return;
			}

			if (QSBPlayerManager.LocalPlayer == null)
			{
				DebugLog.ToConsole($"Warning - LocalPlayer is null!", MessageType.Warning);
				return;
			}

			var serverState = ServerStateManager.Instance.GetServerState();
			var clientState = QSBPlayerManager.LocalPlayer.State;

			if (serverState == ServerState.WaitingForAllPlayersToReady && clientState == ClientState.WaitingForOthersToReadyInSolarSystem)
			{
				if (CurrentState != State.Pausing)
				{
					DebugLog.DebugWrite($"Wait for other clients to be ready");
					StartPausing(PauseReason.WaitingForAllPlayersToBeReady);
				}
			}

			if (CurrentState == State.Pausing && (PauseReason)CurrentReason == PauseReason.WaitingForAllPlayersToBeReady)
			{
				if (clientState == ClientState.AliveInSolarSystem && serverState == ServerState.InSolarSystem)
				{
					DebugLog.DebugWrite($"start of new loop!");
					ResetTimeScale();
				}
			}

			if (serverState == ServerState.WaitingForAllPlayersToDie && clientState == ClientState.WaitingForOthersToReadyInSolarSystem)
			{
				if (CurrentState == State.Pausing && (PauseReason)CurrentReason == PauseReason.WaitingForAllPlayersToBeReady)
				{
					//?
					DebugLog.ToConsole($"Warning - Server waiting for players to die, but players waiting for ready signal! Assume players correct.", MessageType.Warning);
					QSBEventManager.FireEvent(EventNames.QSBServerState, ServerState.WaitingForAllPlayersToReady);
				}
			}

			if (CurrentState != State.Loaded)
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

		private void UpdateClient()
		{
			_serverTime += Time.unscaledDeltaTime;

			var serverState = ServerStateManager.Instance.GetServerState();
			var clientState = QSBPlayerManager.LocalPlayer.State;
			var currentScene = QSBSceneManager.CurrentScene;

			// set fastforwarding timescale

			if (CurrentState == State.FastForwarding)
			{
				if (Locator.GetPlayerCamera() != null && !Locator.GetPlayerCamera().enabled)
				{
					Locator.GetPlayerCamera().enabled = false;
				}

				var diff = _serverTime - Time.timeSinceLevelLoad;
				OWTime.SetTimeScale(Mathf.SmoothStep(MinFastForwardSpeed, MaxFastForwardSpeed, Mathf.Abs(diff) / MaxFastForwardDiff));

				TimeSyncUI.TargetTime = _serverTime;
			}

			if (CurrentState != State.Loaded && CurrentState != State.NotLoaded && CurrentReason == null)
			{
				DebugLog.ToConsole($"Warning - CurrentReason is null.", MessageType.Warning);
			}

			// Checks to pause/fastforward

			if (clientState is ClientState.NotLoaded or ClientState.InTitleScreen)
			{
				return;
			}

			if (serverState == ServerState.NotLoaded && CurrentState != State.Pausing && QSBSceneManager.IsInUniverse)
			{
				DebugLog.DebugWrite($"Server Not Loaded");
				StartPausing(PauseReason.ServerNotStarted);
			}

			if (serverState == ServerState.WaitingForAllPlayersToReady && CurrentState != State.Pausing && clientState == ClientState.WaitingForOthersToReadyInSolarSystem)
			{
				DebugLog.DebugWrite($"Awaiting Play Confirmation");
				StartPausing(PauseReason.WaitingForAllPlayersToBeReady);
			}

			if (serverState == ServerState.InSolarSystem && (clientState == ClientState.WaitingForOthersToReadyInSolarSystem || clientState == ClientState.WaitingForOthersToDieInSolarSystem))
			{
				DebugLog.DebugWrite($"Server is still running game normally, but this player has died from an accepted death!", MessageType.Warning);
			}

			if (serverState == ServerState.WaitingForAllPlayersToDie && clientState == ClientState.WaitingForOthersToReadyInSolarSystem)
			{
				DebugLog.DebugWrite($"Wait for others to load new scene");
				StartPausing(PauseReason.WaitingForAllPlayersToBeReady);
			}

			// Checks to revert to normal

			if (CurrentState == State.Pausing && (PauseReason)CurrentReason == PauseReason.ServerNotStarted)
			{
				if (serverState != ServerState.NotLoaded)
				{
					DebugLog.DebugWrite($"Server started!");
					ResetTimeScale();
				}
			}

			if (CurrentState == State.Pausing && (PauseReason)CurrentReason == PauseReason.WaitingForAllPlayersToBeReady)
			{
				if (clientState == ClientState.AliveInSolarSystem && serverState == ServerState.InSolarSystem)
				{
					DebugLog.DebugWrite($"start of new loop!");
					ResetTimeScale();
				}
			}

			if (CurrentState == State.Pausing && (PauseReason)CurrentReason == PauseReason.TooFarAhead)
			{
				if (Time.timeSinceLevelLoad <= _serverTime)
				{
					DebugLog.DebugWrite($"Done pausing to match time!");
					ResetTimeScale();
				}
			}

			if (CurrentState == State.FastForwarding && (FastForwardReason)CurrentReason == FastForwardReason.TooFarBehind)
			{
				if (Time.timeSinceLevelLoad >= _serverTime)
				{
					DebugLog.DebugWrite($"Done fast-forwarding to match time!");
					ResetTimeScale();
				}
			}

			if (CurrentState == State.Loaded)
			{
				CheckTimeDifference();
			}
		}

		private void CheckTimeDifference()
		{
			var diff = GetTimeDifference();

			if (diff is > PauseOrFastForwardThreshold or < (-PauseOrFastForwardThreshold))
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