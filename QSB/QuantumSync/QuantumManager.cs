using Cysharp.Threading.Tasks;
using OWML.Common;
using QSB.Messaging;
using QSB.Player;
using QSB.QuantumSync.Messages;
using QSB.QuantumSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace QSB.QuantumSync;

public class QuantumManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Both;

	public static QuantumShrine Shrine { get; private set; }

	public void Awake() => QSBPlayerManager.OnRemovePlayer += PlayerLeave;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		DebugLog.DebugWrite("Building quantum objects...", MessageType.Info);
		QSBWorldSync.Init<QSBQuantumState, QuantumState>();
		QSBWorldSync.Init<QSBSocketedQuantumObject, SocketedQuantumObject>();
		QSBWorldSync.Init<QSBMultiStateQuantumObject, MultiStateQuantumObject>();
		QSBWorldSync.Init<QSBQuantumSocket, QuantumSocket>();
		QSBWorldSync.Init<QSBQuantumShuffleObject, QuantumShuffleObject>();
		QSBWorldSync.Init<QSBQuantumMoon, QuantumMoon>();
		QSBWorldSync.Init<QSBEyeProxyQuantumMoon, EyeProxyQuantumMoon>();
		QSBWorldSync.Init<QSBQuantumSkeletonTower, QuantumSkeletonTower>();
		if (scene == OWScene.SolarSystem)
		{
			Shrine = QSBWorldSync.GetUnityObject<QuantumShrine>();
		}
	}

	public void PlayerLeave(PlayerInfo player)
	{
		if (!QSBCore.IsHost)
		{
			return;
		}

		foreach (var obj in QSBWorldSync.GetWorldObjects<IQSBQuantumObject>())
		{
			if (obj.ControllingPlayer == player.PlayerId)
			{
				obj.SendMessage(new QuantumOwnershipMessage(obj.IsEnabled ? QSBPlayerManager.LocalPlayerId : 0u));
			}
		}
	}

	public void OnRenderObject()
	{
		if (!QSBCore.DrawLines)
		{
			return;
		}

		if (Shrine != null)
		{
			Popcron.Gizmos.Sphere(Shrine.transform.position, 10f, Color.magenta);
		}
	}

	public static (bool FoundPlayers, List<PlayerInfo> PlayersWhoCanSee) IsVisibleUsingCameraFrustum(ShapeVisibilityTracker tracker, bool ignoreLocalCamera)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return (false, new List<PlayerInfo>());
		}

		var playersWithCameras = QSBPlayerManager.GetPlayersWithCameras(!ignoreLocalCamera);
		if (playersWithCameras.Count == 0)
		{
			DebugLog.ToConsole($"Warning - Could not find any players with cameras!", MessageType.Warning);
			return (false, new List<PlayerInfo>());
		}

		if (!tracker.gameObject.activeInHierarchy)
		{
			return (false, new List<PlayerInfo>());
		}

		var playersWhoCanSee = new List<PlayerInfo>();
		var foundPlayers = false;
		foreach (var player in playersWithCameras)
		{
			if (player.Camera == null)
			{
				DebugLog.ToConsole($"Warning - Camera is null for id:{player}!", MessageType.Warning);
				continue;
			}

			var isInFrustum = tracker.IsInFrustum(player.Camera.GetFrustumPlanes());
			if (isInFrustum)
			{
				playersWhoCanSee.Add(player);
				foundPlayers = true;
			}
		}

		return (foundPlayers, playersWhoCanSee);
	}

	public static bool IsVisible(ShapeVisibilityTracker tracker, bool ignoreLocalCamera) =>
		tracker.gameObject.activeInHierarchy
		&& IsVisibleUsingCameraFrustum(tracker, ignoreLocalCamera).FoundPlayers
		&& QSBPlayerManager.GetPlayersWithCameras(!ignoreLocalCamera)
			.Any(x => VisibilityOccluder.CanYouSee(tracker, x.Camera.mainCamera.transform.position));

	public static IEnumerable<PlayerInfo> GetEntangledPlayers(QuantumObject obj)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return Enumerable.Empty<PlayerInfo>();
		}

		var worldObj = obj.GetWorldObject<IQSBQuantumObject>();
		return QSBPlayerManager.PlayerList.Where(x => x.EntangledObject == worldObj);
	}

	public static void OnTakeProbeSnapshot(PlayerInfo player, ProbeCamera.ID cameraId)
	{
		foreach (var quantumObject in QSBWorldSync.GetWorldObjects<IQSBQuantumObject>())
		{
			if (quantumObject.ControllingPlayer == QSBPlayerManager.LocalPlayerId || quantumObject.HostControls)
			{
				quantumObject.OnTakeProbeSnapshot(player, cameraId);
			}
		}
	}

	public static void OnRemoveProbeSnapshot(PlayerInfo player)
	{
		foreach (var quantumObject in QSBWorldSync.GetWorldObjects<IQSBQuantumObject>())
		{
			if (quantumObject.ControllingPlayer == QSBPlayerManager.LocalPlayerId || quantumObject.HostControls)
			{
				quantumObject.OnRemoveProbeSnapshot(player);
			}
		}
	}
}
