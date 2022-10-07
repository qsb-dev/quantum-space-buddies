using QSB.DeathSync.Messages;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.RespawnSync.Messages;
using QSB.Spectate;
using QSB.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.RespawnSync;

internal class RespawnManager : MonoBehaviour, IAddComponentOnStart
{
	public static RespawnManager Instance;

	public bool RespawnNeeded => _playersPendingRespawn.Count != 0;

	private readonly List<PlayerInfo> _playersPendingRespawn = new();
	private NotificationData _previousNotification;
	private GameObject _owRecoveryPoint;
	private GameObject _qsbRecoveryPoint;

	private void Awake()
	{
		Instance = this;
		QSBSceneManager.OnSceneLoaded += (_, newScene, inUniverse)
			=> Delay.RunWhen(
				() => Locator.GetMarkerManager() != null,
				() => Init(newScene, inUniverse));
		QSBNetworkManager.singleton.OnClientConnected += OnConnected;
		QSBNetworkManager.singleton.OnClientDisconnected += OnDisconnected;

		QSBPlayerManager.OnRemovePlayer += player =>
		{
			_playersPendingRespawn.Remove(player);
		};
	}

	private void OnConnected()
	{
		if (QSBSceneManager.IsInUniverse)
		{
			Delay.RunWhen(
				() => PlayerTransformSync.LocalInstance != null,
				() => Init(QSBSceneManager.CurrentScene, true));
		}
	}

	private void OnDisconnected(string error)
	{
		_owRecoveryPoint?.SetActive(true);
		_qsbRecoveryPoint?.SetActive(false);
	}

	private void Init(OWScene newScene, bool inUniverse)
	{
		if (!QSBCore.IsInMultiplayer)
		{
			return;
		}

		if (PlayerTransformSync.LocalInstance == null)
		{
			DebugLog.ToConsole($"Error - Tried to init when PlayerTransformSync.LocalInstance was null!", OWML.Common.MessageType.Error);
			return;
		}

		QSBPatchManager.DoUnpatchType(QSBPatchTypes.SpectateTime);
		QSBPlayerManager.ShowAllPlayers();
		QSBPlayerManager.LocalPlayer.UpdateStatesFromObjects();
		QSBPlayerManager.PlayerList.ForEach(x => x.IsDead = false);
		_playersPendingRespawn.Clear();

		if (newScene != OWScene.SolarSystem)
		{
			return;
		}

		if (_owRecoveryPoint == null)
		{
			_owRecoveryPoint = GameObject.Find("Systems_Supplies/PlayerRecoveryPoint");
		}

		if (_owRecoveryPoint == null)
		{
			DebugLog.ToConsole($"Error - Couldn't find the ship's PlayerRecoveryPoint!", OWML.Common.MessageType.Error);
			return;
		}

		_owRecoveryPoint.SetActive(false);

		var Systems_Supplies = _owRecoveryPoint.gameObject.transform.parent;

		if (_qsbRecoveryPoint == null)
		{
			_qsbRecoveryPoint = new GameObject("QSBPlayerRecoveryPoint");
			_qsbRecoveryPoint.SetActive(false);
			_qsbRecoveryPoint.transform.parent = Systems_Supplies;
			_qsbRecoveryPoint.transform.localPosition = new Vector3(2.46f, 1.957f, 1.156f);
			_qsbRecoveryPoint.transform.localRotation = Quaternion.Euler(0, 6.460001f, 0f);
			_qsbRecoveryPoint.layer = 21;

			var boxCollider = _qsbRecoveryPoint.AddComponent<BoxCollider>();
			boxCollider.isTrigger = true;
			boxCollider.size = new Vector3(1.3f, 1.01f, 0.47f);

			var multiInteract = _qsbRecoveryPoint.AddComponent<MultiInteractReceiver>();
			multiInteract._usableInShip = true;
			multiInteract._interactRange = 1.5f;

			_qsbRecoveryPoint.AddComponent<ShipRecoveryPoint>();
			_qsbRecoveryPoint.AddComponent<RespawnHUDMarker>();
		}

		_qsbRecoveryPoint.SetActive(true);
	}

	public void Respawn()
	{
		SpectateManager.Instance.ExitSpectate();

		var playerSpawner = FindObjectOfType<PlayerSpawner>();
		playerSpawner.DebugWarp(playerSpawner.GetSpawnPoint(SpawnLocation.Ship));
	}

	public void OnPlayerDeath(PlayerInfo player)
	{
		player.IsDead = true;

		if (_playersPendingRespawn.Contains(player))
		{
			DebugLog.ToConsole($"Warning - Received death message for player who is already in _playersPendingRespawn!", OWML.Common.MessageType.Warning);
		}
		else
		{
			_playersPendingRespawn.Add(player);
		}

		var deadPlayersCount = QSBPlayerManager.PlayerList.Count(x => x.IsDead);

		if (deadPlayersCount == QSBPlayerManager.PlayerList.Count)
		{
			new EndLoopMessage().Send();
			return;
		}

		player.SetVisible(false, 1);
	}

	public void OnPlayerRespawn(PlayerInfo player)
	{
		player.IsDead = false;

		if (!_playersPendingRespawn.Contains(player))
		{
			DebugLog.ToConsole($"Warning - Received respawn message for player who is not in _playersPendingRespawn!", OWML.Common.MessageType.Warning);
		}
		else
		{
			_playersPendingRespawn.Remove(player);
		}

		player.SetVisible(true, 1);
	}

	public void RespawnSomePlayer()
	{
		var playerToRespawn = _playersPendingRespawn.First();

		if (!playerToRespawn.IsDead)
		{
			DebugLog.ToConsole($"Warning - Tried to respawn player {playerToRespawn.PlayerId} who isn't dead!", OWML.Common.MessageType.Warning);
			_playersPendingRespawn.Remove(playerToRespawn);
			return;
		}

		new PlayerRespawnMessage(playerToRespawn.PlayerId).Send();
	}
}