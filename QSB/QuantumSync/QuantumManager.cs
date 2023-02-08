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

internal class QuantumManager : WorldObjectManager
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

		UpdateFromDebugSetting();
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
				obj.SendMessage(new QuantumAuthorityMessage(obj.IsEnabled ? QSBPlayerManager.LocalPlayerId : 0u));
			}
		}
	}

	public void OnRenderObject()
	{
		if (!QSBCore.DebugSettings.DrawLines)
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

	#region debug shapes

	private static GameObject _debugSphere, _debugCube, _debugCapsule;

	private class DebugShape : MonoBehaviour { }

	public static void UpdateFromDebugSetting()
	{
		if (QSBCore.DebugSettings.DrawQuantumVisibilityObjects)
		{
			if (_debugSphere == null)
			{
				_debugSphere = QSBCore.DebugAssetBundle.LoadAsset<GameObject>("Assets/Prefabs/Sphere.prefab");
			}

			if (_debugCube == null)
			{
				_debugCube = QSBCore.DebugAssetBundle.LoadAsset<GameObject>("Assets/Prefabs/Cube.prefab");
			}

			if (_debugCapsule == null)
			{
				_debugCapsule = QSBCore.DebugAssetBundle.LoadAsset<GameObject>("Assets/Prefabs/Capsule.prefab");
			}

			foreach (var quantumObject in QSBWorldSync.GetWorldObjects<IQSBQuantumObject>())
			{
				foreach (var shape in quantumObject.GetAttachedShapes())
				{
					if (shape is BoxShape boxShape)
					{
						var newCube = Instantiate(_debugCube);
						newCube.transform.parent = shape.transform;
						newCube.transform.localPosition = Vector3.zero;
						newCube.transform.localRotation = Quaternion.Euler(0, 0, 0);
						newCube.transform.localScale = boxShape.size;
						newCube.AddComponent<DebugShape>();
					}
					else if (shape is SphereShape sphereShape)
					{
						var newSphere = Instantiate(_debugSphere);
						newSphere.transform.parent = shape.transform;
						newSphere.transform.localPosition = Vector3.zero;
						newSphere.transform.localRotation = Quaternion.Euler(0, 0, 0);
						newSphere.transform.localScale = Vector3.one * (sphereShape.radius * 2);
						newSphere.AddComponent<DebugShape>();
					}
					else if (shape is CapsuleShape capsuleShape)
					{
						var newCapsule = Instantiate(_debugCapsule);
						newCapsule.transform.parent = shape.transform;
						newCapsule.transform.localPosition = Vector3.zero;
						newCapsule.transform.localRotation = Quaternion.Euler(0, 0, 0);
						newCapsule.transform.localScale = new Vector3(capsuleShape.radius * 2, capsuleShape.height, capsuleShape.radius * 2);
						newCapsule.AddComponent<DebugShape>();
					}
				}
			}
		}
		else
		{
			foreach (var quantumObject in QSBWorldSync.GetWorldObjects<IQSBQuantumObject>())
			{
				foreach (var shape in quantumObject.GetAttachedShapes())
				{
					var debugShape = shape.GetComponentInChildren<DebugShape>();
					if (debugShape)
					{
						Destroy(debugShape.gameObject);
					}
				}
			}
		}
	}

	#endregion
}
