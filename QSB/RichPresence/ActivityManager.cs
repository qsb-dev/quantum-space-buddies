using Discord;
using DiscordMirror;
using Mirror;
using QSB.ClientServerStateSync;
using QSB.Player;
using QSB.Utility;
using System;
using UnityEngine;

namespace QSB.RichPresence;

internal class ActivityManager : MonoBehaviour, IAddComponentOnStart
{
	public static ActivityManager Instance { get; private set; }

	private Discord.Discord _client;
	private Discord.ActivityManager _activityManager;
	private bool _activityHasBeenSet;
	private Activity _currentActivity;
	private long _endUnixEpoch;

	public void Start()
	{
		if (QSBCore.DebugSettings.UseKcpTransport)
		{
			Destroy(this);
			return;
		}

		Instance = this;

		QSBNetworkManager.singleton.OnClientConnected += OnClientConnected;
		QSBNetworkManager.singleton.OnClientDisconnected += OnClientDisconnected;
		QSBPlayerManager.OnAddPlayer += OnAddPlayer;
		QSBPlayerManager.OnRemovePlayer += OnRemovePlayer;

		Delay.RunWhen(() => ClientStateManager.Instance != null,
			() => ClientStateManager.Instance.OnChangeState += OnChangeClientState);
	}

	private void UpdateActivityCallback(Result result)
	{
		DebugLog.DebugWrite($"[UpdateActivityCallback] : {result}", result);
	}

	public void UpdateSecondsRemaining(long timeRemaining)
	{
		var currentEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		var end = currentEpoch + timeRemaining;

		if (Math.Abs(_endUnixEpoch - end) >= 5f)
		{
			DebugLog.DebugWrite($"Calculated epoch ({end}) is significantly different from stored epoch {currentEpoch}). Updating activity.");
			_currentActivity.Timestamps.End = end;
			_endUnixEpoch = end;
			DebugLog.DebugWrite("update activity - end epoch");
			_activityManager.UpdateActivity(_currentActivity, UpdateActivityCallback);
		}
	}

	private void OnClientConnected()
	{
		var discordTransport = (DiscordTransport)Transport.activeTransport;
		if (_client == default)
		{
			_client = discordTransport.discordClient;
			_activityManager = _client.GetActivityManager();
		}

		var activity = new Activity
		{
			State = "STATE!",
			Details = "Loading game...",
			Assets =
			{
				LargeImage = "qsbcoverimage",
				LargeText = "Quantum Space Buddies",
				SmallImage = "owventures",
				SmallText = "Unknown state."
			},
			Party =
			{
				Id = $"{discordTransport.currentLobby.Id}",
				Size =
				{
					MaxSize = 16
				}
			},
			Timestamps =
			{
				End = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			}
		};

		activity.State = QSBCore.IsHost
			? "Hosting server."
			: "Connected to server.";

		_activityHasBeenSet = true;
		_currentActivity = activity;
		DebugLog.DebugWrite("update activity - OnClientConnected");
		_activityManager.UpdateActivity(activity, UpdateActivityCallback);
	}

	private void OnClientDisconnected(string error)
	{
		DebugLog.DebugWrite("update activity - OnClientDisconnected");
		_activityManager.ClearActivity(UpdateActivityCallback);
	}

	private void OnAddPlayer(PlayerInfo player)
	{
		if (!_activityHasBeenSet)
		{
			return;
		}

		_currentActivity.Party.Size.CurrentSize = QSBPlayerManager.PlayerList.Count;
		DebugLog.DebugWrite("update activity - OnAddPlayer");
		_activityManager.UpdateActivity(_currentActivity, UpdateActivityCallback);
	}

	private void OnRemovePlayer(PlayerInfo player)
	{
		if (!_activityHasBeenSet)
		{
			return;
		}

		_currentActivity.Party.Size.CurrentSize = QSBPlayerManager.PlayerList.Count;
		DebugLog.DebugWrite("update activity - OnRemovePlayer");
		_activityManager.UpdateActivity(_currentActivity, UpdateActivityCallback);
	}

	private void OnChangeClientState(ClientState newState)
	{
		DebugLog.DebugWrite($"OnChangeClientState {newState}");
		switch (newState)
		{
			case ClientState.InTitleScreen:
				_currentActivity.Details = "In title screen.";
				_currentActivity.Assets.SmallImage = "owventures";
				_currentActivity.Assets.SmallText = "In title screen.";
				break;
			case ClientState.AliveInSolarSystem:
				_currentActivity.Details = "In solar system.";
				_currentActivity.Assets.SmallImage = "owventures";
				_currentActivity.Assets.SmallText = "Unknown location.";
				break;
			case ClientState.DeadInSolarSystem:
				_currentActivity.Details = "Dead.";
				_currentActivity.Assets.SmallImage = "skull";
				_currentActivity.Assets.SmallText = "Dead.";
				break;
			case ClientState.AliveInEye:
				_currentActivity.Details = "Somewhere strange...";
				_currentActivity.Assets.SmallImage = "owventures";
				_currentActivity.Assets.SmallText = "Somewhere strange...";
				break;
		}

		DebugLog.DebugWrite("update activity - OnChangeClientState");
		_activityManager.UpdateActivity(_currentActivity, UpdateActivityCallback);
	}
}
