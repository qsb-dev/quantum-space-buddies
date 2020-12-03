using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Types;

namespace QSB.QuantumUNET
{
	public class QSBNetworkMigrationManager : MonoBehaviour
	{
		private void AddPendingPlayer(GameObject obj, int connectionId, NetworkInstanceId netId, short playerControllerId)
		{
			if (!this.m_PendingPlayers.ContainsKey(connectionId))
			{
				ConnectionPendingPlayers value = default(ConnectionPendingPlayers);
				value.players = new List<PendingPlayerInfo>();
				this.m_PendingPlayers[connectionId] = value;
			}
			PendingPlayerInfo item = default(PendingPlayerInfo);
			item.netId = netId;
			item.playerControllerId = playerControllerId;
			item.obj = obj;
			this.m_PendingPlayers[connectionId].players.Add(item);
		}

		private GameObject FindPendingPlayer(int connectionId, NetworkInstanceId netId, short playerControllerId)
		{
			if (this.m_PendingPlayers.ContainsKey(connectionId))
			{
				for (int i = 0; i < this.m_PendingPlayers[connectionId].players.Count; i++)
				{
					PendingPlayerInfo pendingPlayerInfo = this.m_PendingPlayers[connectionId].players[i];
					if (pendingPlayerInfo.netId == netId && pendingPlayerInfo.playerControllerId == playerControllerId)
					{
						return pendingPlayerInfo.obj;
					}
				}
			}
			return null;
		}

		private void RemovePendingPlayer(int connectionId)
		{
			this.m_PendingPlayers.Remove(connectionId);
		}

		public bool hostMigration
		{
			get
			{
				return this.m_HostMigration;
			}
			set
			{
				this.m_HostMigration = value;
			}
		}

		public bool showGUI
		{
			get
			{
				return this.m_ShowGUI;
			}
			set
			{
				this.m_ShowGUI = value;
			}
		}

		public int offsetX
		{
			get
			{
				return this.m_OffsetX;
			}
			set
			{
				this.m_OffsetX = value;
			}
		}

		public int offsetY
		{
			get
			{
				return this.m_OffsetY;
			}
			set
			{
				this.m_OffsetY = value;
			}
		}

		public QSBNetworkClient client
		{
			get
			{
				return this.m_Client;
			}
		}

		public bool waitingToBecomeNewHost
		{
			get
			{
				return this.m_WaitingToBecomeNewHost;
			}
			set
			{
				this.m_WaitingToBecomeNewHost = value;
			}
		}

		public bool waitingReconnectToNewHost
		{
			get
			{
				return this.m_WaitingReconnectToNewHost;
			}
			set
			{
				this.m_WaitingReconnectToNewHost = value;
			}
		}

		public bool disconnectedFromHost
		{
			get
			{
				return this.m_DisconnectedFromHost;
			}
		}

		public bool hostWasShutdown
		{
			get
			{
				return this.m_HostWasShutdown;
			}
		}

		public MatchInfo matchInfo
		{
			get
			{
				return this.m_MatchInfo;
			}
		}

		public int oldServerConnectionId
		{
			get
			{
				return this.m_OldServerConnectionId;
			}
		}

		public string newHostAddress
		{
			get
			{
				return this.m_NewHostAddress;
			}
			set
			{
				this.m_NewHostAddress = value;
			}
		}

		public PeerInfoMessage[] peers
		{
			get
			{
				return this.m_Peers;
			}
		}

		public Dictionary<int, ConnectionPendingPlayers> pendingPlayers
		{
			get
			{
				return this.m_PendingPlayers;
			}
		}

		private void Start()
		{
			this.Reset(-1);
		}

		public void Reset(int reconnectId)
		{
			this.m_OldServerConnectionId = -1;
			this.m_WaitingToBecomeNewHost = false;
			this.m_WaitingReconnectToNewHost = false;
			this.m_DisconnectedFromHost = false;
			this.m_HostWasShutdown = false;
			QSBClientScene.SetReconnectId(reconnectId, this.m_Peers);
			if (QSBNetworkManager.singleton != null)
			{
				QSBNetworkManager.singleton.SetupMigrationManager(this);
			}
		}

