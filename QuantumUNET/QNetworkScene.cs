using QuantumUNET.Components;
using QuantumUNET.Messages;
using System.Collections.Generic;
using UnityEngine;

namespace QuantumUNET
{
	internal class QNetworkScene
	{
		internal static Dictionary<int, GameObject> guidToPrefab { get; } = new Dictionary<int, GameObject>();
		internal static Dictionary<int, QSpawnDelegate> spawnHandlers { get; } = new Dictionary<int, QSpawnDelegate>();
		internal static Dictionary<int, QUnSpawnDelegate> unspawnHandlers { get; } = new Dictionary<int, QUnSpawnDelegate>();
		internal Dictionary<QNetworkInstanceId, QNetworkIdentity> localObjects { get; } = new Dictionary<QNetworkInstanceId, QNetworkIdentity>();

		internal void Shutdown()
		{
			ClearLocalObjects();
			ClearSpawners();
		}

		internal void SetLocalObject(QNetworkInstanceId netId, GameObject obj, bool isClient, bool isServer)
		{
			if (obj == null)
			{
				localObjects[netId] = null;
			}
			else
			{
				QNetworkIdentity networkIdentity = null;
				if (localObjects.ContainsKey(netId))
				{
					networkIdentity = localObjects[netId];
				}

				if (networkIdentity == null)
				{
					networkIdentity = obj.GetComponent<QNetworkIdentity>();
					localObjects[netId] = networkIdentity;
				}

				networkIdentity.UpdateClientServer(isClient, isServer);
			}
		}

		internal GameObject FindLocalObject(QNetworkInstanceId netId)
		{
			if (localObjects.ContainsKey(netId))
			{
				var networkIdentity = localObjects[netId];
				if (networkIdentity != null)
				{
					return networkIdentity.gameObject;
				}
			}

			return null;
		}

		internal bool GetNetworkIdentity(QNetworkInstanceId netId, out QNetworkIdentity uv)
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

		internal bool RemoveLocalObject(QNetworkInstanceId netId)
			=> localObjects.Remove(netId);

		internal bool RemoveLocalObjectAndDestroy(QNetworkInstanceId netId)
		{
			bool result;
			if (localObjects.ContainsKey(netId))
			{
				var networkIdentity = localObjects[netId];
				Object.Destroy(networkIdentity.gameObject);
				result = localObjects.Remove(netId);
			}
			else
			{
				result = false;
			}

			return result;
		}

		internal void ClearLocalObjects()
			=> localObjects.Clear();

		internal static void RegisterPrefab(GameObject prefab, int newAssetId)
		{
			var component = prefab.GetComponent<QNetworkIdentity>();
			if (component)
			{
				component.SetDynamicAssetId(newAssetId);
				guidToPrefab[component.AssetId] = prefab;
			}
			else
			{
				Debug.LogError($"Could not register '{prefab.name}' since it contains no NetworkIdentity component");
			}
		}

		internal static void RegisterPrefab(GameObject prefab)
		{
			var component = prefab.GetComponent<QNetworkIdentity>();
			if (component)
			{
				guidToPrefab[component.AssetId] = prefab;
				var componentsInChildren = prefab.GetComponentsInChildren<QNetworkIdentity>();
				if (componentsInChildren.Length > 1)
				{
					Debug.LogWarning(
						$"The prefab '{prefab.name}' has multiple NetworkIdentity components. There can only be one NetworkIdentity on a prefab, and it must be on the root object.");
				}
			}
			else
			{
				Debug.LogError($"Could not register '{prefab.name}' since it contains no NetworkIdentity component");
			}
		}

		internal static bool GetPrefab(int assetId, out GameObject prefab)
		{
			bool result;
			if (assetId == 0)
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

		public static void UnregisterSpawnHandler(int assetId)
		{
			spawnHandlers.Remove(assetId);
			unspawnHandlers.Remove(assetId);
		}

		internal static void RegisterSpawnHandler(int assetId, QSpawnDelegate spawnHandler, QUnSpawnDelegate unspawnHandler)
		{
			if (spawnHandler == null || unspawnHandler == null)
			{
				Debug.LogError($"RegisterSpawnHandler custom spawn function null for {assetId}");
			}
			else
			{
				spawnHandlers[assetId] = spawnHandler;
				unspawnHandlers[assetId] = unspawnHandler;
			}
		}

		internal static void UnregisterPrefab(GameObject prefab)
		{
			var component = prefab.GetComponent<QNetworkIdentity>();
			if (component == null)
			{
				Debug.LogError($"Could not unregister '{prefab.name}' since it contains no NetworkIdentity component");
			}
			else
			{
				spawnHandlers.Remove(component.AssetId);
				unspawnHandlers.Remove(component.AssetId);
			}
		}

		internal static void RegisterPrefab(GameObject prefab, QSpawnDelegate spawnHandler, QUnSpawnDelegate unspawnHandler)
		{
			var component = prefab.GetComponent<QNetworkIdentity>();
			if (component == null)
			{
				Debug.LogError($"Could not register '{prefab.name}' since it contains no NetworkIdentity component");
			}
			else if (spawnHandler == null || unspawnHandler == null)
			{
				Debug.LogError($"RegisterPrefab custom spawn function null for {component.AssetId}");
			}
			else if (component.AssetId == 0)
			{
				Debug.LogError($"RegisterPrefab game object {prefab.name} has no prefab. Use RegisterSpawnHandler() instead?");
			}
			else
			{
				spawnHandlers[component.AssetId] = spawnHandler;
				unspawnHandlers[component.AssetId] = unspawnHandler;
			}
		}

		internal static bool GetSpawnHandler(int assetId, out QSpawnDelegate handler)
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

		internal static bool InvokeUnSpawnHandler(int assetId, GameObject obj)
		{
			bool result;
			if (unspawnHandlers.ContainsKey(assetId) && unspawnHandlers[assetId] != null)
			{
				var unSpawnDelegate = unspawnHandlers[assetId];
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
			foreach (var key in localObjects.Keys)
			{
				var networkIdentity = localObjects[key];
				if (networkIdentity != null && networkIdentity.gameObject != null)
				{
					if (!InvokeUnSpawnHandler(networkIdentity.AssetId, networkIdentity.gameObject))
					{
						if (networkIdentity.SceneId.IsEmpty())
						{
							Object.Destroy(networkIdentity.gameObject);
						}
						else
						{
							networkIdentity.MarkForReset();
							networkIdentity.gameObject.SetActive(false);
						}
					}
				}
			}

			ClearLocalObjects();
		}
	}
}