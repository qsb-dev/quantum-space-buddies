using QuantumUNET.Components;
using QuantumUNET.Messages;
using QuantumUNET.Transport;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace QuantumUNET
{
	public class QClientScene
	{
		internal static void SetNotReady() => ready = false;

		public static List<QPlayerController> localPlayers { get; private set; } = new List<QPlayerController>();

		public static bool ready { get; private set; }

		public static QNetworkConnection readyConnection { get; private set; }

		public static int reconnectId { get; private set; } = -1;

		public static Dictionary<QNetworkInstanceId, QNetworkIdentity> Objects => s_NetworkScene.localObjects;

		public static Dictionary<int, GameObject> Prefabs => QNetworkScene.guidToPrefab;

		public static Dictionary<QNetworkSceneId, QNetworkIdentity> SpawnableObjects { get; private set; }

		internal static void Shutdown()
		{
			s_NetworkScene.Shutdown();
			localPlayers = new List<QPlayerController>();
			s_PendingOwnerIds = new List<PendingOwner>();
			SpawnableObjects = null;
			readyConnection = null;
			ready = false;
			s_IsSpawnFinished = false;
			reconnectId = -1;
			NetworkTransport.Shutdown();
			NetworkTransport.Init();
		}

		internal static bool GetPlayerController(short playerControllerId, out QPlayerController player)
		{
			player = null;
			bool result;
			if (playerControllerId >= localPlayers.Count)
			{
				Debug.Log($"ClientScene::GetPlayer: no local player found for: {playerControllerId}");
				result = false;
			}
			else if (localPlayers[playerControllerId] == null)
			{
				Debug.LogWarning($"ClientScene::GetPlayer: local player is null for: {playerControllerId}");
				result = false;
			}
			else
			{
				player = localPlayers[playerControllerId];
				result = player.Gameobject != null;
			}

			return result;
		}

		internal static void InternalAddPlayer(QNetworkIdentity view, short playerControllerId)
		{
			Debug.LogWarning($"ClientScene::InternalAddPlayer: playerControllerId : {playerControllerId}");
			if (playerControllerId >= localPlayers.Count)
			{
				Debug.LogWarning(
					$"ClientScene::InternalAddPlayer: playerControllerId higher than expected: {playerControllerId}");
				while (playerControllerId >= localPlayers.Count)
				{
					localPlayers.Add(new QPlayerController());
				}
			}

			var playerController = new QPlayerController
			{
				Gameobject = view.gameObject,
				PlayerControllerId = playerControllerId,
				UnetView = view
			};
			localPlayers[playerControllerId] = playerController;
			readyConnection.SetPlayerController(playerController);
		}

		public static bool AddPlayer(short playerControllerId) => AddPlayer(null, playerControllerId);

		public static bool AddPlayer(QNetworkConnection readyConn, short playerControllerId) => AddPlayer(readyConn, playerControllerId, null);

		public static bool AddPlayer(QNetworkConnection readyConn, short playerControllerId, QMessageBase extraMessage)
		{
			bool result;
			if (playerControllerId < 0)
			{
				Debug.LogError($"ClientScene::AddPlayer: playerControllerId of {playerControllerId} is negative");
				result = false;
			}
			else if (playerControllerId > 32)
			{
				Debug.LogError($"ClientScene::AddPlayer: playerControllerId of {playerControllerId} is too high, max is {32}");
				result = false;
			}
			else
			{
				if (playerControllerId > 16)
				{
					Debug.LogWarning($"ClientScene::AddPlayer: playerControllerId of {playerControllerId} is unusually high");
				}

				while (playerControllerId >= localPlayers.Count)
				{
					localPlayers.Add(new QPlayerController());
				}

				if (readyConn == null)
				{
					if (!ready)
					{
						Debug.LogError("Must call AddPlayer() with a connection the first time to become ready.");
						return false;
					}
				}
				else
				{
					ready = true;
					readyConnection = readyConn;
				}

				if (readyConnection.GetPlayerController(playerControllerId, out var playerController))
				{
					if (playerController.IsValid && playerController.Gameobject != null)
					{
						Debug.LogError($"ClientScene::AddPlayer: playerControllerId of {playerControllerId} already in use.");
						return false;
					}
				}

				Debug.Log($"ClientScene::AddPlayer() for ID {playerControllerId} called with connection [{readyConnection}]");
				var addPlayerMessage = new QAddPlayerMessage
				{
					playerControllerId = playerControllerId
				};
				if (extraMessage != null)
				{
					var networkWriter = new QNetworkWriter();
					extraMessage.Serialize(networkWriter);
					addPlayerMessage.msgData = networkWriter.ToArray();
					addPlayerMessage.msgSize = networkWriter.Position;
				}

				readyConnection.Send(37, addPlayerMessage);
				result = true;
			}

			return result;
		}

		public static bool RemovePlayer(short playerControllerId)
		{
			Debug.Log($"ClientScene::RemovePlayer() for ID {playerControllerId} called with connection [{readyConnection}]");
			bool result;
			if (readyConnection.GetPlayerController(playerControllerId, out var playerController))
			{
				var removePlayerMessage = new QRemovePlayerMessage
				{
					PlayerControllerId = playerControllerId
				};
				readyConnection.Send(38, removePlayerMessage);
				readyConnection.RemovePlayerController(playerControllerId);
				localPlayers[playerControllerId] = new QPlayerController();
				Object.Destroy(playerController.Gameobject);
				result = true;
			}
			else
			{
				Debug.LogError($"Failed to find player ID {playerControllerId}");
				result = false;
			}

			return result;
		}

		public static bool Ready(QNetworkConnection conn)
		{
			bool result;
			if (ready)
			{
				Debug.LogError("A connection has already been set as ready. There can only be one.");
				result = false;
			}
			else
			{
				Debug.Log($"ClientScene::Ready() called with connection [{conn}]");
				if (conn != null)
				{
					var msg = new QReadyMessage();
					conn.Send(35, msg);
					ready = true;
					readyConnection = conn;
					readyConnection.isReady = true;
					result = true;
				}
				else
				{
					Debug.LogError("Ready() called with invalid connection object: conn=null");
					result = false;
				}
			}

			return result;
		}

		public static QNetworkClient ConnectLocalServer()
		{
			var localClient = new QLocalClient();
			QNetworkServer.instance.ActivateLocalClientScene();
			localClient.InternalConnectLocalServer(true);
			return localClient;
		}

		internal static QNetworkClient ReconnectLocalServer()
		{
			var localClient = new QLocalClient();
			QNetworkServer.instance.ActivateLocalClientScene();
			localClient.InternalConnectLocalServer(false);
			return localClient;
		}

		internal static void ClearLocalPlayers() => localPlayers.Clear();

		internal static void HandleClientDisconnect(QNetworkConnection conn)
		{
			if (readyConnection == conn && ready)
			{
				ready = false;
				readyConnection = null;
			}
		}

		internal static void PrepareToSpawnSceneObjects()
		{
			SpawnableObjects = new Dictionary<QNetworkSceneId, QNetworkIdentity>();
			foreach (var networkIdentity in Resources.FindObjectsOfTypeAll<QNetworkIdentity>())
			{
				if (!networkIdentity.gameObject.activeSelf)
				{
					if (networkIdentity.gameObject.hideFlags is not HideFlags.NotEditable and not HideFlags.HideAndDontSave)
					{
						if (!networkIdentity.SceneId.IsEmpty())
						{
							SpawnableObjects[networkIdentity.SceneId] = networkIdentity;
							Debug.Log($"ClientScene::PrepareSpawnObjects sceneId:{networkIdentity.SceneId}");
						}
					}
				}
			}
		}

		internal static QNetworkIdentity SpawnSceneObject(QNetworkSceneId sceneId)
		{
			QNetworkIdentity result;
			if (SpawnableObjects.ContainsKey(sceneId))
			{
				var networkIdentity = SpawnableObjects[sceneId];
				SpawnableObjects.Remove(sceneId);
				result = networkIdentity;
			}
			else
			{
				result = null;
			}

			return result;
		}

		internal static void RegisterSystemHandlers(QNetworkClient client, bool localClient)
		{
			if (localClient)
			{
				client.RegisterHandlerSafe(QMsgType.ObjectDestroy, OnLocalClientObjectDestroy);
				client.RegisterHandlerSafe(QMsgType.ObjectSpawn, OnLocalClientObjectSpawn);
				client.RegisterHandlerSafe(QMsgType.ObjectSpawnScene, OnLocalClientObjectSpawnScene);
				client.RegisterHandlerSafe(QMsgType.ObjectHide, OnLocalClientObjectHide);
				client.RegisterHandlerSafe(QMsgType.LocalClientAuthority, OnClientAuthority);
			}
			else
			{
				client.RegisterHandlerSafe(QMsgType.ObjectDestroy, OnObjectDestroy);
				client.RegisterHandlerSafe(QMsgType.ObjectSpawn, OnObjectSpawn);
				client.RegisterHandlerSafe(QMsgType.Owner, OnOwnerMessage);
				client.RegisterHandlerSafe(QMsgType.UpdateVars, OnUpdateVarsMessage);
				client.RegisterHandlerSafe(QMsgType.SyncList, OnSyncListMessage);
				client.RegisterHandlerSafe(QMsgType.ObjectSpawnScene, OnObjectSpawnScene);
				client.RegisterHandlerSafe(QMsgType.SpawnFinished, OnObjectSpawnFinished);
				client.RegisterHandlerSafe(QMsgType.ObjectHide, OnObjectDestroy);
				client.RegisterHandlerSafe(QMsgType.LocalClientAuthority, OnClientAuthority);
				client.RegisterHandlerSafe(QMsgType.Animation, QNetworkAnimator.OnAnimationClientMessage);
				client.RegisterHandlerSafe(QMsgType.AnimationParameters, QNetworkAnimator.OnAnimationParametersClientMessage);
			}

			client.RegisterHandlerSafe(QMsgType.Rpc, OnRPCMessage);
			client.RegisterHandlerSafe(QMsgType.SyncEvent, OnSyncEventMessage);
			client.RegisterHandlerSafe(QMsgType.AnimationTrigger, QNetworkAnimator.OnAnimationTriggerClientMessage);
		}

		internal static string GetStringForAssetId(int assetId)
		{
			if (QNetworkScene.GetPrefab(assetId, out var gameObject))
			{
				return gameObject.name;
			}

			return QNetworkScene.GetSpawnHandler(assetId, out var func)
				? func.GetMethodName()
				: "unknown";
		}

		public static void RegisterPrefab(GameObject prefab, int newAssetId) => QNetworkScene.RegisterPrefab(prefab, newAssetId);

		public static void RegisterPrefab(GameObject prefab) => QNetworkScene.RegisterPrefab(prefab);

		public static void RegisterPrefab(GameObject prefab, QSpawnDelegate spawnHandler, QUnSpawnDelegate unspawnHandler) => QNetworkScene.RegisterPrefab(prefab, spawnHandler, unspawnHandler);

		public static void UnregisterPrefab(GameObject prefab) => QNetworkScene.UnregisterPrefab(prefab);

		public static void RegisterSpawnHandler(int assetId, QSpawnDelegate spawnHandler, QUnSpawnDelegate unspawnHandler) => QNetworkScene.RegisterSpawnHandler(assetId, spawnHandler, unspawnHandler);

		public static void UnregisterSpawnHandler(int assetId) => QNetworkScene.UnregisterSpawnHandler(assetId);

		public static void ClearSpawners() => QNetworkScene.ClearSpawners();

		public static void DestroyAllClientObjects() => s_NetworkScene.DestroyAllClientObjects();

		public static void SetLocalObject(QNetworkInstanceId netId, GameObject obj) => s_NetworkScene.SetLocalObject(netId, obj, s_IsSpawnFinished, false);

		public static GameObject FindLocalObject(QNetworkInstanceId netId) => s_NetworkScene.FindLocalObject(netId);

		private static void ApplySpawnPayload(QNetworkIdentity uv, Vector3 position, byte[] payload, QNetworkInstanceId netId, GameObject newGameObject)
		{
			if (!uv.gameObject.activeSelf)
			{
				uv.gameObject.SetActive(true);
			}

			uv.transform.position = position;
			if (payload != null && payload.Length > 0)
			{
				var reader = new QNetworkReader(payload);
				uv.OnUpdateVars(reader, true);
			}

			if (newGameObject != null)
			{
				newGameObject.SetActive(true);
				uv.SetNetworkInstanceId(netId);
				SetLocalObject(netId, newGameObject);
				if (s_IsSpawnFinished)
				{
					uv.OnStartClient();
					CheckForOwner(uv);
				}
			}
		}

		private static void OnObjectSpawn(QNetworkMessage netMsg)
		{
			netMsg.ReadMessage(s_ObjectSpawnMessage);
			if (s_ObjectSpawnMessage.assetId == 0)
			{
				Debug.LogError($"OnObjSpawn netId: {s_ObjectSpawnMessage.NetId} has invalid asset Id. {s_ObjectSpawnMessage.assetId}");
			}
			else
			{
				if (s_NetworkScene.GetNetworkIdentity(s_ObjectSpawnMessage.NetId, out var component))
				{
					ApplySpawnPayload(component, s_ObjectSpawnMessage.Position, s_ObjectSpawnMessage.Payload, s_ObjectSpawnMessage.NetId, null);
				}
				else if (QNetworkScene.GetPrefab(s_ObjectSpawnMessage.assetId, out var original))
				{
					var gameObject = Object.Instantiate(original, s_ObjectSpawnMessage.Position, s_ObjectSpawnMessage.Rotation);
					component = gameObject.GetComponent<QNetworkIdentity>();
					if (component == null)
					{
						Debug.LogError($"Client object spawned for {s_ObjectSpawnMessage.assetId} does not have a NetworkIdentity");
					}
					else
					{
						component.Reset();
						ApplySpawnPayload(component, s_ObjectSpawnMessage.Position, s_ObjectSpawnMessage.Payload, s_ObjectSpawnMessage.NetId, gameObject);
					}
				}
				else if (QNetworkScene.GetSpawnHandler(s_ObjectSpawnMessage.assetId, out var spawnDelegate))
				{
					var gameObject2 = spawnDelegate(s_ObjectSpawnMessage.Position, s_ObjectSpawnMessage.assetId);
					if (gameObject2 == null)
					{
						Debug.LogWarning($"Client spawn handler for {s_ObjectSpawnMessage.assetId} returned null");
					}
					else
					{
						component = gameObject2.GetComponent<QNetworkIdentity>();
						if (component == null)
						{
							Debug.LogError($"Client object spawned for {s_ObjectSpawnMessage.assetId} does not have a network identity");
						}
						else
						{
							component.Reset();
							component.SetDynamicAssetId(s_ObjectSpawnMessage.assetId);
							ApplySpawnPayload(component, s_ObjectSpawnMessage.Position, s_ObjectSpawnMessage.Payload, s_ObjectSpawnMessage.NetId, gameObject2);
						}
					}
				}
				else
				{
					Debug.LogError($"Failed to spawn server object, did you forget to add it to the QSBNetworkManager? assetId={s_ObjectSpawnMessage.assetId} netId={s_ObjectSpawnMessage.NetId}");
				}
			}
		}

		private static void OnObjectSpawnScene(QNetworkMessage netMsg)
		{
			netMsg.ReadMessage(s_ObjectSpawnSceneMessage);
			if (s_NetworkScene.GetNetworkIdentity(s_ObjectSpawnSceneMessage.NetId, out var networkIdentity))
			{
				ApplySpawnPayload(networkIdentity, s_ObjectSpawnSceneMessage.Position, s_ObjectSpawnSceneMessage.Payload, s_ObjectSpawnSceneMessage.NetId, networkIdentity.gameObject);
			}
			else
			{
				var networkIdentity2 = SpawnSceneObject(s_ObjectSpawnSceneMessage.SceneId);
				if (networkIdentity2 == null)
				{
					Debug.LogError($"Spawn scene object not found for {s_ObjectSpawnSceneMessage.SceneId}");
				}
				else
				{
					Debug.Log(
						$"Client spawn for [netId:{s_ObjectSpawnSceneMessage.NetId}] [sceneId:{s_ObjectSpawnSceneMessage.SceneId}] obj:{networkIdentity2.gameObject.name}");
					ApplySpawnPayload(networkIdentity2, s_ObjectSpawnSceneMessage.Position, s_ObjectSpawnSceneMessage.Payload, s_ObjectSpawnSceneMessage.NetId, networkIdentity2.gameObject);
				}
			}
		}

		private static void OnObjectSpawnFinished(QNetworkMessage netMsg)
		{
			netMsg.ReadMessage(s_ObjectSpawnFinishedMessage);
			Debug.Log($"SpawnFinished:{s_ObjectSpawnFinishedMessage.State}");
			if (s_ObjectSpawnFinishedMessage.State == 0U)
			{
				PrepareToSpawnSceneObjects();
				s_IsSpawnFinished = false;
			}
			else
			{
				foreach (var networkIdentity in Objects.Values)
				{
					if (!networkIdentity.IsClient)
					{
						networkIdentity.OnStartClient();
						CheckForOwner(networkIdentity);
					}
				}

				s_IsSpawnFinished = true;
			}
		}

		private static void OnObjectDestroy(QNetworkMessage netMsg)
		{
			netMsg.ReadMessage(s_ObjectDestroyMessage);
			Debug.Log($"ClientScene::OnObjDestroy netId:{s_ObjectDestroyMessage.NetId}");
			if (s_NetworkScene.GetNetworkIdentity(s_ObjectDestroyMessage.NetId, out var networkIdentity))
			{
				networkIdentity.OnNetworkDestroy();
				if (!QNetworkScene.InvokeUnSpawnHandler(networkIdentity.AssetId, networkIdentity.gameObject))
				{
					if (networkIdentity.SceneId.IsEmpty())
					{
						Object.Destroy(networkIdentity.gameObject);
					}
					else
					{
						networkIdentity.gameObject.SetActive(false);
						SpawnableObjects[networkIdentity.SceneId] = networkIdentity;
					}
				}

				s_NetworkScene.RemoveLocalObject(s_ObjectDestroyMessage.NetId);
				networkIdentity.MarkForReset();
			}
			else
			{
				Debug.LogWarning($"Did not find target for destroy message for {s_ObjectDestroyMessage.NetId}");
			}
		}

		private static void OnLocalClientObjectDestroy(QNetworkMessage netMsg)
		{
			netMsg.ReadMessage(s_ObjectDestroyMessage);
			Debug.Log($"ClientScene::OnLocalObjectObjDestroy netId:{s_ObjectDestroyMessage.NetId}");
			s_NetworkScene.RemoveLocalObject(s_ObjectDestroyMessage.NetId);
		}

		private static void OnLocalClientObjectHide(QNetworkMessage netMsg)
		{
			netMsg.ReadMessage(s_ObjectDestroyMessage);
			Debug.Log($"ClientScene::OnLocalObjectObjHide netId:{s_ObjectDestroyMessage.NetId}");
			if (s_NetworkScene.GetNetworkIdentity(s_ObjectDestroyMessage.NetId, out var networkIdentity))
			{
				networkIdentity.OnSetLocalVisibility(false);
			}
		}

		private static void OnLocalClientObjectSpawn(QNetworkMessage netMsg)
		{
			netMsg.ReadMessage(s_ObjectSpawnMessage);
			if (s_NetworkScene.GetNetworkIdentity(s_ObjectSpawnMessage.NetId, out var networkIdentity))
			{
				networkIdentity.OnSetLocalVisibility(true);
			}
		}

		private static void OnLocalClientObjectSpawnScene(QNetworkMessage netMsg)
		{
			netMsg.ReadMessage(s_ObjectSpawnSceneMessage);
			if (s_NetworkScene.GetNetworkIdentity(s_ObjectSpawnSceneMessage.NetId, out var networkIdentity))
			{
				networkIdentity.OnSetLocalVisibility(true);
			}
		}

		private static void OnUpdateVarsMessage(QNetworkMessage netMsg)
		{
			var networkInstanceId = netMsg.Reader.ReadNetworkId();
			if (s_NetworkScene.GetNetworkIdentity(networkInstanceId, out var networkIdentity))
			{
				networkIdentity.OnUpdateVars(netMsg.Reader, false);
			}
			else
			{
				Debug.LogWarning($"Did not find target for sync message for {networkInstanceId}");
			}
		}

		private static void OnRPCMessage(QNetworkMessage netMsg)
		{
			var num = (int)netMsg.Reader.ReadPackedUInt32();
			var networkInstanceId = netMsg.Reader.ReadNetworkId();
			Debug.Log($"ClientScene::OnRPCMessage hash:{num} netId:{networkInstanceId}");
			if (s_NetworkScene.GetNetworkIdentity(networkInstanceId, out var networkIdentity))
			{
				networkIdentity.HandleRPC(num, netMsg.Reader);
			}
			else
			{
				var cmdHashHandlerName = QNetworkBehaviour.GetCmdHashHandlerName(num);
				Debug.LogWarningFormat("Could not find target object with netId:{0} for RPC call {1}", networkInstanceId, cmdHashHandlerName);
			}
		}

		private static void OnSyncEventMessage(QNetworkMessage netMsg)
		{
			var cmdHash = (int)netMsg.Reader.ReadPackedUInt32();
			var networkInstanceId = netMsg.Reader.ReadNetworkId();
			Debug.Log($"ClientScene::OnSyncEventMessage {networkInstanceId}");
			if (s_NetworkScene.GetNetworkIdentity(networkInstanceId, out var networkIdentity))
			{
				networkIdentity.HandleSyncEvent(cmdHash, netMsg.Reader);
			}
			else
			{
				Debug.LogWarning($"Did not find target for SyncEvent message for {networkInstanceId}");
			}
		}

		private static void OnSyncListMessage(QNetworkMessage netMsg)
		{
			var networkInstanceId = netMsg.Reader.ReadNetworkId();
			var cmdHash = (int)netMsg.Reader.ReadPackedUInt32();
			Debug.Log($"ClientScene::OnSyncListMessage {networkInstanceId}");
			if (s_NetworkScene.GetNetworkIdentity(networkInstanceId, out var networkIdentity))
			{
				networkIdentity.HandleSyncList(cmdHash, netMsg.Reader);
			}
			else
			{
				Debug.LogWarning($"Did not find target for SyncList message for {networkInstanceId}");
			}
		}

		private static void OnClientAuthority(QNetworkMessage netMsg)
		{
			netMsg.ReadMessage(s_ClientAuthorityMessage);
			Debug.Log(
				$"ClientScene::OnClientAuthority for  connectionId={netMsg.Connection.connectionId} netId: {s_ClientAuthorityMessage.netId}");
			if (s_NetworkScene.GetNetworkIdentity(s_ClientAuthorityMessage.netId, out var networkIdentity))
			{
				networkIdentity.HandleClientAuthority(s_ClientAuthorityMessage.authority);
			}
		}

		private static void OnOwnerMessage(QNetworkMessage netMsg)
		{
			netMsg.ReadMessage(s_OwnerMessage);
			Debug.Log(
				$"ClientScene::OnOwnerMessage - connectionId={netMsg.Connection.connectionId} netId: {s_OwnerMessage.NetId}");
			if (netMsg.Connection.GetPlayerController(s_OwnerMessage.PlayerControllerId, out var playerController))
			{
				playerController.UnetView.SetNotLocalPlayer();
			}

			if (s_NetworkScene.GetNetworkIdentity(s_OwnerMessage.NetId, out var networkIdentity))
			{
				networkIdentity.SetConnectionToServer(netMsg.Connection);
				networkIdentity.SetLocalPlayer(s_OwnerMessage.PlayerControllerId);
				InternalAddPlayer(networkIdentity, s_OwnerMessage.PlayerControllerId);
			}
			else
			{
				var item = new PendingOwner
				{
					netId = s_OwnerMessage.NetId,
					playerControllerId = s_OwnerMessage.PlayerControllerId
				};
				s_PendingOwnerIds.Add(item);
			}
		}

		private static void CheckForOwner(QNetworkIdentity uv)
		{
			var i = 0;
			while (i < s_PendingOwnerIds.Count)
			{
				var pendingOwner = s_PendingOwnerIds[i];
				if (pendingOwner.netId == uv.NetId)
				{
					uv.SetConnectionToServer(readyConnection);
					uv.SetLocalPlayer(pendingOwner.playerControllerId);
					Debug.Log($"ClientScene::OnOwnerMessage - player={uv.gameObject.name}");
					if (readyConnection.connectionId < 0)
					{
						Debug.LogError("Owner message received on a local client.");
						break;
					}

					InternalAddPlayer(uv, pendingOwner.playerControllerId);
					s_PendingOwnerIds.RemoveAt(i);
					break;
				}

				i++;
			}
		}

		private static bool s_IsSpawnFinished;

		private static readonly QNetworkScene s_NetworkScene = new();

		private static readonly QObjectSpawnSceneMessage s_ObjectSpawnSceneMessage = new();

		private static readonly QObjectSpawnFinishedMessage s_ObjectSpawnFinishedMessage = new();

		private static readonly QObjectDestroyMessage s_ObjectDestroyMessage = new();

		private static readonly QObjectSpawnMessage s_ObjectSpawnMessage = new();

		private static readonly QOwnerMessage s_OwnerMessage = new();

		private static readonly QClientAuthorityMessage s_ClientAuthorityMessage = new();

		public const int ReconnectIdInvalid = -1;

		public const int ReconnectIdHost = 0;

		private static List<PendingOwner> s_PendingOwnerIds = new();

		private struct PendingOwner
		{
			public QNetworkInstanceId netId;

			public short playerControllerId;
		}
	}
}