		internal void AssignAuthorityCallback(QSBNetworkConnection conn, QSBNetworkIdentity uv, bool authorityState)
		{
			PeerAuthorityMessage peerAuthorityMessage = new PeerAuthorityMessage();
			peerAuthorityMessage.connectionId = conn.connectionId;
			peerAuthorityMessage.netId = uv.NetId;
			peerAuthorityMessage.authorityState = authorityState;
			if (LogFilter.logDebug)
			{
				Debug.Log("AssignAuthorityCallback send for netId" + uv.NetId);
			}
			for (int i = 0; i < QSBNetworkServer.connections.Count; i++)
			{
				QSBNetworkConnection networkConnection = QSBNetworkServer.connections[i];
				if (networkConnection != null)
				{
					networkConnection.Send(18, peerAuthorityMessage);
				}
			}
		}

		public void Initialize(QSBNetworkClient newClient, MatchInfo newMatchInfo)
		{
			Debug.Log("NetworkMigrationManager initialize");
			this.m_Client = newClient;
			this.m_MatchInfo = newMatchInfo;
			newClient.RegisterHandlerSafe(11, new QSBNetworkMessageDelegate(this.OnPeerInfo));
			newClient.RegisterHandlerSafe(18, new QSBNetworkMessageDelegate(this.OnPeerClientAuthority));
			QSBNetworkIdentity.clientAuthorityCallback = new QSBNetworkIdentity.ClientAuthorityCallback(this.AssignAuthorityCallback);
		}

		public void DisablePlayerObjects()
		{
			Debug.Log("NetworkMigrationManager DisablePlayerObjects");
			if (this.m_Peers != null)
			{
				for (int i = 0; i < this.m_Peers.Length; i++)
				{
					PeerInfoMessage peerInfoMessage = this.m_Peers[i];
					if (peerInfoMessage.playerIds != null)
					{
						for (int j = 0; j < peerInfoMessage.playerIds.Length; j++)
						{
							PeerInfoPlayer peerInfoPlayer = peerInfoMessage.playerIds[j];
							Debug.Log(string.Concat(new object[]
							{
								"DisablePlayerObjects disable player for ",
								peerInfoMessage.address,
								" netId:",
								peerInfoPlayer.netId,
								" control:",
								peerInfoPlayer.playerControllerId
							}));
							GameObject gameObject = ClientScene.FindLocalObject(peerInfoPlayer.netId);
							if (gameObject != null)
							{
								gameObject.SetActive(false);
								this.AddPendingPlayer(gameObject, peerInfoMessage.connectionId, peerInfoPlayer.netId, peerInfoPlayer.playerControllerId);
							}
							else if (LogFilter.logWarn)
							{
								Debug.LogWarning(string.Concat(new object[]
								{
									"DisablePlayerObjects didnt find player Conn:",
									peerInfoMessage.connectionId,
									" NetId:",
									peerInfoPlayer.netId
								}));
							}
						}
					}
				}
			}
		}

		public void SendPeerInfo()
		{
			if (this.m_HostMigration)
			{
				PeerListMessage peerListMessage = new PeerListMessage();
				List<PeerInfoMessage> list = new List<PeerInfoMessage>();
				for (int i = 0; i < QSBNetworkServer.connections.Count; i++)
				{
					QSBNetworkConnection networkConnection = QSBNetworkServer.connections[i];
					if (networkConnection != null)
					{
						PeerInfoMessage peerInfoMessage = new PeerInfoMessage();
						string address;
						int port;
						NetworkID networkID;
						NodeID nodeID;
						byte b;
						NetworkTransport.GetConnectionInfo(QSBNetworkServer.serverHostId, networkConnection.connectionId, out address, out port, out networkID, out nodeID, out b);
						peerInfoMessage.connectionId = networkConnection.connectionId;
						peerInfoMessage.port = port;
						if (i == 0)
						{
							peerInfoMessage.port = QSBNetworkServer.listenPort;
							peerInfoMessage.isHost = true;
							peerInfoMessage.address = "<host>";
						}
						else
						{
							peerInfoMessage.address = address;
							peerInfoMessage.isHost = false;
						}
						List<PeerInfoPlayer> list2 = new List<PeerInfoPlayer>();
						for (int j = 0; j < networkConnection.PlayerControllers.Count; j++)
						{
							QSBPlayerController playerController = networkConnection.PlayerControllers[j];
							if (playerController != null && playerController.unetView != null)
							{
								PeerInfoPlayer item;
								item.netId = playerController.unetView.NetId;
								item.playerControllerId = playerController.unetView.PlayerControllerId;
								list2.Add(item);
							}
						}
						if (networkConnection.ClientOwnedObjects != null)
						{
							foreach (NetworkInstanceId netId in networkConnection.ClientOwnedObjects)
							{
								GameObject gameObject = QSBNetworkServer.FindLocalObject(netId);
								if (!(gameObject == null))
								{
									QSBNetworkIdentity component = gameObject.GetComponent<QSBNetworkIdentity>();
									if (component.PlayerControllerId == -1)
									{
										PeerInfoPlayer item2;
										item2.netId = netId;
										item2.playerControllerId = -1;
										list2.Add(item2);
									}
								}
							}
						}
						if (list2.Count > 0)
						{
							peerInfoMessage.playerIds = list2.ToArray();
						}
						list.Add(peerInfoMessage);
					}
				}
				peerListMessage.peers = list.ToArray();
				for (int k = 0; k < QSBNetworkServer.connections.Count; k++)
				{
					QSBNetworkConnection networkConnection2 = QSBNetworkServer.connections[k];
					if (networkConnection2 != null)
					{
						peerListMessage.oldServerConnectionId = networkConnection2.connectionId;
						networkConnection2.Send(11, peerListMessage);
					}
				}
			}
		}

