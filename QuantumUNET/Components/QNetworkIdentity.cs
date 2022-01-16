﻿using QuantumUNET.Logging;
using QuantumUNET.Messages;
using QuantumUNET.Transport;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace QuantumUNET.Components
{
	public sealed class QNetworkIdentity : MonoBehaviour
	{
		public bool IsClient { get; private set; }
		public bool IsServer => m_IsServer && QNetworkServer.active && m_IsServer;
		public bool HasAuthority { get; private set; }
		public QNetworkInstanceId NetId { get; private set; }
		public QNetworkSceneId SceneId => m_SceneId;
		public QNetworkConnection ClientAuthorityOwner { get; private set; }
		public int AssetId => m_AssetId;
		public bool IsLocalPlayer { get; private set; }
		public short PlayerControllerId { get; private set; } = -1;
		public QNetworkConnection ConnectionToServer { get; private set; }
		public QNetworkConnection ConnectionToClient { get; private set; }

		public bool ServerOnly
		{
			get => m_ServerOnly;
			set => m_ServerOnly = value;
		}

		public bool LocalPlayerAuthority
		{
			get => m_LocalPlayerAuthority;
			set => m_LocalPlayerAuthority = value;
		}

		public QNetworkBehaviour[] GetNetworkBehaviours()
			=> m_NetworkBehaviours;

		internal void SetDynamicAssetId(int newAssetId)
		{
			if (m_AssetId == 0 || m_AssetId.Equals(newAssetId))
			{
				m_AssetId = newAssetId;
			}
			else
			{
				QLog.Warning($"SetDynamicAssetId object already has an assetId <{m_AssetId}>");
			}
		}

		internal void SetClientOwner(QNetworkConnection conn)
		{
			if (ClientAuthorityOwner != null)
			{
				QLog.Error("SetClientOwner m_ClientAuthorityOwner already set!");
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

		public ReadOnlyCollection<QNetworkConnection> Observers
			=> m_Observers == null ? null : new ReadOnlyCollection<QNetworkConnection>(m_Observers);

		internal static QNetworkInstanceId GetNextNetworkId()
		{
			var value = s_NextNetworkId;
			s_NextNetworkId += 1U;
			return new QNetworkInstanceId(value);
		}

		private void CacheBehaviours()
		{
			if (m_NetworkBehaviours == null)
			{
				m_NetworkBehaviours = GetComponents<QNetworkBehaviour>();
			}
		}

		internal static void AddNetworkId(uint id)
		{
			if (id >= s_NextNetworkId)
			{
				s_NextNetworkId = id + 1U;
			}
		}

		internal void SetNetworkInstanceId(QNetworkInstanceId newNetId)
		{
			NetId = newNetId;
			if (newNetId.Value == 0U)
			{
				m_IsServer = false;
			}
		}

		public void ForceSceneId(int newSceneId) => m_SceneId = new QNetworkSceneId((uint)newSceneId);

		internal void UpdateClientServer(bool isClientFlag, bool isServerFlag)
		{
			IsClient = IsClient || isClientFlag;
			m_IsServer = m_IsServer || isServerFlag;
		}

		internal void SetNotLocalPlayer()
		{
			IsLocalPlayer = false;
			if (!QNetworkServer.active || !QNetworkServer.localClientActive)
			{
				HasAuthority = false;
			}
		}

		internal void RemoveObserverInternal(QNetworkConnection conn)
		{
			if (m_Observers != null)
			{
				m_Observers.Remove(conn);
				m_ObserverConnections.Remove(conn.connectionId);
			}
		}

		public void OnDestroy()
		{
			if (m_IsServer && QNetworkServer.active)
			{
				QNetworkServer.Destroy(gameObject);
			}
		}

		internal void OnStartServer(bool allowNonZeroNetId)
		{
			if (!m_IsServer)
			{
				m_IsServer = true;
				HasAuthority = !m_LocalPlayerAuthority;

				m_Observers = new List<QNetworkConnection>();
				m_ObserverConnections = new HashSet<int>();
				CacheBehaviours();
				if (NetId.IsEmpty())
				{
					NetId = GetNextNetworkId();
				}
				else if (!allowNonZeroNetId)
				{
					QLog.Warning($"Object has non-zero netId {NetId} for {gameObject}");
					return;
				}

				QNetworkServer.instance.SetLocalObjectOnServer(NetId, gameObject);
				foreach (var networkBehaviour in m_NetworkBehaviours)
				{
					try
					{
						networkBehaviour.OnStartServer();
					}
					catch (Exception ex)
					{
						QLog.FatalError($"Exception in OnStartServer:{ex.Message} {ex.StackTrace}");
					}
				}

				if (QNetworkClient.active && QNetworkServer.localClientActive)
				{
					QClientScene.SetLocalObject(NetId, gameObject);
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
			QLog.Debug($"OnStartClient {gameObject} GUID:{NetId} localPlayerAuthority:{LocalPlayerAuthority}");
			foreach (var networkBehaviour in m_NetworkBehaviours)
			{
				try
				{
					networkBehaviour.PreStartClient();
					networkBehaviour.OnStartClient();
				}
				catch (Exception ex)
				{
					QLog.FatalError($"Exception in OnStartClient:{ex.Message} {ex.StackTrace}");
				}
			}
		}

		internal void OnStartAuthority()
		{
			foreach (var networkBehaviour in m_NetworkBehaviours)
			{
				try
				{
					networkBehaviour.OnStartAuthority();
				}
				catch (Exception ex)
				{
					Debug.LogError($"Exception in OnStartAuthority:{ex.Message} {ex.StackTrace}");
				}
			}
		}

		internal void OnStopAuthority()
		{
			foreach (var networkBehaviour in m_NetworkBehaviours)
			{
				try
				{
					networkBehaviour.OnStopAuthority();
				}
				catch (Exception ex)
				{
					QLog.FatalError($"Exception in OnStopAuthority:{ex.Message} {ex.StackTrace}");
				}
			}
		}

		internal void OnSetLocalVisibility(bool vis)
		{
			foreach (var networkBehaviour in m_NetworkBehaviours)
			{
				try
				{
					networkBehaviour.OnSetLocalVisibility(vis);
				}
				catch (Exception ex)
				{
					QLog.FatalError($"Exception in OnSetLocalVisibility:{ex.Message} {ex.StackTrace}");
				}
			}
		}

		internal bool OnCheckObserver(QNetworkConnection conn)
		{
			foreach (var networkBehaviour in m_NetworkBehaviours)
			{
				try
				{
					if (!networkBehaviour.OnCheckObserver(conn))
					{
						return false;
					}
				}
				catch (Exception ex)
				{
					QLog.FatalError($"Exception in OnCheckObserver:{ex.Message} {ex.StackTrace}");
				}
			}

			return true;
		}

		internal void UNetSerializeAllVars(QNetworkWriter writer)
		{
			foreach (var networkBehaviour in m_NetworkBehaviours)
			{
				networkBehaviour.OnSerialize(writer, true);
			}
		}

		internal void HandleClientAuthority(bool authority)
		{
			if (!LocalPlayerAuthority)
			{
				QLog.Error($"HandleClientAuthority {gameObject} does not have localPlayerAuthority");
			}
			else
			{
				ForceAuthority(authority);
			}
		}

		private bool GetInvokeComponent(int cmdHash, Type invokeClass, out QNetworkBehaviour invokeComponent)
		{
			QNetworkBehaviour networkBehaviour = null;
			foreach (var networkBehaviour2 in m_NetworkBehaviours)
			{
				if (networkBehaviour2.GetType() == invokeClass || networkBehaviour2.GetType().IsSubclassOf(invokeClass))
				{
					networkBehaviour = networkBehaviour2;
					break;
				}
			}

			bool result;
			if (networkBehaviour == null)
			{
				var cmdHashHandlerName = QNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				QLog.FatalError(
					$"Found no behaviour for incoming [{cmdHashHandlerName}] on {gameObject},  the server and client should have the same NetworkBehaviour instances [netId={NetId}].");
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

		internal void HandleSyncEvent(int cmdHash, QNetworkReader reader)
		{
			if (gameObject == null)
			{
				var cmdHashHandlerName = QNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				QLog.Warning($"SyncEvent [{cmdHashHandlerName}] received for deleted object [netId={NetId}]");
			}
			else if (!QNetworkBehaviour.GetInvokerForHashSyncEvent(cmdHash, out var invokeClass, out var cmdDelegate))
			{
				var cmdHashHandlerName2 = QNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				QLog.FatalError(
					$"Found no receiver for incoming [{cmdHashHandlerName2}] on {gameObject},  the server and client should have the same NetworkBehaviour instances [netId={NetId}].");
			}
			else if (!GetInvokeComponent(cmdHash, invokeClass, out var obj))
			{
				var cmdHashHandlerName3 = QNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				QLog.Warning($"SyncEvent [{cmdHashHandlerName3}] handler not found [netId={NetId}]");
			}
			else
			{
				cmdDelegate(obj, reader);
			}
		}

		internal void HandleSyncList(int cmdHash, QNetworkReader reader)
		{
			if (gameObject == null)
			{
				var cmdHashHandlerName = QNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				QLog.Warning($"SyncList [{cmdHashHandlerName}] received for deleted object [netId={NetId}]");
			}
			else if (!QNetworkBehaviour.GetInvokerForHashSyncList(cmdHash, out var invokeClass, out var cmdDelegate))
			{
				var cmdHashHandlerName2 = QNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				QLog.FatalError(
					$"Found no receiver for incoming [{cmdHashHandlerName2}] on {gameObject},  the server and client should have the same NetworkBehaviour instances [netId={NetId}].");
			}
			else if (!GetInvokeComponent(cmdHash, invokeClass, out var obj))
			{
				var cmdHashHandlerName3 = QNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				QLog.Warning($"SyncList [{cmdHashHandlerName3}] handler not found [netId={NetId}]");
			}
			else
			{
				cmdDelegate(obj, reader);
			}
		}

		internal void HandleCommand(int cmdHash, QNetworkReader reader)
		{
			if (gameObject == null)
			{
				var cmdHashHandlerName = QNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				QLog.Warning($"Command [{cmdHashHandlerName}] received for deleted object [netId={NetId}]");
			}
			else if (!QNetworkBehaviour.GetInvokerForHashCommand(cmdHash, out var invokeClass, out var cmdDelegate))
			{
				var cmdHashHandlerName2 = QNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				QLog.FatalError(
					$"Found no receiver for incoming [{cmdHashHandlerName2}] on {gameObject},  the server and client should have the same NetworkBehaviour instances [netId={NetId}].");
			}
			else if (!GetInvokeComponent(cmdHash, invokeClass, out var obj))
			{
				var cmdHashHandlerName3 = QNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				QLog.Warning($"Command [{cmdHashHandlerName3}] handler not found [netId={NetId}]");
			}
			else
			{
				cmdDelegate(obj, reader);
			}
		}

		internal void HandleRPC(int cmdHash, QNetworkReader reader)
		{
			if (gameObject == null)
			{
				var cmdHashHandlerName = QNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				QLog.Warning($"ClientRpc [{cmdHashHandlerName}] received for deleted object [netId={NetId}]");
			}
			else if (!QNetworkBehaviour.GetInvokerForHashClientRpc(cmdHash, out var invokeClass, out var cmdDelegate))
			{
				var cmdHashHandlerName2 = QNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				QLog.FatalError(
					$"Found no receiver for incoming [{cmdHashHandlerName2}] on {gameObject},  the server and client should have the same NetworkBehaviour instances [netId={NetId}].");
			}
			else if (!GetInvokeComponent(cmdHash, invokeClass, out var obj))
			{
				var cmdHashHandlerName3 = QNetworkBehaviour.GetCmdHashHandlerName(cmdHash);
				QLog.Warning($"ClientRpc [{cmdHashHandlerName3}] handler not found [netId={NetId}]");
			}
			else
			{
				cmdDelegate(obj, reader);
			}
		}

		internal void UNetUpdate()
		{
			var num = 0U;
			foreach (var networkBehaviour in m_NetworkBehaviours)
			{
				var dirtyChannel = networkBehaviour.GetDirtyChannel();
				if (dirtyChannel != -1)
				{
					num |= 1U << dirtyChannel;
				}
			}

			if (num != 0U)
			{
				var j = 0;
				while (j < QNetworkServer.numChannels)
				{
					if ((num & (1U << j)) != 0U)
					{
						s_UpdateWriter.StartMessage(QMsgType.UpdateVars);
						s_UpdateWriter.Write(NetId);
						var flag = false;
						foreach (var networkBehaviour in m_NetworkBehaviours)
						{
							var position = s_UpdateWriter.Position;
							if (networkBehaviour.GetDirtyChannel() != j)
							{
								networkBehaviour.OnSerialize(s_UpdateWriter, false);
							}
							else
							{
								if (networkBehaviour.OnSerialize(s_UpdateWriter, false))
								{
									networkBehaviour.ClearAllDirtyBits();
									flag = true;
								}

								var maxPacketSize = QNetworkServer.maxPacketSize;
								if (s_UpdateWriter.Position - position > maxPacketSize)
								{
									QLog.Warning(
										$"Large state update of {s_UpdateWriter.Position - position} bytes for netId:{NetId} from script:{networkBehaviour}");
								}
							}
						}

						if (flag)
						{
							s_UpdateWriter.FinishMessage();
							QNetworkServer.SendWriterToReady(gameObject, s_UpdateWriter, j);
						}
					}

					j++;
				}
			}
		}

		internal void OnUpdateVars(QNetworkReader reader, bool initialState)
		{
			if (initialState && m_NetworkBehaviours == null)
			{
				m_NetworkBehaviours = GetComponents<QNetworkBehaviour>();
			}

			foreach (var networkBehaviour in m_NetworkBehaviours)
			{
				networkBehaviour.OnDeserialize(reader, initialState);
			}
		}

		internal void SetLocalPlayer(short localPlayerControllerId)
		{
			IsLocalPlayer = true;
			PlayerControllerId = localPlayerControllerId;
			var hasAuthority = HasAuthority;
			if (LocalPlayerAuthority)
			{
				HasAuthority = true;
			}

			foreach (var networkBehaviour in m_NetworkBehaviours)
			{
				networkBehaviour.OnStartLocalPlayer();
				if (LocalPlayerAuthority && !hasAuthority)
				{
					networkBehaviour.OnStartAuthority();
				}
			}
		}

		internal void SetConnectionToServer(QNetworkConnection conn) => ConnectionToServer = conn;

		internal void SetConnectionToClient(QNetworkConnection conn, short newPlayerControllerId)
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

		internal void AddObserver(QNetworkConnection conn)
		{
			if (m_Observers == null)
			{
				QLog.Error($"AddObserver for {gameObject} observer list is null");
			}
			else if (m_ObserverConnections.Contains(conn.connectionId))
			{
				QLog.Warning($"Duplicate observer {conn.address} added for {gameObject}");
			}
			else
			{
				QLog.Debug($"Added observer {conn.address} added for {gameObject}");
				m_Observers.Add(conn);
				m_ObserverConnections.Add(conn.connectionId);
				conn.AddToVisList(this);
			}
		}

		internal void RemoveObserver(QNetworkConnection conn)
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
				var hashSet = new HashSet<QNetworkConnection>();
				var hashSet2 = new HashSet<QNetworkConnection>(m_Observers);
				foreach (var networkBehaviour in m_NetworkBehaviours)
				{
					flag2 |= networkBehaviour.OnRebuildObservers(hashSet, initialize);
				}

				if (!flag2)
				{
					if (initialize)
					{
						foreach (var networkConnection in QNetworkServer.connections)
						{
							if (networkConnection != null)
							{
								if (networkConnection.isReady)
								{
									AddObserver(networkConnection);
								}
							}
						}

						var networkConnection2 = QNetworkServer.localConnection;
						if (networkConnection2 != null)
						{
							if (networkConnection2.isReady)
							{
								AddObserver(networkConnection2);
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
								QLog.Warning($"Observer is not ready for {gameObject} {networkConnection3}");
							}
							else if (initialize || !hashSet2.Contains(networkConnection3))
							{
								networkConnection3.AddToVisList(this);
								QLog.Log($"New Observer for {gameObject} {networkConnection3}");
								flag = true;
							}
						}
					}

					foreach (var networkConnection4 in hashSet2)
					{
						if (!hashSet.Contains(networkConnection4))
						{
							networkConnection4.RemoveFromVisList(this, true);
							QLog.Log($"Removed Observer for {gameObject} {networkConnection4}");
							flag = true;
						}
					}

					if (initialize)
					{
						var connection = QNetworkServer.localConnection;
						if (!hashSet.Contains(connection))
						{
							OnSetLocalVisibility(false);
						}
					}

					if (flag)
					{
						m_Observers = new List<QNetworkConnection>(hashSet);
						m_ObserverConnections.Clear();
						foreach (var observer in m_Observers)
						{
							m_ObserverConnections.Add(observer.connectionId);
						}
					}
				}
			}
		}

		public bool RemoveClientAuthority(QNetworkConnection conn)
		{
			if (!IsServer)
			{
				QLog.Warning($"Cannot remove authority on client-side. (NetId:{NetId}, Gameobject:{gameObject.name})");
				return false;
			}
			else if (ConnectionToClient != null)
			{
				QLog.Warning("RemoveClientAuthority cannot remove authority for a player object");
				return false;
			}
			else if (ClientAuthorityOwner == null)
			{
				QLog.Warning($"RemoveClientAuthority for {gameObject} has no clientAuthority owner.");
				return false;
			}
			else if (ClientAuthorityOwner != conn)
			{
				QLog.Warning($"RemoveClientAuthority for {gameObject} has different owner.");
				return false;
			}

			ClientAuthorityOwner.RemoveOwnedObject(this);
			ClientAuthorityOwner = null;
			ForceAuthority(true);
			conn.Send(15, new QClientAuthorityMessage
			{
				netId = NetId,
				authority = false
			});
			clientAuthorityCallback?.Invoke(conn, this, false);
			return true;
		}

		public bool AssignClientAuthority(QNetworkConnection conn)
		{
			if (!IsServer)
			{
				QLog.Warning($"Cannot assign authority on client-side. (NetId:{NetId}, Gameobject:{gameObject.name})");
				return false;
			}
			else if (!LocalPlayerAuthority)
			{
				QLog.Warning($"Cannot assign authority on object without LocalPlayerAuthority. (NetId:{NetId}, Gameobject:{gameObject.name})");
				return false;
			}
			else if (ClientAuthorityOwner != null && conn != ClientAuthorityOwner)
			{
				QLog.Warning($"AssignClientAuthority for {gameObject} already has an owner. Use RemoveClientAuthority() first.");
				return false;
			}
			else if (conn == null)
			{
				QLog.Warning($"AssignClientAuthority for {gameObject} owner cannot be null. Use RemoveClientAuthority() instead.");
				return false;
			}

			ClientAuthorityOwner = conn;
			ClientAuthorityOwner.AddOwnedObject(this);

			ForceAuthority(false);
			conn.Send(15, new QClientAuthorityMessage
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
				NetId = QNetworkInstanceId.Zero;
				IsLocalPlayer = false;
				ConnectionToServer = null;
				ConnectionToClient = null;
				PlayerControllerId = -1;
				m_NetworkBehaviours = null;
				ClearObservers();
				ClientAuthorityOwner = null;
			}
		}

		public static void UNetStaticUpdate()
		{
			QNetworkServer.Update();
			QNetworkClient.UpdateClients();
		}

		[SerializeField]
		private QNetworkSceneId m_SceneId;

		[SerializeField]
		public int m_AssetId;

		[SerializeField]
		private bool m_ServerOnly;

		[SerializeField]
		private bool m_LocalPlayerAuthority;

		private bool m_IsServer;

		private QNetworkBehaviour[] m_NetworkBehaviours;

		private HashSet<int> m_ObserverConnections;

		private List<QNetworkConnection> m_Observers;

		private bool m_Reset;

		private static uint s_NextNetworkId = 1U;

		private static readonly QNetworkWriter s_UpdateWriter = new();

		public static ClientAuthorityCallback clientAuthorityCallback;

		public delegate void ClientAuthorityCallback(QNetworkConnection conn, QNetworkIdentity uv, bool authorityState);
	}
}
