using QSB.Animation;
using QSB.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	public class QSBClientScene
	{
		private static bool HasMigrationPending() => reconnectId != -1;

		public static void SetReconnectId(int newReconnectId, QSBPeerInfoMessage[] peers)
		{
			reconnectId = newReconnectId;
			s_Peers = peers;
			if (LogFilter.logDebug)
			{
				Debug.Log("ClientScene::SetReconnectId: " + newReconnectId);
			}
		}

		internal static void SetNotReady() => ready = false;

		public static List<QSBPlayerController> localPlayers { get; private set; } = new List<QSBPlayerController>();

		public static bool ready { get; private set; }

		public static QSBNetworkConnection readyConnection { get; private set; }

		public static int reconnectId { get; private set; } = -1;

		public static Dictionary<NetworkInstanceId, QSBNetworkIdentity> Objects => s_NetworkScene.localObjects;

		public static Dictionary<NetworkHash128, GameObject> Prefabs => QSBNetworkScene.guidToPrefab;

		public static Dictionary<NetworkSceneId, QSBNetworkIdentity> SpawnableObjects { get; private set; }

		internal static void Shutdown()
		{
			s_NetworkScene.Shutdown();
			localPlayers = new List<QSBPlayerController>();
			s_PendingOwnerIds = new List<PendingOwner>();
			SpawnableObjects = null;
			readyConnection = null;
			ready = false;
			s_IsSpawnFinished = false;
			reconnectId = -1;
			NetworkTransport.Shutdown();
			NetworkTransport.Init();
		}

		internal static bool GetPlayerController(short playerControllerId, out QSBPlayerController player)
		{
			player = null;
			bool result;
			if ((int)playerControllerId >= localPlayers.Count)
			{
				if (LogFilter.logWarn)
				{
					Debug.Log("ClientScene::GetPlayer: no local player found for: " + playerControllerId);
				}
				result = false;
			}
			else if (localPlayers[(int)playerControllerId] == null)
			{
				if (LogFilter.logWarn)
				{
					Debug.LogWarning("ClientScene::GetPlayer: local player is null for: " + playerControllerId);
				}
				result = false;
			}
			else
			{
				player = localPlayers[(int)playerControllerId];
				result = (player.Gameobject != null);
			}
			return result;
		}

		internal static void InternalAddPlayer(QSBNetworkIdentity view, short playerControllerId)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("ClientScene::InternalAddPlayer: playerControllerId : " + playerControllerId);
			}
			if ((int)playerControllerId >= localPlayers.Count)
			{
				if (LogFilter.logWarn)
				{
					Debug.LogWarning("ClientScene::InternalAddPlayer: playerControllerId higher than expected: " + playerControllerId);
				}
				while ((int)playerControllerId >= localPlayers.Count)
				{
					localPlayers.Add(new QSBPlayerController());
				}
			}
			var playerController = new QSBPlayerController
			{
				Gameobject = view.gameObject,
				PlayerControllerId = playerControllerId,
				UnetView = view
			};
			localPlayers[(int)playerControllerId] = playerController;
			readyConnection.SetPlayerController(playerController);
		}

		public static bool AddPlayer(short playerControllerId) => AddPlayer(null, playerControllerId);

		public static bool AddPlayer(QSBNetworkConnection readyConn, short playerControllerId) => AddPlayer(readyConn, playerControllerId, null);

		public static bool AddPlayer(QSBNetworkConnection readyConn, short playerControllerId, QSBMessageBase extraMessage)
		{
			bool result;
			if (playerControllerId < 0)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("ClientScene::AddPlayer: playerControllerId of " + playerControllerId + " is negative");
				}
				result = false;
			}
			else if (playerControllerId > 32)
			{
				if (LogFilter.logError)
				{
					Debug.LogError(string.Concat(new object[]
					{
						"ClientScene::AddPlayer: playerControllerId of ",
						playerControllerId,
						" is too high, max is ",
						32
					}));
				}
				result = false;
			}
			else
			{
				if (playerControllerId > 16)
				{
					if (LogFilter.logWarn)
					{
						Debug.LogWarning("ClientScene::AddPlayer: playerControllerId of " + playerControllerId + " is unusually high");
					}
				}
				while ((int)playerControllerId >= localPlayers.Count)
				{
					localPlayers.Add(new QSBPlayerController());
				}
				if (readyConn == null)
				{
					if (!ready)
					{
						if (LogFilter.logError)
						{
							Debug.LogError("Must call AddPlayer() with a connection the first time to become ready.");
						}
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
						if (LogFilter.logError)
						{
							Debug.LogError("ClientScene::AddPlayer: playerControllerId of " + playerControllerId + " already in use.");
						}
						return false;
					}
				}
				if (LogFilter.logDebug)
				{
					Debug.Log(string.Concat(new object[]
					{
						"ClientScene::AddPlayer() for ID ",
						playerControllerId,
						" called with connection [",
						readyConnection,
						"]"
					}));
				}
				if (!HasMigrationPending())
				{
					var addPlayerMessage = new QSBAddPlayerMessage
					{
						playerControllerId = playerControllerId
					};
					if (extraMessage != null)
					{
						var networkWriter = new QSBNetworkWriter();
						extraMessage.Serialize(networkWriter);
						addPlayerMessage.msgData = networkWriter.ToArray();
						addPlayerMessage.msgSize = (int)networkWriter.Position;
					}
					readyConnection.Send(37, addPlayerMessage);
					result = true;
				}
				else
				{
					result = SendReconnectMessage(extraMessage);
				}
			}
			return result;
		}

		public static bool SendReconnectMessage(QSBMessageBase extraMessage)
		{
			bool result;
			if (!HasMigrationPending())
			{
				result = false;
			}
			else
			{
				if (LogFilter.logDebug)
				{
					Debug.Log("ClientScene::AddPlayer reconnect " + reconnectId);
				}
				if (s_Peers == null)
				{
					SetReconnectId(-1, null);
					if (LogFilter.logError)
					{
						Debug.LogError("ClientScene::AddPlayer: reconnecting, but no peers.");
					}
					result = false;
				}
				else
				{
					for (var i = 0; i < s_Peers.Length; i++)
					{
						var peerInfoMessage = s_Peers[i];
						if (peerInfoMessage.playerIds != null)
						{
							if (peerInfoMessage.connectionId == reconnectId)
							{
								for (var j = 0; j < peerInfoMessage.playerIds.Length; j++)
								{
									var reconnectMessage = new QSBReconnectMessage
									{
										oldConnectionId = reconnectId,
										netId = peerInfoMessage.playerIds[j].netId,
										playerControllerId = peerInfoMessage.playerIds[j].playerControllerId
									};
									if (extraMessage != null)
									{
										var networkWriter = new QSBNetworkWriter();
										extraMessage.Serialize(networkWriter);
										reconnectMessage.msgData = networkWriter.ToArray();
										reconnectMessage.msgSize = (int)networkWriter.Position;
									}
									readyConnection.Send(47, reconnectMessage);
								}
							}
						}
					}
					SetReconnectId(-1, null);
					result = true;
				}
			}
			return result;
		}

		public static bool RemovePlayer(short playerControllerId)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"ClientScene::RemovePlayer() for ID ",
					playerControllerId,
					" called with connection [",
					readyConnection,
					"]"
				}));
			}
			bool result;
			if (readyConnection.GetPlayerController(playerControllerId, out var playerController))
			{
				var removePlayerMessage = new QSBRemovePlayerMessage
				{
					PlayerControllerId = playerControllerId
				};
				readyConnection.Send(38, removePlayerMessage);
				readyConnection.RemovePlayerController(playerControllerId);
				localPlayers[(int)playerControllerId] = new QSBPlayerController();
				UnityEngine.Object.Destroy(playerController.Gameobject);
				result = true;
			}
			else
			{
				if (LogFilter.logError)
				{
					Debug.LogError("Failed to find player ID " + playerControllerId);
				}
				result = false;
			}
			return result;
		}

		public static bool Ready(QSBNetworkConnection conn)
		{
			bool result;
			if (ready)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("A connection has already been set as ready. There can only be one.");
				}
				result = false;
			}
			else
			{
				if (LogFilter.logDebug)
				{
					Debug.Log("ClientScene::Ready() called with connection [" + conn + "]");
				}
				if (conn != null)
				{
					var msg = new QSBReadyMessage();
					conn.Send(35, msg);
					DebugLog.DebugWrite("SENDING READY");
					ready = true;
					readyConnection = conn;
					readyConnection.isReady = true;
					result = true;
				}
				else
				{
					if (LogFilter.logError)
					{
						Debug.LogError("Ready() called with invalid connection object: conn=null");
					}
					result = false;
				}
			}
			return result;
		}

		public static QSBNetworkClient ConnectLocalServer()
		{
			var localClient = new QSBLocalClient();
			QSBNetworkServer.instance.ActivateLocalClientScene();
			localClient.InternalConnectLocalServer(true);
			return localClient;
		}

		internal static QSBNetworkClient ReconnectLocalServer()
		{
			var localClient = new QSBLocalClient();
			QSBNetworkServer.instance.ActivateLocalClientScene();
			localClient.InternalConnectLocalServer(false);
			return localClient;
		}

		internal static void ClearLocalPlayers() => localPlayers.Clear();

		internal static void HandleClientDisconnect(QSBNetworkConnection conn)
		{
			if (readyConnection == conn && ready)
			{
				ready = false;
				readyConnection = null;
			}
		}

		internal static void PrepareToSpawnSceneObjects()
		{
			SpawnableObjects = new Dictionary<NetworkSceneId, QSBNetworkIdentity>();
			foreach (var networkIdentity in Resources.FindObjectsOfTypeAll<QSBNetworkIdentity>())
			{
				if (!networkIdentity.gameObject.activeSelf)
				{
					if (networkIdentity.gameObject.hideFlags != HideFlags.NotEditable && networkIdentity.gameObject.hideFlags != HideFlags.HideAndDontSave)
					{
						if (!networkIdentity.SceneId.IsEmpty())
						{
							SpawnableObjects[networkIdentity.SceneId] = networkIdentity;
							if (LogFilter.logDebug)
							{
								Debug.Log("ClientScene::PrepareSpawnObjects sceneId:" + networkIdentity.SceneId);
							}
						}
					}
				}
			}
		}

		internal static QSBNetworkIdentity SpawnSceneObject(NetworkSceneId sceneId)
		{
			QSBNetworkIdentity result;
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

		internal static void RegisterSystemHandlers(QSBNetworkClient client, bool localClient)
		{
			if (localClient)
			{
				client.RegisterHandlerSafe((short)1, new QSBNetworkMessageDelegate(OnLocalClientObjectDestroy));
				client.RegisterHandlerSafe((short)13, new QSBNetworkMessageDelegate(OnLocalClientObjectHide));
				client.RegisterHandlerSafe((short)3, new QSBNetworkMessageDelegate(OnLocalClientObjectSpawn));
				client.RegisterHandlerSafe((short)10, new QSBNetworkMessageDelegate(OnLocalClientObjectSpawnScene));
				client.RegisterHandlerSafe((short)15, new QSBNetworkMessageDelegate(OnClientAuthority));
			}
			else
			{
				client.RegisterHandlerSafe((short)3, new QSBNetworkMessageDelegate(OnObjectSpawn));
				client.RegisterHandlerSafe((short)10, new QSBNetworkMessageDelegate(OnObjectSpawnScene));
				client.RegisterHandlerSafe((short)12, new QSBNetworkMessageDelegate(OnObjectSpawnFinished));
				client.RegisterHandlerSafe((short)1, new QSBNetworkMessageDelegate(OnObjectDestroy));
				client.RegisterHandlerSafe((short)13, new QSBNetworkMessageDelegate(OnObjectDestroy));
				client.RegisterHandlerSafe((short)8, new QSBNetworkMessageDelegate(OnUpdateVarsMessage));
				client.RegisterHandlerSafe((short)4, new QSBNetworkMessageDelegate(OnOwnerMessage));
				client.RegisterHandlerSafe((short)9, new QSBNetworkMessageDelegate(OnSyncListMessage));
				client.RegisterHandlerSafe((short)40, new QSBNetworkMessageDelegate(QSBNetworkAnimator.OnAnimationClientMessage));
				client.RegisterHandlerSafe((short)41, new QSBNetworkMessageDelegate(QSBNetworkAnimator.OnAnimationParametersClientMessage));
				client.RegisterHandlerSafe((short)15, new QSBNetworkMessageDelegate(OnClientAuthority));
			}
			client.RegisterHandlerSafe((short)2, new QSBNetworkMessageDelegate(OnRPCMessage));
			client.RegisterHandlerSafe((short)7, new QSBNetworkMessageDelegate(OnSyncEventMessage));
			client.RegisterHandlerSafe((short)42, new QSBNetworkMessageDelegate(QSBNetworkAnimator.OnAnimationTriggerClientMessage));
		}

		internal static string GetStringForAssetId(NetworkHash128 assetId)
		{
			string result;
			if (QSBNetworkScene.GetPrefab(assetId, out var gameObject))
			{
				result = gameObject.name;
			}
			else if (QSBNetworkScene.GetSpawnHandler(assetId, out var func))
			{
				result = func.GetMethodName();
			}
			else
			{
				result = "unknown";
			}
			return result;
		}

		public static void RegisterPrefab(GameObject prefab, NetworkHash128 newAssetId) => QSBNetworkScene.RegisterPrefab(prefab, newAssetId);

		public static void RegisterPrefab(GameObject prefab) => QSBNetworkScene.RegisterPrefab(prefab);

		public static void RegisterPrefab(GameObject prefab, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler) => QSBNetworkScene.RegisterPrefab(prefab, spawnHandler, unspawnHandler);

		public static void UnregisterPrefab(GameObject prefab) => QSBNetworkScene.UnregisterPrefab(prefab);

		public static void RegisterSpawnHandler(NetworkHash128 assetId, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler) => QSBNetworkScene.RegisterSpawnHandler(assetId, spawnHandler, unspawnHandler);

		public static void UnregisterSpawnHandler(NetworkHash128 assetId) => QSBNetworkScene.UnregisterSpawnHandler(assetId);

		public static void ClearSpawners() => QSBNetworkScene.ClearSpawners();

		public static void DestroyAllClientObjects() => s_NetworkScene.DestroyAllClientObjects();

		public static void SetLocalObject(NetworkInstanceId netId, GameObject obj) => s_NetworkScene.SetLocalObject(netId, obj, s_IsSpawnFinished, false);

		public static GameObject FindLocalObject(NetworkInstanceId netId) => s_NetworkScene.FindLocalObject(netId);

		private static void ApplySpawnPayload(QSBNetworkIdentity uv, Vector3 position, byte[] payload, NetworkInstanceId netId, GameObject newGameObject)
		{
			if (!uv.gameObject.activeSelf)
			{
				uv.gameObject.SetActive(true);
			}
			uv.transform.position = position;
			if (payload != null && payload.Length > 0)
			{
				var reader = new QSBNetworkReader(payload);
				uv.OnUpdateVars(reader, true);
			}
			if (!(newGameObject == null))
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

		private static void OnObjectSpawn(QSBNetworkMessage netMsg)
		{
			DebugLog.DebugWrite("OnObjectSpawn");
			netMsg.ReadMessage<QSBObjectSpawnMessage>(s_ObjectSpawnMessage);
			if (!s_ObjectSpawnMessage.assetId.IsValid())
			{
				Debug.LogError($"OnObjSpawn netId: {s_ObjectSpawnMessage.NetId} has invalid asset Id. {s_ObjectSpawnMessage.assetId}");
			}
			else
			{
				DebugLog.DebugWrite(string.Concat(new object[]
				{
					"Client spawn handler instantiating [netId:",
					s_ObjectSpawnMessage.NetId,
					" asset ID:",
					s_ObjectSpawnMessage.assetId,
					" pos:",
					s_ObjectSpawnMessage.Position,
					"]"
				}));
				if (s_NetworkScene.GetNetworkIdentity(s_ObjectSpawnMessage.NetId, out var component))
				{
					ApplySpawnPayload(component, s_ObjectSpawnMessage.Position, s_ObjectSpawnMessage.Payload, s_ObjectSpawnMessage.NetId, null);
				}
				else if (QSBNetworkScene.GetPrefab(s_ObjectSpawnMessage.assetId, out var original))
				{
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original, s_ObjectSpawnMessage.Position, s_ObjectSpawnMessage.Rotation);
					if (LogFilter.logDebug)
					{
						DebugLog.DebugWrite(string.Concat(new object[]
						{
							"Client spawn handler instantiating [netId:",
							s_ObjectSpawnMessage.NetId,
							" asset ID:",
							s_ObjectSpawnMessage.assetId,
							" pos:",
							s_ObjectSpawnMessage.Position,
							" rotation: ",
							s_ObjectSpawnMessage.Rotation,
							"]"
						}));
					}
					component = gameObject.GetComponent<QSBNetworkIdentity>();
					if (component == null)
					{
						Debug.LogError("Client object spawned for " + s_ObjectSpawnMessage.assetId + " does not have a NetworkIdentity");
					}
					else
					{
						component.Reset();
						ApplySpawnPayload(component, s_ObjectSpawnMessage.Position, s_ObjectSpawnMessage.Payload, s_ObjectSpawnMessage.NetId, gameObject);
					}
				}
				else if (QSBNetworkScene.GetSpawnHandler(s_ObjectSpawnMessage.assetId, out var spawnDelegate))
				{
					var gameObject2 = spawnDelegate(s_ObjectSpawnMessage.Position, s_ObjectSpawnMessage.assetId);
					if (gameObject2 == null)
					{
						Debug.LogWarning("Client spawn handler for " + s_ObjectSpawnMessage.assetId + " returned null");
					}
					else
					{
						component = gameObject2.GetComponent<QSBNetworkIdentity>();
						if (component == null)
						{
							Debug.LogError("Client object spawned for " + s_ObjectSpawnMessage.assetId + " does not have a network identity");
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
					Debug.LogError(string.Concat(new object[]
					{
						"Failed to spawn server object, did you forget to add it to the QSBNetworkManager? assetId=",
						s_ObjectSpawnMessage.assetId,
						" netId=",
						s_ObjectSpawnMessage.NetId
					}));
				}
			}
		}

		private static void OnObjectSpawnScene(QSBNetworkMessage netMsg)
		{
			netMsg.ReadMessage<QSBObjectSpawnSceneMessage>(s_ObjectSpawnSceneMessage);
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"Client spawn scene handler instantiating [netId:",
					s_ObjectSpawnSceneMessage.NetId,
					" sceneId:",
					s_ObjectSpawnSceneMessage.SceneId,
					" pos:",
					s_ObjectSpawnSceneMessage.Position
				}));
			}
			if (s_NetworkScene.GetNetworkIdentity(s_ObjectSpawnSceneMessage.NetId, out var networkIdentity))
			{
				ApplySpawnPayload(networkIdentity, s_ObjectSpawnSceneMessage.Position, s_ObjectSpawnSceneMessage.Payload, s_ObjectSpawnSceneMessage.NetId, networkIdentity.gameObject);
			}
			else
			{
				var networkIdentity2 = SpawnSceneObject(s_ObjectSpawnSceneMessage.SceneId);
				if (networkIdentity2 == null)
				{
					if (LogFilter.logError)
					{
						Debug.LogError("Spawn scene object not found for " + s_ObjectSpawnSceneMessage.SceneId);
					}
				}
				else
				{
					if (LogFilter.logDebug)
					{
						Debug.Log(string.Concat(new object[]
						{
							"Client spawn for [netId:",
							s_ObjectSpawnSceneMessage.NetId,
							"] [sceneId:",
							s_ObjectSpawnSceneMessage.SceneId,
							"] obj:",
							networkIdentity2.gameObject.name
						}));
					}
					ApplySpawnPayload(networkIdentity2, s_ObjectSpawnSceneMessage.Position, s_ObjectSpawnSceneMessage.Payload, s_ObjectSpawnSceneMessage.NetId, networkIdentity2.gameObject);
				}
			}
		}

		private static void OnObjectSpawnFinished(QSBNetworkMessage netMsg)
		{
			netMsg.ReadMessage<QSBObjectSpawnFinishedMessage>(s_ObjectSpawnFinishedMessage);
			if (LogFilter.logDebug)
			{
				Debug.Log("SpawnFinished:" + s_ObjectSpawnFinishedMessage.State);
			}
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

		private static void OnObjectDestroy(QSBNetworkMessage netMsg)
		{
			netMsg.ReadMessage<QSBObjectDestroyMessage>(s_ObjectDestroyMessage);
			if (LogFilter.logDebug)
			{
				Debug.Log("ClientScene::OnObjDestroy netId:" + s_ObjectDestroyMessage.NetId);
			}
			if (s_NetworkScene.GetNetworkIdentity(s_ObjectDestroyMessage.NetId, out var networkIdentity))
			{
				networkIdentity.OnNetworkDestroy();
				if (!QSBNetworkScene.InvokeUnSpawnHandler(networkIdentity.AssetId, networkIdentity.gameObject))
				{
					if (networkIdentity.SceneId.IsEmpty())
					{
						UnityEngine.Object.Destroy(networkIdentity.gameObject);
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
			else if (LogFilter.logDebug)
			{
				Debug.LogWarning("Did not find target for destroy message for " + s_ObjectDestroyMessage.NetId);
			}
		}

		private static void OnLocalClientObjectDestroy(QSBNetworkMessage netMsg)
		{
			netMsg.ReadMessage<QSBObjectDestroyMessage>(s_ObjectDestroyMessage);
			if (LogFilter.logDebug)
			{
				Debug.Log("ClientScene::OnLocalObjectObjDestroy netId:" + s_ObjectDestroyMessage.NetId);
			}
			s_NetworkScene.RemoveLocalObject(s_ObjectDestroyMessage.NetId);
		}

		private static void OnLocalClientObjectHide(QSBNetworkMessage netMsg)
		{
			netMsg.ReadMessage<QSBObjectDestroyMessage>(s_ObjectDestroyMessage);
			if (LogFilter.logDebug)
			{
				Debug.Log("ClientScene::OnLocalObjectObjHide netId:" + s_ObjectDestroyMessage.NetId);
			}
			if (s_NetworkScene.GetNetworkIdentity(s_ObjectDestroyMessage.NetId, out var networkIdentity))
			{
				networkIdentity.OnSetLocalVisibility(false);
			}
		}

		private static void OnLocalClientObjectSpawn(QSBNetworkMessage netMsg)
		{
			netMsg.ReadMessage<QSBObjectSpawnMessage>(s_ObjectSpawnMessage);
			if (s_NetworkScene.GetNetworkIdentity(s_ObjectSpawnMessage.NetId, out var networkIdentity))
			{
				networkIdentity.OnSetLocalVisibility(true);
			}
		}

		private static void OnLocalClientObjectSpawnScene(QSBNetworkMessage netMsg)
		{
			netMsg.ReadMessage<QSBObjectSpawnSceneMessage>(s_ObjectSpawnSceneMessage);
			if (s_NetworkScene.GetNetworkIdentity(s_ObjectSpawnSceneMessage.NetId, out var networkIdentity))
			{
				networkIdentity.OnSetLocalVisibility(true);
			}
		}

		private static void OnUpdateVarsMessage(QSBNetworkMessage netMsg)
		{
			var networkInstanceId = netMsg.Reader.ReadNetworkId();
			DebugLog.DebugWrite(string.Concat(new object[]
			{
				"ClientScene::OnUpdateVarsMessage ",
				networkInstanceId,
				" channel:",
				netMsg.ChannelId
			}));
			if (s_NetworkScene.GetNetworkIdentity(networkInstanceId, out var networkIdentity))
			{
				networkIdentity.OnUpdateVars(netMsg.Reader, false);
			}
			else if (LogFilter.logWarn)
			{
				Debug.LogWarning("Did not find target for sync message for " + networkInstanceId);
			}
		}

		private static void OnRPCMessage(QSBNetworkMessage netMsg)
		{
			var num = (int)netMsg.Reader.ReadPackedUInt32();
			var networkInstanceId = netMsg.Reader.ReadNetworkId();
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"ClientScene::OnRPCMessage hash:",
					num,
					" netId:",
					networkInstanceId
				}));
			}
			if (s_NetworkScene.GetNetworkIdentity(networkInstanceId, out var networkIdentity))
			{
				networkIdentity.HandleRPC(num, netMsg.Reader);
			}
			else if (LogFilter.logWarn)
			{
				string cmdHashHandlerName = QSBNetworkBehaviour.GetCmdHashHandlerName(num);
				Debug.LogWarningFormat("Could not find target object with netId:{0} for RPC call {1}", new object[]
				{
					networkInstanceId,
					cmdHashHandlerName
				});
			}
		}

		private static void OnSyncEventMessage(QSBNetworkMessage netMsg)
		{
			var cmdHash = (int)netMsg.Reader.ReadPackedUInt32();
			var networkInstanceId = netMsg.Reader.ReadNetworkId();
			if (LogFilter.logDebug)
			{
				Debug.Log("ClientScene::OnSyncEventMessage " + networkInstanceId);
			}
			if (s_NetworkScene.GetNetworkIdentity(networkInstanceId, out var networkIdentity))
			{
				networkIdentity.HandleSyncEvent(cmdHash, netMsg.Reader);
			}
			else if (LogFilter.logWarn)
			{
				Debug.LogWarning("Did not find target for SyncEvent message for " + networkInstanceId);
			}
		}

		private static void OnSyncListMessage(QSBNetworkMessage netMsg)
		{
			var networkInstanceId = netMsg.Reader.ReadNetworkId();
			var cmdHash = (int)netMsg.Reader.ReadPackedUInt32();
			if (LogFilter.logDebug)
			{
				Debug.Log("ClientScene::OnSyncListMessage " + networkInstanceId);
			}
			if (s_NetworkScene.GetNetworkIdentity(networkInstanceId, out var networkIdentity))
			{
				networkIdentity.HandleSyncList(cmdHash, netMsg.Reader);
			}
			else if (LogFilter.logWarn)
			{
				Debug.LogWarning("Did not find target for SyncList message for " + networkInstanceId);
			}
		}

		private static void OnClientAuthority(QSBNetworkMessage netMsg)
		{
			netMsg.ReadMessage<QSBClientAuthorityMessage>(s_ClientAuthorityMessage);
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"ClientScene::OnClientAuthority for  connectionId=",
					netMsg.Connection.connectionId,
					" netId: ",
					s_ClientAuthorityMessage.netId
				}));
			}
			if (s_NetworkScene.GetNetworkIdentity(s_ClientAuthorityMessage.netId, out var networkIdentity))
			{
				networkIdentity.HandleClientAuthority(s_ClientAuthorityMessage.authority);
			}
		}

		private static void OnOwnerMessage(QSBNetworkMessage netMsg)
		{
			netMsg.ReadMessage<QSBOwnerMessage>(s_OwnerMessage);
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"ClientScene::OnOwnerMessage - connectionId=",
					netMsg.Connection.connectionId,
					" netId: ",
					s_OwnerMessage.NetId
				}));
			}
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

		private static void CheckForOwner(QSBNetworkIdentity uv)
		{
			var i = 0;
			while (i < s_PendingOwnerIds.Count)
			{
				var pendingOwner = s_PendingOwnerIds[i];
				if (pendingOwner.netId == uv.NetId)
				{
					uv.SetConnectionToServer(readyConnection);
					uv.SetLocalPlayer(pendingOwner.playerControllerId);
					Debug.Log("ClientScene::OnOwnerMessage - player=" + uv.gameObject.name);
					if (readyConnection.connectionId < 0)
					{
						if (LogFilter.logError)
						{
							Debug.LogError("Owner message received on a local client.");
						}
						break;
					}
					InternalAddPlayer(uv, pendingOwner.playerControllerId);
					s_PendingOwnerIds.RemoveAt(i);
					break;
				}
				else
				{
					i++;
				}
			}
		}

		private static bool s_IsSpawnFinished;

		private static QSBNetworkScene s_NetworkScene = new QSBNetworkScene();

		private static QSBObjectSpawnSceneMessage s_ObjectSpawnSceneMessage = new QSBObjectSpawnSceneMessage();

		private static QSBObjectSpawnFinishedMessage s_ObjectSpawnFinishedMessage = new QSBObjectSpawnFinishedMessage();

		private static QSBObjectDestroyMessage s_ObjectDestroyMessage = new QSBObjectDestroyMessage();

		private static QSBObjectSpawnMessage s_ObjectSpawnMessage = new QSBObjectSpawnMessage();

		private static QSBOwnerMessage s_OwnerMessage = new QSBOwnerMessage();

		private static QSBClientAuthorityMessage s_ClientAuthorityMessage = new QSBClientAuthorityMessage();

		public const int ReconnectIdInvalid = -1;

		public const int ReconnectIdHost = 0;

		private static QSBPeerInfoMessage[] s_Peers;

		private static List<PendingOwner> s_PendingOwnerIds = new List<PendingOwner>();

		private struct PendingOwner
		{
			public NetworkInstanceId netId;

			public short playerControllerId;
		}
	}
}