		private void OnPeerClientAuthority(QSBNetworkMessage netMsg)
		{
			PeerAuthorityMessage peerAuthorityMessage = netMsg.ReadMessage<PeerAuthorityMessage>();
			if (LogFilter.logDebug)
			{
				Debug.Log("OnPeerClientAuthority for netId:" + peerAuthorityMessage.netId);
			}
			if (this.m_Peers != null)
			{
				for (int i = 0; i < this.m_Peers.Length; i++)
				{
					PeerInfoMessage peerInfoMessage = this.m_Peers[i];
					if (peerInfoMessage.connectionId == peerAuthorityMessage.connectionId)
					{
						if (peerInfoMessage.playerIds == null)
						{
							peerInfoMessage.playerIds = new PeerInfoPlayer[0];
						}
						if (peerAuthorityMessage.authorityState)
						{
							for (int j = 0; j < peerInfoMessage.playerIds.Length; j++)
							{
								if (peerInfoMessage.playerIds[j].netId == peerAuthorityMessage.netId)
								{
									return;
								}
							}
							PeerInfoPlayer item = default(PeerInfoPlayer);
							item.netId = peerAuthorityMessage.netId;
							item.playerControllerId = -1;
							peerInfoMessage.playerIds = new List<PeerInfoPlayer>(peerInfoMessage.playerIds)
							{
								item
							}.ToArray();
						}
						else
						{
							for (int k = 0; k < peerInfoMessage.playerIds.Length; k++)
							{
								if (peerInfoMessage.playerIds[k].netId == peerAuthorityMessage.netId)
								{
									List<PeerInfoPlayer> list = new List<PeerInfoPlayer>(peerInfoMessage.playerIds);
									list.RemoveAt(k);
									peerInfoMessage.playerIds = list.ToArray();
									break;
								}
							}
						}
					}
				}
				GameObject go = ClientScene.FindLocalObject(peerAuthorityMessage.netId);
				this.OnAuthorityUpdated(go, peerAuthorityMessage.connectionId, peerAuthorityMessage.authorityState);
			}
		}

		private void OnPeerInfo(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("OnPeerInfo");
			}
			netMsg.ReadMessage<PeerListMessage>(this.m_PeerListMessage);
			this.m_Peers = this.m_PeerListMessage.peers;
			this.m_OldServerConnectionId = this.m_PeerListMessage.oldServerConnectionId;
			for (int i = 0; i < this.m_Peers.Length; i++)
			{
				if (LogFilter.logDebug)
				{
					Debug.Log(string.Concat(new object[]
					{
						"peer conn ",
						this.m_Peers[i].connectionId,
						" your conn ",
						this.m_PeerListMessage.oldServerConnectionId
					}));
				}
				if (this.m_Peers[i].connectionId == this.m_PeerListMessage.oldServerConnectionId)
				{
					this.m_Peers[i].isYou = true;
					break;
				}
			}
			this.OnPeersUpdated(this.m_PeerListMessage);
		}

