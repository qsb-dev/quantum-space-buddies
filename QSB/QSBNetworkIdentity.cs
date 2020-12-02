using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;
using OWML.ModHelper.Events;
using QSB.Utility;

namespace QSB
{
	public sealed class QSBNetworkIdentity : MonoBehaviour
	{
		public bool IsClient { get; private set; }
		public bool IsServer => m_IsServer && NetworkServer.active && m_IsServer;
		public bool HasAuthority { get; private set; }
		public NetworkInstanceId NetId { get; private set; }
		public NetworkSceneId SceneId => m_SceneId;
		public NetworkConnection ClientAuthorityOwner { get; private set; }
		public NetworkHash128 AssetId => m_AssetId;
		public bool IsLocalPlayer { get; private set; }
		public short PlayerControllerId { get; private set; } = -1;
		public NetworkConnection ConnectionToServer { get; private set; }
		public NetworkConnection ConnectionToClient { get; private set; }

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

		internal void SetClientOwner(NetworkConnection conn)
		{
			if (ClientAuthorityOwner != null)
			{
				Debug.LogError("SetClientOwner m_ClientAuthorityOwner already set!");
			}
			ClientAuthorityOwner = conn;
			ClientAuthorityOwner.GetType().GetMethod("AddOwnedObject", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Invoke(ClientAuthorityOwner, new object[] { this });
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

		

		public ReadOnlyCollection<NetworkConnection> Observers
		{
			get
			{
				ReadOnlyCollection<NetworkConnection> result;
				if (m_Observers == null)
				{
					result = null;
				}
				else
				{
					result = new ReadOnlyCollection<NetworkConnection>(m_Observers);
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
			if (!NetworkServer.active || !NetworkServer.localClientActive)
			{
				HasAuthority = false;
			}
		}

		internal void RemoveObserverInternal(NetworkConnection conn)
		{
			if (m_Observers != null)
			{
				m_Observers.Remove(conn);
				m_ObserverConnections.Remove(conn.connectionId);
			}
		}

		private void OnDestroy()
		{
			if (m_IsServer && NetworkServer.active)
			{
				NetworkServer.Destroy(base.gameObject);
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
				m_Observers = new List<NetworkConnection>();
				m_ObserverConnections = new HashSet<int>();
				CacheBehaviours();
				if (NetId.IsEmpty())
				{
					NetId = GetNextNetworkId();
				}
				else if (!allowNonZeroNetId)
				{
					Debug.LogError(string.Concat(new object[]
					{
						"Object has non-zero netId ",
						NetId,
						" for ",
						base.gameObject
					}));
					return;
				}
				Debug.Log(string.Concat(new object[]
				{
					"OnStartServer ",
					base.gameObject,
					" GUID:",
					NetId
				}));
				var server = (NetworkServer)typeof(NetworkServer).GetField("instance", System.Reflection.BindingFlags.Static).GetValue(null);
				server.GetType().GetMethod("SetLocalObjectOnServer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Invoke(server, new object[] { NetId, gameObject });
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
				if (NetworkClient.active && NetworkServer.localClientActive)
				{
					ClientScene.SetLocalObject(NetId, base.gameObject);
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

		internal bool OnCheckObserver(NetworkConnection conn)
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

		internal void UNetSerializeAllVars(NetworkWriter writer)
		{
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

		internal void HandleSyncEvent(int cmdHash, NetworkReader reader)
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

		internal void HandleSyncList(int cmdHash, NetworkReader reader)
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

		internal void HandleCommand(int cmdHash, NetworkReader reader)
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

		internal void HandleRPC(int cmdHash, NetworkReader reader)
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
				while (j < NetworkServer.numChannels)
				{
					if ((num & (1U << j)) != 0U)
					{
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
								var maxPacketSize = (ushort)typeof(NetworkServer).GetField("maxPacketSize", System.Reflection.BindingFlags.Static).GetValue(null);
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
							s_UpdateWriter.FinishMessage();
							NetworkServer.SendWriterToReady(base.gameObject, s_UpdateWriter, j);
						}
					}
					IL_197:
					j++;
					continue;
					goto IL_197;
				}
			}
		}

		internal void OnUpdateVars(NetworkReader reader, bool initialState)
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

		internal void SetConnectionToServer(NetworkConnection conn) => ConnectionToServer = conn;

		internal void SetConnectionToClient(NetworkConnection conn, short newPlayerControllerId)
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
					networkConnection.GetType().GetMethod("RemoveFromVisList", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Invoke(networkConnection, new object[] { this, true });
				}
				m_Observers.Clear();
				m_ObserverConnections.Clear();
			}
		}

		internal void AddObserver(NetworkConnection conn)
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
				conn.GetType().GetMethod("AddToVisList", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Invoke(conn, new object[] { this });
			}
		}

		internal void RemoveObserver(NetworkConnection conn)
		{
			if (m_Observers != null)
			{
				m_Observers.Remove(conn);
				m_ObserverConnections.Remove(conn.connectionId);
				conn.GetType().GetMethod("RemoveFromVisList", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Invoke(conn, new object[] { this, true });
			}
		}

		public void RebuildObservers(bool initialize)
		{
			if (m_Observers != null)
			{
				var flag = false;
				var flag2 = false;
				var hashSet = new HashSet<NetworkConnection>();
				var hashSet2 = new HashSet<NetworkConnection>(m_Observers);
				for (var i = 0; i < m_NetworkBehaviours.Length; i++)
				{
					var networkBehaviour = m_NetworkBehaviours[i];
					flag2 |= networkBehaviour.OnRebuildObservers(hashSet, initialize);
				}
				if (!flag2)
				{
					if (initialize)
					{
						for (var j = 0; j < NetworkServer.connections.Count; j++)
						{
							var networkConnection = NetworkServer.connections[j];
							if (networkConnection != null)
							{
								if (networkConnection.isReady)
								{
									AddObserver(networkConnection);
								}
							}
						}
						for (var k = 0; k < NetworkServer.localConnections.Count; k++)
						{
							var networkConnection2 = NetworkServer.localConnections[k];
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
								networkConnection3.GetType().GetMethod("AddToVisList", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Invoke(networkConnection3, new object[] { this });
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
							networkConnection4.GetType().GetMethod("RemoveFromVisList", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Invoke(networkConnection4, new object[] { this, true });
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
						for (var l = 0; l < NetworkServer.localConnections.Count; l++)
						{
							if (!hashSet.Contains(NetworkServer.localConnections[l]))
							{
								OnSetLocalVisibility(false);
							}
						}
					}
					if (flag)
					{
						m_Observers = new List<NetworkConnection>(hashSet);
						m_ObserverConnections.Clear();
						for (var m = 0; m < m_Observers.Count; m++)
						{
							m_ObserverConnections.Add(m_Observers[m].connectionId);
						}
					}
				}
			}
		}

		public bool RemoveClientAuthority(NetworkConnection conn)
		{
			bool result;
			if (!IsServer)
			{
				Debug.LogError("RemoveClientAuthority can only be call on the server for spawned objects.");
				result = false;
			}
			else if (ConnectionToClient != null)
			{
				Debug.LogError("RemoveClientAuthority cannot remove authority for a player object");
				result = false;
			}
			else if (ClientAuthorityOwner == null)
			{
				Debug.LogError("RemoveClientAuthority for " + base.gameObject + " has no clientAuthority owner.");
				result = false;
			}
			else if (ClientAuthorityOwner != conn)
			{
				Debug.LogError("RemoveClientAuthority for " + base.gameObject + " has different owner.");
				result = false;
			}
			else
			{
				ClientAuthorityOwner.GetType().GetMethod("RemoveOwnedObject", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Invoke(ClientAuthorityOwner, new object[] { this });
				ClientAuthorityOwner = null;
				ForceAuthority(true);
				/*
				conn.Send(15, new ClientAuthorityMessage
				{
					netId = NetId,
					authority = false
				});
				*/
				clientAuthorityCallback?.Invoke(conn, this, false);
				result = true;
			}
			return result;
		}

		public bool AssignClientAuthority(NetworkConnection conn)
		{
			bool result;
			if (!IsServer)
			{
				Debug.LogError("AssignClientAuthority can only be call on the server for spawned objects.");
				result = false;
			}
			else if (!LocalPlayerAuthority)
			{
				Debug.LogError("AssignClientAuthority can only be used for NetworkIdentity component with LocalPlayerAuthority set.");
				result = false;
			}
			else if (ClientAuthorityOwner != null && conn != ClientAuthorityOwner)
			{
				Debug.LogError("AssignClientAuthority for " + base.gameObject + " already has an owner. Use RemoveClientAuthority() first.");
				result = false;
			}
			else if (conn == null)
			{
				Debug.LogError("AssignClientAuthority for " + base.gameObject + " owner cannot be null. Use RemoveClientAuthority() instead.");
				result = false;
			}
			else
			{
				ClientAuthorityOwner = conn;
				ClientAuthorityOwner.GetType().GetMethod("AddOwnedObject", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Invoke(ClientAuthorityOwner, new object[] { this });

				ForceAuthority(false);
				/*
				conn.Send(15, new ClientAuthorityMessage
				{
					netId = NetId,
					authority = true
				});
				*/
				clientAuthorityCallback?.Invoke(conn, this, true);
				result = true;
			}
			return result;
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
			typeof(NetworkServer).GetMethod("Update", System.Reflection.BindingFlags.Static).Invoke(null, null);
			typeof(NetworkClient).GetMethod("UpdateClients", System.Reflection.BindingFlags.Static).Invoke(null, null);
			typeof(NetworkManager).GetMethod("UpdateScene", System.Reflection.BindingFlags.Static).Invoke(null, null);
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

		private List<NetworkConnection> m_Observers;
		private bool m_Reset = false;

		private static uint s_NextNetworkId = 1U;

		private static readonly NetworkWriter s_UpdateWriter = new NetworkWriter();

		public static ClientAuthorityCallback clientAuthorityCallback;

		public delegate void ClientAuthorityCallback(NetworkConnection conn, QSBNetworkIdentity uv, bool authorityState);
	}
}