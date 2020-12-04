using OWML.Common;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	public sealed class QSBNetworkIdentity : MonoBehaviour
	{
		public bool IsClient { get; private set; }
		public bool IsServer => m_IsServer && QSBNetworkServer.active && m_IsServer;
		public bool HasAuthority { get; private set; }
		public NetworkInstanceId NetId { get; private set; }
		public NetworkSceneId SceneId => m_SceneId;
		public QSBNetworkConnection ClientAuthorityOwner { get; private set; }
		public NetworkHash128 AssetId => m_AssetId;
		public bool IsLocalPlayer { get; private set; }
		public short PlayerControllerId { get; private set; } = -1;
		public QSBNetworkConnection ConnectionToServer { get; private set; }
		public QSBNetworkConnection ConnectionToClient { get; private set; }

		public bool ServerOnly
		{
			get
			{
				return m_ServerOnly;
			}
			set
			{
				m_ServerOnly = value;
			}
		}

		public bool LocalPlayerAuthority
		{
			get
			{
				return m_LocalPlayerAuthority;
			}
			set
			{
				m_LocalPlayerAuthority = value;
			}
		}

		internal void SetDynamicAssetId(NetworkHash128 newAssetId)
		{
			if (!m_AssetId.IsValid() || m_AssetId.Equals(newAssetId))
			{
				m_AssetId = newAssetId;
			}
			else
			{
				Debug.LogWarning("SetDynamicAssetId object already has an assetId <" + m_AssetId + ">");
			}
		}

		internal void SetClientOwner(QSBNetworkConnection conn)
		{
			if (ClientAuthorityOwner != null)
			{
				Debug.LogError("SetClientOwner m_ClientAuthorityOwner already set!");
			}
			ClientAuthorityOwner = conn;
			ClientAuthorityOwner.AddOwnedObject(this);
		}

		internal void ClearClientOwner() => ClientAuthorityOwner = null;

		internal void ForceAuthority(bool authority)
		{
			if (HasAuthority != authority)
			{
				HasAuthority = authority;
				if (authority)
				{
					OnStartAuthority();
				}
				else
				{
					OnStopAuthority();
				}
			}
		}

		public ReadOnlyCollection<QSBNetworkConnection> Observers
		{
			get
			{
				ReadOnlyCollection<QSBNetworkConnection> result;
				if (m_Observers == null)
				{
					result = null;
				}
				else
				{
					result = new ReadOnlyCollection<QSBNetworkConnection>(m_Observers);
				}
				return result;
			}
		}

		internal static NetworkInstanceId GetNextNetworkId()
		{
			var value = s_NextNetworkId;
			s_NextNetworkId += 1U;
			return new NetworkInstanceId(value);
		}

		private void CacheBehaviours()
		{
			if (m_NetworkBehaviours == null)
			{
				m_NetworkBehaviours = base.GetComponents<QSBNetworkBehaviour>();
			}
		}

		internal static void AddNetworkId(uint id)
		{
			if (id >= s_NextNetworkId)
			{
				s_NextNetworkId = id + 1U;
			}
		}

		internal void SetNetworkInstanceId(NetworkInstanceId newNetId)
		{
			NetId = newNetId;
			if (newNetId.Value == 0U)
			{
				m_IsServer = false;
			}
		}

		public void ForceSceneId(int newSceneId) => m_SceneId = new NetworkSceneId((uint)newSceneId);

		internal void UpdateClientServer(bool isClientFlag, bool isServerFlag)
		{
			IsClient = (IsClient || isClientFlag);
			m_IsServer = (m_IsServer || isServerFlag);
		}

		internal void SetNotLocalPlayer()
		{
			IsLocalPlayer = false;
			if (!QSBNetworkServer.active || !QSBNetworkServer.localClientActive)
			{
				HasAuthority = false;
			}
		}

		internal void RemoveObserverInternal(QSBNetworkConnection conn)
		{
			if (m_Observers != null)
			{
				m_Observers.Remove(conn);
				m_ObserverConnections.Remove(conn.connectionId);
			}
		}

		private void OnDestroy()
		{
			if (m_IsServer && QSBNetworkServer.active)
			{
				QSBNetworkServer.Destroy(base.gameObject);
			}
		}

		internal void OnStartServer(bool allowNonZeroNetId)
		{
			if (!m_IsServer)
			{
				m_IsServer = true;
				if (m_LocalPlayerAuthority)
				{
					HasAuthority = false;
				}
				else
				{
					HasAuthority = true;
				}
				m_Observers = new List<QSBNetworkConnection>();
				m_ObserverConnections = new HashSet<int>();
				CacheBehaviours();
				if (NetId.IsEmpty())
				{
					NetId = GetNextNetworkId();
				}
				else if (!allowNonZeroNetId)
				{
					DebugLog.DebugWrite($"netid is {NetId}");
					Debug.LogError(string.Concat(new object[]
					{
						"Object has non-zero netId ",
						NetId,
						" for ",
						gameObject
					}));
					return;
				}
				DebugLog.DebugWrite($"OnStartServer {gameObject} GUID:{NetId}");
				QSBNetworkServer.instance.SetLocalObjectOnServer(NetId, gameObject);
				for (var i = 0; i < m_NetworkBehaviours.Length; i++)
				{
					var networkBehaviour = m_NetworkBehaviours[i];
					try
					{
						networkBehaviour.OnStartServer();
					}
					catch (Exception ex)
					{
						Debug.LogError("Exception in OnStartServer:" + ex.Message + " " + ex.StackTrace);
					}
				}
				if (QSBNetworkClient.active && QSBNetworkServer.localClientActive)
				{
					QSBClientScene.SetLocalObject(NetId, base.gameObject);
					OnStartClient();
				}
				if (HasAuthority)
				{
					OnStartAuthority();
				}
			}
		}

		internal void OnStartClient()
		{
			if (!IsClient)
			{
				IsClient = true;
			}
			CacheBehaviours();
			Debug.Log(string.Concat(new object[]
			{
				"OnStartClient ",
				base.gameObject,
				" GUID:",
				NetId,
				" localPlayerAuthority:",
				LocalPlayerAuthority
			}));
			for (var i = 0; i < m_NetworkBehaviours.Length; i++)
			{
				var networkBehaviour = m_NetworkBehaviours[i];
				try
				{
					networkBehaviour.PreStartClient();
					networkBehaviour.OnStartClient();
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception in OnStartClient:" + ex.Message + " " + ex.StackTrace);
				}
			}
		}

		internal void OnStartAuthority()
		{
			for (var i = 0; i < m_NetworkBehaviours.Length; i++)
			{
				var networkBehaviour = m_NetworkBehaviours[i];
				try
				{
					networkBehaviour.OnStartAuthority();
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception in OnStartAuthority:" + ex.Message + " " + ex.StackTrace);
				}
			}
		}

		internal void OnStopAuthority()
		{
			for (var i = 0; i < m_NetworkBehaviours.Length; i++)
			{
				var networkBehaviour = m_NetworkBehaviours[i];
				try
				{
					networkBehaviour.OnStopAuthority();
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception in OnStopAuthority:" + ex.Message + " " + ex.StackTrace);
				}
			}
		}

		internal void OnSetLocalVisibility(bool vis)
		{
			for (var i = 0; i < m_NetworkBehaviours.Length; i++)
			{
				var networkBehaviour = m_NetworkBehaviours[i];
				try
				{
					networkBehaviour.OnSetLocalVisibility(vis);
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception in OnSetLocalVisibility:" + ex.Message + " " + ex.StackTrace);
				}
			}
		}

		internal bool OnCheckObserver(QSBNetworkConnection conn)
		{
			for (var i = 0; i < m_NetworkBehaviours.Length; i++)
			{
				var networkBehaviour = m_NetworkBehaviours[i];
				try
				{
					if (!networkBehaviour.OnCheckObserver(conn))
					{
						return false;
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception in OnCheckObserver:" + ex.Message + " " + ex.StackTrace);
				}
			}
			return true;
		}

		internal void UNetSerializeAllVars(QSBNetworkWriter writer)
		{
			DebugLog.DebugWrite($"Sync all vars (NetId:{NetId}, Gameobject:{gameObject.name})");
			for (var i = 0; i < m_NetworkBehaviours.Length; i++)
			{
				var networkBehaviour = m_NetworkBehaviours[i];
				networkBehaviour.OnSerialize(writer, true);
			}
		}

		internal void HandleClientAuthority(bool authority)
		{
			if (!LocalPlayerAuthority)
			{
				Debug.LogError("HandleClientAuthority " + base.gameObject + " does not have localPlayerAuthority");
			}
			else
			{
				ForceAuthority(authority);
			}
		}

		private bool GetInvokeComponent(int cmdHash, Type invokeClass, out QSBNetworkBehaviour invokeComponent)
		{
			QSBNetworkBehaviour networkBehaviour = null;
			for (var i = 0; i < m_NetworkBehaviours.Length; i++)
			{
				var networkBehaviour2 = m_NetworkBehaviours[i];
				if (networkBehaviour2.GetType() == invokeClass || networkBehaviour2.GetType().IsSubclassOf(invokeClass))
				{
					networkBehaviour = networkBehaviour2;
					break;
				}
			}
			bool result;
			if (networkBehaviour == null)
			{
				var cmdHashHandlerName = QSBNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				Debug.LogError(string.Concat(new object[]
				{
					"Found no behaviour for incoming [",
					cmdHashHandlerName,
					"] on ",
					base.gameObject,
					",  the server and client should have the same NetworkBehaviour instances [netId=",
					NetId,
					"]."
				}));
				invokeComponent = null;
				result = false;
			}
			else
			{
				invokeComponent = networkBehaviour;
				result = true;
			}
			return result;
		}

		internal void HandleSyncEvent(int cmdHash, QSBNetworkReader reader)
		{
			if (base.gameObject == null)
			{
				var cmdHashHandlerName = QSBNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				Debug.LogWarning(string.Concat(new object[]
				{
					"SyncEvent [",
					cmdHashHandlerName,
					"] received for deleted object [netId=",
					NetId,
					"]"
				}));
			}
			else if (!QSBNetworkBehaviour.GetInvokerForHashSyncEvent(cmdHash, out var invokeClass, out var cmdDelegate))
			{
				var cmdHashHandlerName2 = QSBNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				Debug.LogError(string.Concat(new object[]
				{
					"Found no receiver for incoming [",
					cmdHashHandlerName2,
					"] on ",
					base.gameObject,
					",  the server and client should have the same NetworkBehaviour instances [netId=",
					NetId,
					"]."
				}));
			}
			else if (!GetInvokeComponent(cmdHash, invokeClass, out var obj))
			{
				var cmdHashHandlerName3 = QSBNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				Debug.LogWarning(string.Concat(new object[]
				{
					"SyncEvent [",
					cmdHashHandlerName3,
					"] handler not found [netId=",
					NetId,
					"]"
				}));
			}
			else
			{
				cmdDelegate(obj, reader);
			}
		}

		internal void HandleSyncList(int cmdHash, QSBNetworkReader reader)
		{
			if (base.gameObject == null)
			{
				var cmdHashHandlerName = QSBNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				Debug.LogWarning(string.Concat(new object[]
				{
					"SyncList [",
					cmdHashHandlerName,
					"] received for deleted object [netId=",
					NetId,
					"]"
				}));
			}
			else if (!QSBNetworkBehaviour.GetInvokerForHashSyncList(cmdHash, out var invokeClass, out var cmdDelegate))
			{
				var cmdHashHandlerName2 = QSBNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				Debug.LogError(string.Concat(new object[]
				{
					"Found no receiver for incoming [",
					cmdHashHandlerName2,
					"] on ",
					base.gameObject,
					",  the server and client should have the same NetworkBehaviour instances [netId=",
					NetId,
					"]."
				}));
			}
			else if (!GetInvokeComponent(cmdHash, invokeClass, out var obj))
			{
				var cmdHashHandlerName3 = QSBNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				Debug.LogWarning(string.Concat(new object[]
				{
					"SyncList [",
					cmdHashHandlerName3,
					"] handler not found [netId=",
					NetId,
					"]"
				}));
			}
			else
			{
				cmdDelegate(obj, reader);
			}
		}

		internal void HandleCommand(int cmdHash, QSBNetworkReader reader)
		{
			if (base.gameObject == null)
			{
				var cmdHashHandlerName = QSBNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				Debug.LogWarning(string.Concat(new object[]
				{
					"Command [",
					cmdHashHandlerName,
					"] received for deleted object [netId=",
					NetId,
					"]"
				}));
			}
			else if (!QSBNetworkBehaviour.GetInvokerForHashCommand(cmdHash, out var invokeClass, out var cmdDelegate))
			{
				var cmdHashHandlerName2 = QSBNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				Debug.LogError(string.Concat(new object[]
				{
					"Found no receiver for incoming [",
					cmdHashHandlerName2,
					"] on ",
					base.gameObject,
					",  the server and client should have the same NetworkBehaviour instances [netId=",
					NetId,
					"]."
				}));
			}
			else if (!GetInvokeComponent(cmdHash, invokeClass, out var obj))
			{
				var cmdHashHandlerName3 = QSBNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				Debug.LogWarning(string.Concat(new object[]
				{
					"Command [",
					cmdHashHandlerName3,
					"] handler not found [netId=",
					NetId,
					"]"
				}));
			}
			else
			{
				cmdDelegate(obj, reader);
			}
		}

		internal void HandleRPC(int cmdHash, QSBNetworkReader reader)
		{
			if (base.gameObject == null)
			{
				var cmdHashHandlerName = QSBNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				Debug.LogWarning(string.Concat(new object[]
				{
					"ClientRpc [",
					cmdHashHandlerName,
					"] received for deleted object [netId=",
					NetId,
					"]"
				}));
			}
			else if (!QSBNetworkBehaviour.GetInvokerForHashClientRpc(cmdHash, out var invokeClass, out var cmdDelegate))
			{
				var cmdHashHandlerName2 = QSBNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				Debug.LogError(string.Concat(new object[]
				{
					"Found no receiver for incoming [",
					cmdHashHandlerName2,
					"] on ",
					base.gameObject,
					",  the server and client should have the same NetworkBehaviour instances [netId=",
					NetId,
					"]."
				}));
			}
			else if (!GetInvokeComponent(cmdHash, invokeClass, out var obj))
			{
				var cmdHashHandlerName3 = QSBNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				Debug.LogWarning(string.Concat(new object[]
				{
					"ClientRpc [",
					cmdHashHandlerName3,
					"] handler not found [netId=",
					NetId,
					"]"
				}));
			}
			else
			{
				cmdDelegate(obj, reader);
			}
		}

		internal void UNetUpdate()
		{
			var num = 0U;
			for (var i = 0; i < m_NetworkBehaviours.Length; i++)
			{
				var networkBehaviour = m_NetworkBehaviours[i];
				var dirtyChannel = networkBehaviour.GetDirtyChannel();
				if (dirtyChannel != -1)
				{
					num |= 1U << dirtyChannel;
				}
			}
			if (num != 0U)
			{
				var j = 0;
				while (j < QSBNetworkServer.numChannels)
				{
					if ((num & (1U << j)) != 0U)
					{
						DebugLog.DebugWrite("sending update vars message");
						s_UpdateWriter.StartMessage(8);
						s_UpdateWriter.Write(NetId);
						var flag = false;
						for (var k = 0; k < m_NetworkBehaviours.Length; k++)
						{
							var position = s_UpdateWriter.Position;
							var networkBehaviour2 = m_NetworkBehaviours[k];
							if (networkBehaviour2.GetDirtyChannel() != j)
							{
								networkBehaviour2.OnSerialize(s_UpdateWriter, false);
							}
							else
							{
								if (networkBehaviour2.OnSerialize(s_UpdateWriter, false))
								{
									networkBehaviour2.ClearAllDirtyBits();
									flag = true;
								}
								var maxPacketSize = QSBNetworkServer.maxPacketSize;
								if (s_UpdateWriter.Position - position > maxPacketSize)
								{
									Debug.LogWarning(string.Concat(new object[]
									{
										"Large state update of ",
										(int)(s_UpdateWriter.Position - position),
										" bytes for netId:",
										NetId,
										" from script:",
										networkBehaviour2
									}));
								}
							}
						}
						if (flag)
						{
							DebugLog.DebugWrite("FINISH MESSAGE");
							s_UpdateWriter.FinishMessage();
							QSBNetworkServer.SendWriterToReady(base.gameObject, s_UpdateWriter, j);
						}
					}
					IL_197:
					j++;
					continue;
					goto IL_197;
				}
			}
		}

		internal void OnUpdateVars(QSBNetworkReader reader, bool initialState)
		{
			if (initialState && m_NetworkBehaviours == null)
			{
				m_NetworkBehaviours = base.GetComponents<QSBNetworkBehaviour>();
			}
			for (var i = 0; i < m_NetworkBehaviours.Length; i++)
			{
				var networkBehaviour = m_NetworkBehaviours[i];
				networkBehaviour.OnDeserialize(reader, initialState);
			}
		}

		internal void SetLocalPlayer(short localPlayerControllerId)
		{
			DebugLog.DebugWrite($"SetLocalPlayer {localPlayerControllerId}", OWML.Common.MessageType.Warning);
			IsLocalPlayer = true;
			PlayerControllerId = localPlayerControllerId;
			var hasAuthority = this.HasAuthority;
			if (LocalPlayerAuthority)
			{
				HasAuthority = true;
			}
			for (var i = 0; i < m_NetworkBehaviours.Length; i++)
			{
				var networkBehaviour = m_NetworkBehaviours[i];
				networkBehaviour.OnStartLocalPlayer();
				if (LocalPlayerAuthority && !hasAuthority)
				{
					networkBehaviour.OnStartAuthority();
				}
			}
		}

		internal void SetConnectionToServer(QSBNetworkConnection conn) => ConnectionToServer = conn;

		internal void SetConnectionToClient(QSBNetworkConnection conn, short newPlayerControllerId)
		{
			PlayerControllerId = newPlayerControllerId;
			ConnectionToClient = conn;
		}

		internal void OnNetworkDestroy()
		{
			var num = 0;
			while (m_NetworkBehaviours != null && num < m_NetworkBehaviours.Length)
			{
				var networkBehaviour = m_NetworkBehaviours[num];
				networkBehaviour.OnNetworkDestroy();
				num++;
			}
			m_IsServer = false;
		}

		internal void ClearObservers()
		{
			if (m_Observers != null)
			{
				var count = m_Observers.Count;
				for (var i = 0; i < count; i++)
				{
					var networkConnection = m_Observers[i];
					networkConnection.RemoveFromVisList(this, true);
				}
				m_Observers.Clear();
				m_ObserverConnections.Clear();
			}
		}

		internal void AddObserver(QSBNetworkConnection conn)
		{
			if (m_Observers == null)
			{
				Debug.LogError("AddObserver for " + base.gameObject + " observer list is null");
			}
			else if (m_ObserverConnections.Contains(conn.connectionId))
			{
				Debug.Log(string.Concat(new object[]
				{
					"Duplicate observer ",
					conn.address,
					" added for ",
					base.gameObject
				}));
			}
			else
			{
				Debug.Log(string.Concat(new object[]
				{
					"Added observer ",
					conn.address,
					" added for ",
					base.gameObject
				}));
				m_Observers.Add(conn);
				m_ObserverConnections.Add(conn.connectionId);
				conn.AddToVisList(this);
			}
		}

		internal void RemoveObserver(QSBNetworkConnection conn)
		{
			if (m_Observers != null)
			{
				m_Observers.Remove(conn);
				m_ObserverConnections.Remove(conn.connectionId);
				conn.RemoveFromVisList(this, true);
			}
		}

		public void RebuildObservers(bool initialize)
		{
			if (m_Observers != null)
			{
				var flag = false;
				var flag2 = false;
				var hashSet = new HashSet<QSBNetworkConnection>();
				var hashSet2 = new HashSet<QSBNetworkConnection>(m_Observers);
				for (var i = 0; i < m_NetworkBehaviours.Length; i++)
				{
					var networkBehaviour = m_NetworkBehaviours[i];
					flag2 |= networkBehaviour.OnRebuildObservers(hashSet, initialize);
				}
				if (!flag2)
				{
					if (initialize)
					{
						for (var j = 0; j < QSBNetworkServer.connections.Count; j++)
						{
							var networkConnection = QSBNetworkServer.connections[j];
							if (networkConnection != null)
							{
								if (networkConnection.isReady)
								{
									AddObserver(networkConnection);
								}
							}
						}
						for (var k = 0; k < QSBNetworkServer.localConnections.Count; k++)
						{
							var networkConnection2 = QSBNetworkServer.localConnections[k];
							if (networkConnection2 != null)
							{
								if (networkConnection2.isReady)
								{
									AddObserver(networkConnection2);
								}
							}
						}
					}
				}
				else
				{
					foreach (var networkConnection3 in hashSet)
					{
						if (networkConnection3 != null)
						{
							if (!networkConnection3.isReady)
							{
								Debug.LogWarning(string.Concat(new object[]
								{
									"Observer is not ready for ",
									base.gameObject,
									" ",
									networkConnection3
								}));
							}
							else if (initialize || !hashSet2.Contains(networkConnection3))
							{
								networkConnection3.AddToVisList(this);
								Debug.Log(string.Concat(new object[]
								{
									"New Observer for ",
									base.gameObject,
									" ",
									networkConnection3
								}));
								flag = true;
							}
						}
					}
					foreach (var networkConnection4 in hashSet2)
					{
						if (!hashSet.Contains(networkConnection4))
						{
							networkConnection4.RemoveFromVisList(this, true);
							Debug.Log(string.Concat(new object[]
							{
								"Removed Observer for ",
								base.gameObject,
								" ",
								networkConnection4
							}));
							flag = true;
						}
					}
					if (initialize)
					{
						for (var l = 0; l < QSBNetworkServer.localConnections.Count; l++)
						{
							if (!hashSet.Contains(QSBNetworkServer.localConnections[l]))
							{
								OnSetLocalVisibility(false);
							}
						}
					}
					if (flag)
					{
						m_Observers = new List<QSBNetworkConnection>(hashSet);
						m_ObserverConnections.Clear();
						for (var m = 0; m < m_Observers.Count; m++)
						{
							m_ObserverConnections.Add(m_Observers[m].connectionId);
						}
					}
				}
			}
		}

		public bool RemoveClientAuthority(QSBNetworkConnection conn)
		{
			if (!IsServer)
			{
				DebugLog.ToConsole($"Warning - Cannot remove authority on client-side. (NetId:{NetId}, Gameobject:{gameObject.name})", MessageType.Warning);
				return false;
			}
			else if (ConnectionToClient != null)
			{
				Debug.LogError("RemoveClientAuthority cannot remove authority for a player object");
				return false;
			}
			else if (ClientAuthorityOwner == null)
			{
				Debug.LogError("RemoveClientAuthority for " + base.gameObject + " has no clientAuthority owner.");
				return false;
			}
			else if (ClientAuthorityOwner != conn)
			{
				Debug.LogError("RemoveClientAuthority for " + base.gameObject + " has different owner.");
				return false;
			}
			ClientAuthorityOwner.RemoveOwnedObject(this);
			ClientAuthorityOwner = null;
			ForceAuthority(true);
			conn.Send(15, new QSBClientAuthorityMessage
			{
				netId = NetId,
				authority = false
			});
			clientAuthorityCallback?.Invoke(conn, this, false);
			return true;
		}

		public bool AssignClientAuthority(QSBNetworkConnection conn)
		{
			if (!IsServer)
			{
				DebugLog.ToConsole($"Warning - Cannot assign authority on client-side. (NetId:{NetId}, Gameobject:{gameObject.name})", MessageType.Warning);
				return false;
			}
			else if (!LocalPlayerAuthority)
			{
				DebugLog.ToConsole($"Warning - Cannot assign authority on object without LocalPlayerAuthority. (NetId:{NetId}, Gameobject:{gameObject.name})", MessageType.Warning);
				return false;
			}
			else if (ClientAuthorityOwner != null && conn != ClientAuthorityOwner)
			{
				Debug.LogError("AssignClientAuthority for " + base.gameObject + " already has an owner. Use RemoveClientAuthority() first.");
				return false;
			}
			else if (conn == null)
			{
				Debug.LogError("AssignClientAuthority for " + base.gameObject + " owner cannot be null. Use RemoveClientAuthority() instead.");
				return false;
			}
			ClientAuthorityOwner = conn;
			ClientAuthorityOwner.AddOwnedObject(this);

			ForceAuthority(false);
			conn.Send(15, new QSBClientAuthorityMessage
			{
				netId = NetId,
				authority = true
			});
			clientAuthorityCallback?.Invoke(conn, this, true);
			return true;
		}

		internal void MarkForReset() => m_Reset = true;

		internal void Reset()
		{
			if (m_Reset)
			{
				m_Reset = false;
				m_IsServer = false;
				IsClient = false;
				HasAuthority = false;
				NetId = (NetworkInstanceId)typeof(NetworkInstanceId).GetField("Zero", System.Reflection.BindingFlags.Static).GetValue(null);
				IsLocalPlayer = false;
				ConnectionToServer = null;
				ConnectionToClient = null;
				PlayerControllerId = -1;
				m_NetworkBehaviours = null;
				ClearObservers();
				ClientAuthorityOwner = null;
			}
		}

		internal static void UNetStaticUpdate()
		{
			QSBNetworkServer.Update();
			QSBNetworkClient.UpdateClients();
			QSBNetworkManagerUNET.UpdateScene();
		}

		[SerializeField]
		private NetworkSceneId m_SceneId;

		[SerializeField]
		private NetworkHash128 m_AssetId;

		[SerializeField]
		private bool m_ServerOnly;

		[SerializeField]
		private bool m_LocalPlayerAuthority;

		private bool m_IsServer;
		private QSBNetworkBehaviour[] m_NetworkBehaviours;

		private HashSet<int> m_ObserverConnections;

		private List<QSBNetworkConnection> m_Observers;
		private bool m_Reset = false;

		private static uint s_NextNetworkId = 1U;

		private static readonly QSBNetworkWriter s_UpdateWriter = new QSBNetworkWriter();

		public static ClientAuthorityCallback clientAuthorityCallback;

		public delegate void ClientAuthorityCallback(QSBNetworkConnection conn, QSBNetworkIdentity uv, bool authorityState);
	}
}