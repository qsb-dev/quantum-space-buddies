using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
	class QSBNetworkScene
	{
		private Dictionary<NetworkInstanceId, QSBNetworkIdentity> m_LocalObjects = new Dictionary<NetworkInstanceId, QSBNetworkIdentity>();

		internal static Dictionary<NetworkHash128, GameObject> guidToPrefab { get; } = new Dictionary<NetworkHash128, GameObject>();

		internal static Dictionary<NetworkHash128, SpawnDelegate> spawnHandlers { get; } = new Dictionary<NetworkHash128, SpawnDelegate>();

		internal static Dictionary<NetworkHash128, UnSpawnDelegate> unspawnHandlers { get; } = new Dictionary<NetworkHash128, UnSpawnDelegate>();

		internal Dictionary<NetworkInstanceId, QSBNetworkIdentity> localObjects
		{
			get
			{
				return this.m_LocalObjects;
			}
		}

		internal void Shutdown()
		{
			this.ClearLocalObjects();
			ClearSpawners();
		}

		internal void SetLocalObject(NetworkInstanceId netId, GameObject obj, bool isClient, bool isServer)
		{
			if (obj == null)
			{
				this.localObjects[netId] = null;
			}
			else
			{
				QSBNetworkIdentity networkIdentity = null;
				if (this.localObjects.ContainsKey(netId))
				{
					networkIdentity = this.localObjects[netId];
				}
				if (networkIdentity == null)
				{
					DebugLog.DebugWrite($"Adding {netId} to local objects.");
					networkIdentity = obj.GetComponent<QSBNetworkIdentity>();
					this.localObjects[netId] = networkIdentity;
				}
				networkIdentity.UpdateClientServer(isClient, isServer);
			}
		}

		internal GameObject FindLocalObject(NetworkInstanceId netId)
		{
			if (this.localObjects.ContainsKey(netId))
			{
				QSBNetworkIdentity networkIdentity = this.localObjects[netId];
				if (networkIdentity != null)
				{
					return networkIdentity.gameObject;
				}
			}
			return null;
		}

		internal bool GetNetworkIdentity(NetworkInstanceId netId, out QSBNetworkIdentity uv)
		{
			bool result;
			if (localObjects.ContainsKey(netId) && localObjects[netId] != null)
			{
				uv = localObjects[netId];
				result = true;
			}
			else
			{
				uv = null;
				result = false;
			}
			return result;
		}

		internal bool RemoveLocalObject(NetworkInstanceId netId)
		{
			return localObjects.Remove(netId);
		}

		internal bool RemoveLocalObjectAndDestroy(NetworkInstanceId netId)
		{
			bool result;
			if (this.localObjects.ContainsKey(netId))
			{
				QSBNetworkIdentity networkIdentity = this.localObjects[netId];
				UnityEngine.Object.Destroy(networkIdentity.gameObject);
				result = this.localObjects.Remove(netId);
			}
			else
			{
				result = false;
			}
			return result;
		}

		internal void ClearLocalObjects()
		{
			this.localObjects.Clear();
		}

		internal static void RegisterPrefab(GameObject prefab, NetworkHash128 newAssetId)
		{
			QSBNetworkIdentity component = prefab.GetComponent<QSBNetworkIdentity>();
			if (component)
			{
				component.SetDynamicAssetId(newAssetId);
				guidToPrefab[component.AssetId] = prefab;
			}
			else if (LogFilter.logError)
			{
				Debug.LogError("Could not register '" + prefab.name + "' since it contains no NetworkIdentity component");
			}
		}

		internal static void RegisterPrefab(GameObject prefab)
		{
			QSBNetworkIdentity component = prefab.GetComponent<QSBNetworkIdentity>();
			if (component)
			{
				guidToPrefab[component.AssetId] = prefab;
				NetworkIdentity[] componentsInChildren = prefab.GetComponentsInChildren<NetworkIdentity>();
				if (componentsInChildren.Length > 1)
				{
					if (LogFilter.logWarn)
					{
						Debug.LogWarning("The prefab '" + prefab.name + "' has multiple NetworkIdentity components. There can only be one NetworkIdentity on a prefab, and it must be on the root object.");
					}
				}
			}
			else if (LogFilter.logError)
			{
				Debug.LogError("Could not register '" + prefab.name + "' since it contains no NetworkIdentity component");
			}
		}

		internal static bool GetPrefab(NetworkHash128 assetId, out GameObject prefab)
		{
			bool result;
			if (!assetId.IsValid())
			{
				prefab = null;
				result = false;
			}
			else if (guidToPrefab.ContainsKey(assetId) && guidToPrefab[assetId] != null)
			{
				prefab = guidToPrefab[assetId];
				result = true;
			}
			else
			{
				prefab = null;
				result = false;
			}
			return result;
		}

		internal static void ClearSpawners()
		{
			guidToPrefab.Clear();
			spawnHandlers.Clear();
			unspawnHandlers.Clear();
		}

		public static void UnregisterSpawnHandler(NetworkHash128 assetId)
		{
			spawnHandlers.Remove(assetId);
			unspawnHandlers.Remove(assetId);
		}

		internal static void RegisterSpawnHandler(NetworkHash128 assetId, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
		{
			if (spawnHandler == null || unspawnHandler == null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("RegisterSpawnHandler custom spawn function null for " + assetId);
				}
			}
			else
			{
				spawnHandlers[assetId] = spawnHandler;
				unspawnHandlers[assetId] = unspawnHandler;
			}
		}

		internal static void UnregisterPrefab(GameObject prefab)
		{
			QSBNetworkIdentity component = prefab.GetComponent<QSBNetworkIdentity>();
			if (component == null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("Could not unregister '" + prefab.name + "' since it contains no NetworkIdentity component");
				}
			}
			else
			{
				spawnHandlers.Remove(component.AssetId);
				unspawnHandlers.Remove(component.AssetId);
			}
		}

		internal static void RegisterPrefab(GameObject prefab, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
		{
			QSBNetworkIdentity component = prefab.GetComponent<QSBNetworkIdentity>();
			if (component == null)
			{
				Debug.LogError("Could not register '" + prefab.name + "' since it contains no NetworkIdentity component");
			}
			else if (spawnHandler == null || unspawnHandler == null)
			{
				Debug.LogError("RegisterPrefab custom spawn function null for " + component.AssetId);
			}
			else if (!component.AssetId.IsValid())
			{
				Debug.LogError("RegisterPrefab game object " + prefab.name + " has no prefab. Use RegisterSpawnHandler() instead?");
			}
			else
			{
				spawnHandlers[component.AssetId] = spawnHandler;
				unspawnHandlers[component.AssetId] = unspawnHandler;
			}
		}

		internal static bool GetSpawnHandler(NetworkHash128 assetId, out SpawnDelegate handler)
		{
			bool result;
			if (spawnHandlers.ContainsKey(assetId))
			{
				handler = spawnHandlers[assetId];
				result = true;
			}
			else
			{
				handler = null;
				result = false;
			}
			return result;
		}

		internal static bool InvokeUnSpawnHandler(NetworkHash128 assetId, GameObject obj)
		{
			bool result;
			if (unspawnHandlers.ContainsKey(assetId) && unspawnHandlers[assetId] != null)
			{
				UnSpawnDelegate unSpawnDelegate = unspawnHandlers[assetId];
				unSpawnDelegate(obj);
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		internal void DestroyAllClientObjects()
		{
			foreach (NetworkInstanceId key in this.localObjects.Keys)
			{
				QSBNetworkIdentity networkIdentity = this.localObjects[key];
				if (networkIdentity != null && networkIdentity.gameObject != null)
				{
					if (!InvokeUnSpawnHandler(networkIdentity.AssetId, networkIdentity.gameObject))
					{
						if (networkIdentity.SceneId.IsEmpty())
						{
							UnityEngine.Object.Destroy(networkIdentity.gameObject);
						}
						else
						{
							networkIdentity.MarkForReset();
							networkIdentity.gameObject.SetActive(false);
						}
					}
				}
			}
			this.ClearLocalObjects();
		}

		internal void DumpAllClientObjects()
		{
			foreach (NetworkInstanceId networkInstanceId in this.localObjects.Keys)
			{
				QSBNetworkIdentity networkIdentity = this.localObjects[networkInstanceId];
				if (networkIdentity != null)
				{
					Debug.Log(string.Concat(new object[]
					{
						"ID:",
						networkInstanceId,
						" OBJ:",
						networkIdentity.gameObject,
						" AS:",
						networkIdentity.AssetId
					}));
				}
				else
				{
					Debug.Log("ID:" + networkInstanceId + " OBJ: null");
				}
			}
		}
	}
}