		private void OnServerReconnectPlayerMessage(QSBNetworkMessage netMsg)
		{
			ReconnectMessage reconnectMessage = netMsg.ReadMessage<ReconnectMessage>();
			Debug.Log(string.Concat(new object[]
			{
				"OnReconnectMessage: connId=",
				reconnectMessage.oldConnectionId,
				" playerControllerId:",
				reconnectMessage.playerControllerId,
				" netId:",
				reconnectMessage.netId
			}));
			GameObject gameObject = this.FindPendingPlayer(reconnectMessage.oldConnectionId, reconnectMessage.netId, reconnectMessage.playerControllerId);
			if (gameObject == null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError(string.Concat(new object[]
					{
						"OnReconnectMessage connId=",
						reconnectMessage.oldConnectionId,
						" player null for netId:",
						reconnectMessage.netId,
						" msg.playerControllerId:",
						reconnectMessage.playerControllerId
					}));
				}
			}
			else if (gameObject.activeSelf)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("OnReconnectMessage connId=" + reconnectMessage.oldConnectionId + " player already active?");
				}
			}
			else
			{
				if (LogFilter.logDebug)
				{
					Debug.Log("OnReconnectMessage: player=" + gameObject);
				}
				NetworkReader networkReader = null;
				if (reconnectMessage.msgSize != 0)
				{
					networkReader = new NetworkReader(reconnectMessage.msgData);
				}
				if (reconnectMessage.playerControllerId != -1)
				{
					if (networkReader == null)
					{
						this.OnServerReconnectPlayer(netMsg.conn, gameObject, reconnectMessage.oldConnectionId, reconnectMessage.playerControllerId);
					}
					else
					{
						this.OnServerReconnectPlayer(netMsg.conn, gameObject, reconnectMessage.oldConnectionId, reconnectMessage.playerControllerId, networkReader);
					}
				}
				else
				{
					this.OnServerReconnectObject(netMsg.conn, gameObject, reconnectMessage.oldConnectionId);
				}
			}
		}

		public bool ReconnectObjectForConnection(QSBNetworkConnection newConnection, GameObject oldObject, int oldConnectionId)
		{
			bool result;
			if (!QSBNetworkServer.active)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("ReconnectObjectForConnection must have active server");
				}
				result = false;
			}
			else
			{
				if (LogFilter.logDebug)
				{
					Debug.Log(string.Concat(new object[]
					{
						"ReconnectObjectForConnection: oldConnId=",
						oldConnectionId,
						" obj=",
						oldObject,
						" conn:",
						newConnection
					}));
				}
				if (!this.m_PendingPlayers.ContainsKey(oldConnectionId))
				{
					if (LogFilter.logError)
					{
						Debug.LogError("ReconnectObjectForConnection oldConnId=" + oldConnectionId + " not found.");
					}
					result = false;
				}
				else
				{
					oldObject.SetActive(true);
					oldObject.GetComponent<QSBNetworkIdentity>().SetNetworkInstanceId(new NetworkInstanceId(0U));
					if (!QSBNetworkServer.SpawnWithClientAuthority(oldObject, newConnection))
					{
						if (LogFilter.logError)
						{
							Debug.LogError("ReconnectObjectForConnection oldConnId=" + oldConnectionId + " SpawnWithClientAuthority failed.");
						}
						result = false;
					}
					else
					{
						result = true;
					}
				}
			}
			return result;
		}

		public bool ReconnectPlayerForConnection(QSBNetworkConnection newConnection, GameObject oldPlayer, int oldConnectionId, short playerControllerId)
		{
			bool result;
			if (!QSBNetworkServer.active)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("ReconnectPlayerForConnection must have active server");
				}
				result = false;
			}
			else
			{
				if (LogFilter.logDebug)
				{
					Debug.Log(string.Concat(new object[]
					{
						"ReconnectPlayerForConnection: oldConnId=",
						oldConnectionId,
						" player=",
						oldPlayer,
						" conn:",
						newConnection
					}));
				}
				if (!this.m_PendingPlayers.ContainsKey(oldConnectionId))
				{
					if (LogFilter.logError)
					{
						Debug.LogError("ReconnectPlayerForConnection oldConnId=" + oldConnectionId + " not found.");
					}
					result = false;
				}
				else
				{
					oldPlayer.SetActive(true);
					QSBNetworkServer.Spawn(oldPlayer);
					if (!QSBNetworkServer.AddPlayerForConnection(newConnection, oldPlayer, playerControllerId))
					{
						if (LogFilter.logError)
						{
							Debug.LogError("ReconnectPlayerForConnection oldConnId=" + oldConnectionId + " AddPlayerForConnection failed.");
						}
						result = false;
					}
					else
					{
						if (QSBNetworkServer.localClientActive)
						{
							this.SendPeerInfo();
						}
						result = true;
					}
				}
			}
			return result;
		}

		public bool LostHostOnClient(QSBNetworkConnection conn)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkMigrationManager client OnDisconnectedFromHost");
			}
			bool result;
			if (Application.platform == RuntimePlatform.WebGLPlayer)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("LostHostOnClient: Host migration not supported on WebGL");
				}
				result = false;
			}
			else if (this.m_Client == null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("NetworkMigrationManager LostHostOnHost client was never initialized.");
				}
				result = false;
			}
			else if (!this.m_HostMigration)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("NetworkMigrationManager LostHostOnHost migration not enabled.");
				}
				result = false;
			}
			else
			{
				this.m_DisconnectedFromHost = true;
				this.DisablePlayerObjects();
				byte b;
				NetworkTransport.Disconnect(this.m_Client.hostId, this.m_Client.connection.connectionId, out b);
				if (this.m_OldServerConnectionId != -1)
				{
					SceneChangeOption sceneChangeOption;
					this.OnClientDisconnectedFromHost(conn, out sceneChangeOption);
					result = (sceneChangeOption == SceneChangeOption.StayInOnlineScene);
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		public void LostHostOnHost()
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkMigrationManager LostHostOnHost");
			}
			if (Application.platform == RuntimePlatform.WebGLPlayer)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("LostHostOnHost: Host migration not supported on WebGL");
				}
			}
			else
			{
				this.OnServerHostShutdown();
				if (this.m_Peers == null)
				{
					if (LogFilter.logError)
					{
						Debug.LogError("NetworkMigrationManager LostHostOnHost no peers");
					}
				}
				else if (this.m_Peers.Length != 1)
				{
					this.m_HostWasShutdown = true;
				}
			}
		}

		public bool BecomeNewHost(int port)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkMigrationManager BecomeNewHost " + this.m_MatchInfo);
			}
			QSBNetworkServer.RegisterHandler(47, new QSBNetworkMessageDelegate(this.OnServerReconnectPlayerMessage));
			QSBNetworkClient networkClient = QSBNetworkServer.BecomeHost(this.m_Client, port, this.m_MatchInfo, this.oldServerConnectionId, this.peers);
			bool result;
			if (networkClient != null)
			{
				if (QSBNetworkManager.singleton != null)
				{
					QSBNetworkManager.singleton.RegisterServerMessages();
					QSBNetworkManager.singleton.UseExternalClient(networkClient);
				}
				else
				{
					Debug.LogWarning("MigrationManager BecomeNewHost - No NetworkManager.");
				}
				networkClient.RegisterHandlerSafe(11, new QSBNetworkMessageDelegate(this.OnPeerInfo));
				this.RemovePendingPlayer(this.m_OldServerConnectionId);
				this.Reset(-1);
				this.SendPeerInfo();
				result = true;
			}
			else
			{
				if (LogFilter.logError)
				{
					Debug.LogError("NetworkServer.BecomeHost failed");
				}
				result = false;
			}
			return result;
		}

		protected virtual void OnClientDisconnectedFromHost(QSBNetworkConnection conn, out SceneChangeOption sceneChange)
		{
			sceneChange = SceneChangeOption.StayInOnlineScene;
		}

		protected virtual void OnServerHostShutdown()
		{
		}

		protected virtual void OnServerReconnectPlayer(QSBNetworkConnection newConnection, GameObject oldPlayer, int oldConnectionId, short playerControllerId)
		{
			this.ReconnectPlayerForConnection(newConnection, oldPlayer, oldConnectionId, playerControllerId);
		}

		protected virtual void OnServerReconnectPlayer(QSBNetworkConnection newConnection, GameObject oldPlayer, int oldConnectionId, short playerControllerId, NetworkReader extraMessageReader)
		{
			this.ReconnectPlayerForConnection(newConnection, oldPlayer, oldConnectionId, playerControllerId);
		}

		protected virtual void OnServerReconnectObject(QSBNetworkConnection newConnection, GameObject oldObject, int oldConnectionId)
		{
			this.ReconnectObjectForConnection(newConnection, oldObject, oldConnectionId);
		}

		protected virtual void OnPeersUpdated(PeerListMessage peers)
		{
			Debug.Log("NetworkMigrationManager NumPeers " + peers.peers.Length);
		}

		protected virtual void OnAuthorityUpdated(GameObject go, int connectionId, bool authorityState)
		{
			Debug.Log(string.Concat(new object[]
			{
				"NetworkMigrationManager OnAuthorityUpdated for ",
				go,
				" conn:",
				connectionId,
				" state:",
				authorityState
			}));
		}

		public virtual bool FindNewHost(out PeerInfoMessage newHostInfo, out bool youAreNewHost)
		{
			bool result;
			if (this.m_Peers == null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("NetworkMigrationManager FindLowestHost no peers");
				}
				newHostInfo = null;
				youAreNewHost = false;
				result = false;
			}
			else
			{
				Debug.Log("NetworkMigrationManager FindLowestHost");
				newHostInfo = new PeerInfoMessage();
				newHostInfo.connectionId = 50000;
				newHostInfo.address = "";
				newHostInfo.port = 0;
				int num = -1;
				youAreNewHost = false;
				if (this.m_Peers == null)
				{
					result = false;
				}
				else
				{
					for (int i = 0; i < this.m_Peers.Length; i++)
					{
						PeerInfoMessage peerInfoMessage = this.m_Peers[i];
						if (peerInfoMessage.connectionId != 0)
						{
							if (!peerInfoMessage.isHost)
							{
								if (peerInfoMessage.isYou)
								{
									num = peerInfoMessage.connectionId;
								}
								if (peerInfoMessage.connectionId < newHostInfo.connectionId)
								{
									newHostInfo = peerInfoMessage;
								}
							}
						}
					}
					if (newHostInfo.connectionId == 50000)
					{
						result = false;
					}
					else
					{
						if (newHostInfo.connectionId == num)
						{
							youAreNewHost = true;
						}
						Debug.Log("FindNewHost new host is " + newHostInfo.address);
						result = true;
					}
				}
			}
			return result;
		}

		private void OnGUIHost()
		{
			int num = this.m_OffsetY;
			GUI.Label(new Rect((float)this.m_OffsetX, (float)num, 200f, 40f), "Host Was Shutdown ID(" + this.m_OldServerConnectionId + ")");
			num += 25;
			if (Application.platform == RuntimePlatform.WebGLPlayer)
			{
				GUI.Label(new Rect((float)this.m_OffsetX, (float)num, 200f, 40f), "Host Migration not supported for WebGL");
			}
			else
			{
				if (this.m_WaitingReconnectToNewHost)
				{
					if (GUI.Button(new Rect((float)this.m_OffsetX, (float)num, 200f, 20f), "Reconnect as Client"))
					{
						this.Reset(0);
						if (NetworkManager.singleton != null)
						{
							NetworkManager.singleton.networkAddress = GUI.TextField(new Rect((float)(this.m_OffsetX + 100), (float)num, 95f, 20f), NetworkManager.singleton.networkAddress);
							NetworkManager.singleton.StartClient();
						}
						else
						{
							Debug.LogWarning("MigrationManager Old Host Reconnect - No NetworkManager.");
						}
					}
					num += 25;
				}
				else
				{
					if (GUI.Button(new Rect((float)this.m_OffsetX, (float)num, 200f, 20f), "Pick New Host"))
					{
						bool flag;
						if (this.FindNewHost(out this.m_NewHostInfo, out flag))
						{
							this.m_NewHostAddress = this.m_NewHostInfo.address;
							if (flag)
							{
								Debug.LogWarning("MigrationManager FindNewHost - new host is self?");
							}
							else
							{
								this.m_WaitingReconnectToNewHost = true;
							}
						}
					}
					num += 25;
				}
				if (GUI.Button(new Rect((float)this.m_OffsetX, (float)num, 200f, 20f), "Leave Game"))
				{
					if (NetworkManager.singleton != null)
					{
						NetworkManager.singleton.SetupMigrationManager(null);
						NetworkManager.singleton.StopHost();
					}
					else
					{
						Debug.LogWarning("MigrationManager Old Host LeaveGame - No NetworkManager.");
					}
					this.Reset(-1);
				}
				num += 25;
			}
		}

		private void OnGUIClient()
		{
			int num = this.m_OffsetY;
			GUI.Label(new Rect((float)this.m_OffsetX, (float)num, 200f, 40f), "Lost Connection To Host ID(" + this.m_OldServerConnectionId + ")");
			num += 25;
			if (Application.platform == RuntimePlatform.WebGLPlayer)
			{
				GUI.Label(new Rect((float)this.m_OffsetX, (float)num, 200f, 40f), "Host Migration not supported for WebGL");
			}
			else
			{
				if (this.m_WaitingToBecomeNewHost)
				{
					GUI.Label(new Rect((float)this.m_OffsetX, (float)num, 200f, 40f), "You are the new host");
					num += 25;
					if (GUI.Button(new Rect((float)this.m_OffsetX, (float)num, 200f, 20f), "Start As Host"))
					{
						if (NetworkManager.singleton != null)
						{
							this.BecomeNewHost(NetworkManager.singleton.networkPort);
						}
						else
						{
							Debug.LogWarning("MigrationManager Client BecomeNewHost - No NetworkManager.");
						}
					}
					num += 25;
				}
				else if (this.m_WaitingReconnectToNewHost)
				{
					GUI.Label(new Rect((float)this.m_OffsetX, (float)num, 200f, 40f), "New host is " + this.m_NewHostAddress);
					num += 25;
					if (GUI.Button(new Rect((float)this.m_OffsetX, (float)num, 200f, 20f), "Reconnect To New Host"))
					{
						this.Reset(this.m_OldServerConnectionId);
						if (NetworkManager.singleton != null)
						{
							NetworkManager.singleton.networkAddress = this.m_NewHostAddress;
							NetworkManager.singleton.client.ReconnectToNewHost(this.m_NewHostAddress, NetworkManager.singleton.networkPort);
						}
						else
						{
							Debug.LogWarning("MigrationManager Client reconnect - No NetworkManager.");
						}
					}
					num += 25;
				}
				else
				{
					if (GUI.Button(new Rect((float)this.m_OffsetX, (float)num, 200f, 20f), "Pick New Host"))
					{
						bool flag;
						if (this.FindNewHost(out this.m_NewHostInfo, out flag))
						{
							this.m_NewHostAddress = this.m_NewHostInfo.address;
							if (flag)
							{
								this.m_WaitingToBecomeNewHost = true;
							}
							else
							{
								this.m_WaitingReconnectToNewHost = true;
							}
						}
					}
					num += 25;
				}
				if (GUI.Button(new Rect((float)this.m_OffsetX, (float)num, 200f, 20f), "Leave Game"))
				{
					if (NetworkManager.singleton != null)
					{
						NetworkManager.singleton.SetupMigrationManager(null);
						NetworkManager.singleton.StopHost();
					}
					else
					{
						Debug.LogWarning("MigrationManager Client LeaveGame - No NetworkManager.");
					}
					this.Reset(-1);
				}
				num += 25;
			}
		}

		private void OnGUI()
		{
			if (this.m_ShowGUI)
			{
				if (this.m_HostWasShutdown)
				{
					this.OnGUIHost();
				}
				else if (this.m_DisconnectedFromHost && this.m_OldServerConnectionId != -1)
				{
					this.OnGUIClient();
				}
			}
		}

		[SerializeField]
		private bool m_HostMigration = true;

		[SerializeField]
		private bool m_ShowGUI = true;

		[SerializeField]
		private int m_OffsetX = 10;

		[SerializeField]
		private int m_OffsetY = 300;

		private QSBNetworkClient m_Client;

		private bool m_WaitingToBecomeNewHost;

		private bool m_WaitingReconnectToNewHost;

		private bool m_DisconnectedFromHost;

		private bool m_HostWasShutdown;

		private MatchInfo m_MatchInfo;

		private int m_OldServerConnectionId = -1;

		private string m_NewHostAddress;

		private PeerInfoMessage m_NewHostInfo = new PeerInfoMessage();

		private PeerListMessage m_PeerListMessage = new PeerListMessage();

		private PeerInfoMessage[] m_Peers;

		private Dictionary<int, ConnectionPendingPlayers> m_PendingPlayers = new Dictionary<int, ConnectionPendingPlayers>();

		public enum SceneChangeOption
		{
			StayInOnlineScene,
			SwitchToOfflineScene
		}

		public struct PendingPlayerInfo
		{
			public NetworkInstanceId netId;

			public short playerControllerId;

			public GameObject obj;
		}

		public struct ConnectionPendingPlayers
		{
			public List<PendingPlayerInfo> players;
		}
	}
}