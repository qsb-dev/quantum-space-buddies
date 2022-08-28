using Mirror;
using OWML.Common;
using QSB.ClientServerStateSync;
using QSB.ClientServerStateSync.Messages;
using QSB.DeathSync;
using QSB.Inputs;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Player.Messages;
using QSB.TimeSync.Messages;
using QSB.Utility;
using QSB.WorldSync;
using System;
using UnityEngine;

namespace QSB.TimeSync;

/// <summary>
/// BUG: this runs on remote players = BAD! can we move this off of network player?
/// </summary>
[UsedInUnityProject]
public class WakeUpSync : NetworkBehaviour
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
		if (!isLocalPlayer)
		{
			return;
		}

		if (QSBSceneManager.IsInUniverse)
		{
			Init();
		}

		QSBSceneManager.OnSceneLoaded += OnSceneLoaded;

		GlobalMessenger.AddListener(OWEvents.WakeUp, OnWakeUp);
	}

	public float GetTimeDifference()
	{
		var myTime = Time.timeSinceLevelLoad;
		return myTime - _serverTime;
	}

	private void OnWakeUp()
	{
		DebugLog.DebugWrite($"OnWakeUp", MessageType.Info);
		if (QSBCore.IsHost)
		{
			RespawnOnDeath.Instance.Init();
		}

		_hasWokenUp = true;
	}

	public void OnDestroy()
	{
		QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
		GlobalMessenger.RemoveListener(OWEvents.WakeUp, OnWakeUp);
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
		gameObject.GetRequiredComponent<StopMeditation>().Init();
		if (isServer)
		{
			SendServerTime();
		}
		else
		{
			if (!QSBCore.DebugSettings.AvoidTimeSync)
			{
				WakeUpOrSleep();
			}
			else
			{
				// dont bother sleeping, just wake up
				if (!_hasWokenUp)
				{
					Delay.RunWhen(() => QSBWorldSync.AllObjectsReady, WakeUp);
				}
			}
		}
	}

	private void SendServerTime()
		=> new ServerTimeMessage(_serverTime, PlayerData.LoadLoopCount(), TimeLoop.GetSecondsRemaining()).Send();

	public void OnClientReceiveMessage(float time, int count, float secondsRemaining)
	{
		_serverTime = time;
		_serverLoopCount = count;
		// prevents accidental supernova at start of loop
		if (_serverLoopCount == PlayerData.LoadLoopCount())
		{
			QSBPatch.RemoteCall(() => TimeLoop.SetSecondsRemaining(secondsRemaining));
		}
	}

	private void WakeUpOrSleep()
	{
		if (CurrentState == State.NotLoaded)
		{
			return;
		}

		if (PlayerData.LoadLoopCount() != _serverLoopCount && !isServer)
		{
			DebugLog.ToConsole($"Warning - ServerLoopCount is not the same as local loop count! local:{PlayerData.LoadLoopCount()} server:{_serverLoopCount}");
			return;
		}

		var myTime = Time.timeSinceLevelLoad;
		var diff = myTime - _serverTime;

		if (ServerStateManager.Instance.GetServerState() is not (ServerState.InSolarSystem or ServerState.InEye))
		{
			return;
		}

		if (diff > PauseOrFastForwardThreshold)
		{
			StartPausing(PauseReason.TooFarAhead);
		}
		else if (diff < -PauseOrFastForwardThreshold)
		{
			StartFastForwarding(FastForwardReason.TooFarBehind);
		}
		else
		{
			// should only happen from Init so we gotta wait
			if (!_hasWokenUp)
			{
				Delay.RunWhen(() => QSBWorldSync.AllObjectsReady, WakeUp);
			}
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
		TimeSyncUI.Start(TimeSyncType.FastForwarding, reason);
	}

	private void StartPausing(PauseReason reason)
	{
		if (CurrentState == State.Pausing)
		{
			TimeSyncUI.TargetTime = _serverTime;
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
		TimeSyncUI.TargetTime = _serverTime;
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
		=> Locator.GetPlayerCamera().GetComponent<PlayerCameraEffectController>().WakeUp();

	public void Update()
	{
		if (isServer)
		{
			UpdateServer();
		}
		else if (isLocalPlayer && !QSBCore.DebugSettings.AvoidTimeSync)
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

		if (serverState == ServerState.WaitingForAllPlayersToReady && clientState == ClientState.WaitingForOthersToBeReady)
		{
			if (CurrentState != State.Pausing)
			{
				StartPausing(PauseReason.WaitingForAllPlayersToBeReady);
			}
		}

		if (CurrentState == State.Pausing && (PauseReason)CurrentReason == PauseReason.WaitingForAllPlayersToBeReady)
		{
			if ((clientState == ClientState.AliveInSolarSystem && serverState == ServerState.InSolarSystem) ||
				(clientState == ClientState.AliveInEye && serverState == ServerState.InEye))
			{
				ResetTimeScale();
			}
		}

		if (serverState == ServerState.WaitingForAllPlayersToDie && clientState == ClientState.WaitingForOthersToBeReady)
		{
			if (CurrentState == State.Pausing && (PauseReason)CurrentReason == PauseReason.WaitingForAllPlayersToBeReady)
			{
				//?
				DebugLog.ToConsole($"Warning - Server waiting for players to die, but players waiting for ready signal! Assume players correct.", MessageType.Warning);
				new ServerStateMessage(ServerState.WaitingForAllPlayersToReady).Send();
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

		if (CurrentState == State.FastForwarding && (FastForwardReason)CurrentReason == FastForwardReason.TooFarBehind)
		{
			if (Locator.GetPlayerCamera() != null && !Locator.GetPlayerCamera().enabled)
			{
				Locator.GetPlayerCamera().enabled = false;
			}

			var diff = _serverTime - Time.timeSinceLevelLoad;
			OWTime.SetTimeScale(Mathf.SmoothStep(MinFastForwardSpeed, MaxFastForwardSpeed, Mathf.Abs(diff) / MaxFastForwardDiff));

			TimeSyncUI.TargetTime = _serverTime;
		}

		if (CurrentState == State.Pausing && (PauseReason)CurrentReason == PauseReason.TooFarAhead)
		{
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
			StartPausing(PauseReason.ServerNotStarted);
		}

		if (serverState == ServerState.WaitingForAllPlayersToReady && CurrentState != State.Pausing && clientState == ClientState.WaitingForOthersToBeReady)
		{
			StartPausing(PauseReason.WaitingForAllPlayersToBeReady);
		}

		if (serverState == ServerState.WaitingForAllPlayersToDie && CurrentState != State.Pausing && clientState == ClientState.WaitingForOthersToBeReady)
		{
			StartPausing(PauseReason.WaitingForAllPlayersToBeReady);
		}

		// Checks to revert to normal

		if (CurrentState == State.Pausing && (PauseReason)CurrentReason == PauseReason.ServerNotStarted)
		{
			if (serverState != ServerState.NotLoaded)
			{
				ResetTimeScale();
			}
		}

		if (CurrentState == State.Pausing && (PauseReason)CurrentReason == PauseReason.WaitingForAllPlayersToBeReady)
		{
			if ((clientState == ClientState.AliveInSolarSystem && serverState == ServerState.InSolarSystem) ||
				(clientState == ClientState.AliveInEye && serverState == ServerState.InEye))
			{
				ResetTimeScale();
			}
		}

		if (CurrentState == State.Pausing && (PauseReason)CurrentReason == PauseReason.TooFarAhead)
		{
			if (Time.timeSinceLevelLoad <= _serverTime)
			{
				ResetTimeScale();
			}
		}

		if (CurrentState == State.FastForwarding && (FastForwardReason)CurrentReason == FastForwardReason.TooFarBehind)
		{
			if (Time.timeSinceLevelLoad >= _serverTime)
			{
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

		if (diff is > PauseOrFastForwardThreshold or < -PauseOrFastForwardThreshold)
		{
			WakeUpOrSleep();
			return;
		}

		var mappedTimescale = diff.Map(-PauseOrFastForwardThreshold, PauseOrFastForwardThreshold, 1 + TimescaleBounds, 1 - TimescaleBounds, true);
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
