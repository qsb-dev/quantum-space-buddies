using System;
using System.Collections.Generic;
using System.Linq;
using OWML.Common;
using QSB.OrbSync.TransformSync;
using QSB.OrbSync.WorldObjects;
using QSB.Utility;
using UnityEngine;

namespace QSB.WorldSync
{
	public static class QSBWorldSync
	{
		public static List<NomaiInterfaceOrb> OldOrbList { get; set; } = new();
		public static List<CharacterDialogueTree> OldDialogueTrees { get; set; } = new();
		public static Dictionary<string, bool> DialogueConditions { get; } = new();
		public static List<FactReveal> ShipLogFacts { get; } = new();

		public static int NextObjectId;
		private static readonly List<IWorldObject> WorldObjects = new();
		private static readonly Dictionary<MonoBehaviour, IWorldObject> WorldObjectsToUnityObjects = new();

		public static IEnumerable<TWorldObject> GetWorldObjects<TWorldObject>()
			=> WorldObjects.OfType<TWorldObject>();

		public static TWorldObject GetWorldFromId<TWorldObject>(int id)
		{
			if (id < 0 || id >= WorldObjects.Count)
			{
				DebugLog.ToConsole($"Warning - Tried to find world object id {id}. Count is {WorldObjects.Count}.", MessageType.Warning);
				return default;
			}

			var iWorldObject = WorldObjects[id];
			if (iWorldObject is not TWorldObject worldObject)
			{
				DebugLog.ToConsole($"Error - World object id {id} expected {typeof(TWorldObject).Name}, but got {iWorldObject.GetType().Name}.", MessageType.Error);
				return default;
			}

			return worldObject;
		}

		public static IWorldObject GetWorldFromUnity(MonoBehaviour unityObject)
		{
			if (unityObject == null)
			{
				DebugLog.ToConsole($"Error - Trying to run GetWorldFromUnity with a null unity object! TUnityObject:NULL, Stacktrace:\r\n{Environment.StackTrace}", MessageType.Error);
				return default;
			}

			if (!QSBCore.IsInMultiplayer)
			{
				DebugLog.ToConsole($"Warning - Trying to run GetWorldFromUnity while not in multiplayer! TUnityObject:{unityObject.GetType().Name}, Stacktrace:\r\n{Environment.StackTrace}", MessageType.Warning);
				return default;
			}

			if (!WorldObjectsToUnityObjects.TryGetValue(unityObject, out var returnObject))
			{
				DebugLog.ToConsole($"Error - WorldObjectsToUnityObjects does not contain \"{unityObject.name}\"! TUnityObject:{unityObject.GetType().Name}, Stacktrace:\r\n{Environment.StackTrace}", MessageType.Error);
				return default;
			}

			if (returnObject == null)
			{
				DebugLog.ToConsole($"Error - World object for unity object {unityObject.name} is null! TUnityObject:{unityObject.GetType().Name}, Stacktrace:\r\n{Environment.StackTrace}", MessageType.Error);
				return default;
			}

			return returnObject;
		}

		public static TWorldObject GetWorldFromUnity<TWorldObject>(MonoBehaviour unityObject)
			where TWorldObject : IWorldObject
		{
			if (unityObject == null)
			{
				DebugLog.ToConsole($"Error - Trying to run GetWorldFromUnity with a null unity object! TWorldObject:{typeof(TWorldObject).Name}, TUnityObject:NULL, Stacktrace:\r\n{Environment.StackTrace}", MessageType.Error);
				return default;
			}

			if (!QSBCore.IsInMultiplayer)
			{
				DebugLog.ToConsole($"Warning - Trying to run GetWorldFromUnity while not in multiplayer! TWorldObject:{typeof(TWorldObject).Name}, TUnityObject:{unityObject.GetType().Name}, Stacktrace:\r\n{Environment.StackTrace}", MessageType.Warning);
				return default;
			}

			if (!WorldObjectsToUnityObjects.TryGetValue(unityObject, out var returnObject))
			{
				DebugLog.ToConsole($"Error - WorldObjectsToUnityObjects does not contain \"{unityObject.name}\"! TWorldObject:{typeof(TWorldObject).Name}, TUnityObject:{unityObject.GetType().Name}, Stacktrace:\r\n{Environment.StackTrace}", MessageType.Error);
				return default;
			}

			if (returnObject == null)
			{
				DebugLog.ToConsole($"Error - World object for unity object {unityObject.name} is null! TWorldObject:{typeof(TWorldObject).Name}, TUnityObject:{unityObject.GetType().Name}, Stacktrace:\r\n{Environment.StackTrace}", MessageType.Error);
				return default;
			}

			return (TWorldObject)returnObject;
		}

		public static int GetIdFromUnity<TWorldObject>(MonoBehaviour unityObject)
			where TWorldObject : IWorldObject
			=> GetWorldFromUnity<TWorldObject>(unityObject).ObjectId;

		public static int GetIdFromTypeSubset<TTypeSubset>(TTypeSubset typeSubset)
		{
			return ((IWorldObject)typeSubset).ObjectId;

			var index = GetWorldObjects<TTypeSubset>().ToList().IndexOf(typeSubset);
			if (index == -1)
			{
				DebugLog.ToConsole($"Warning - {((IWorldObject)typeSubset).Name} doesn't exist in list of {typeof(TTypeSubset).Name} !", MessageType.Warning);
			}

			return index;
		}

		public static void RemoveWorldObjects<TWorldObject>()
		{
			if (WorldObjects.Count == 0)
			{
				DebugLog.ToConsole($"Warning - Trying to remove WorldObjects of type {typeof(TWorldObject).Name}, but there are no WorldObjects!", MessageType.Warning);
				return;
			}

			var itemsToRemove = WorldObjects.Where(x => x is TWorldObject);

			foreach (var item in itemsToRemove)
			{
				try
				{
					WorldObjectsToUnityObjects.Remove(item.ReturnObject());
					item.OnRemoval();
				}
				catch (Exception e)
				{
					DebugLog.ToConsole($"Error - Exception in OnRemoval() for {item.GetType()}. Message : {e.Message}, Stack trace : {e.StackTrace}", MessageType.Error);
				}
			}

			WorldObjects.RemoveAll(x => x is TWorldObject);
		}

		public static IEnumerable<TUnityObject> GetUnityObjects<TUnityObject>()
			where TUnityObject : MonoBehaviour
			=> Resources.FindObjectsOfTypeAll<TUnityObject>()
				.Where(x => x.gameObject.scene.name != null);

		public static void Init<TWorldObject, TUnityObject>()
			where TWorldObject : WorldObject<TUnityObject>, new()
			where TUnityObject : MonoBehaviour
		{
			RemoveWorldObjects<TWorldObject>();
			var list = GetUnityObjects<TUnityObject>().ToList();
			//DebugLog.DebugWrite($"{typeof(TWorldObject).Name} init : {list.Count} instances.", MessageType.Info);
			foreach (var unityObject in list)
			{
				var obj = CreateWorldObject<TWorldObject>();
				obj.Init(unityObject, NextObjectId++);
				WorldObjectsToUnityObjects.Add(unityObject, obj);
			}
		}

		private static TWorldObject CreateWorldObject<TWorldObject>()
			where TWorldObject : IWorldObject, new()
		{
			var worldObject = new TWorldObject();
			WorldObjects.Add(worldObject);

			return worldObject;
		}

		public static void HandleSlotStateChange(NomaiInterfaceSlot slot, NomaiInterfaceOrb affectingOrb, bool state)
		{
			var slotList = GetWorldObjects<QSBOrbSlot>().ToList();
			if (!slotList.Any())
			{
				return;
			}

			var qsbSlot = slotList.FirstOrDefault(x => x.AttachedObject == slot);
			if (qsbSlot == null)
			{
				DebugLog.ToConsole($"Error - No QSBOrbSlot found for {slot.name}!", MessageType.Error);
				return;
			}

			var orbSync = NomaiOrbTransformSync.OrbTransformSyncs.Where(x => x != null).FirstOrDefault(x => x.AttachedObject == affectingOrb.transform);
			if (orbSync == null)
			{
				DebugLog.ToConsole($"Error - No NomaiOrbTransformSync found for {affectingOrb.name} (For slot {slot.name})!", MessageType.Error);
				return;
			}

			if (orbSync.HasAuthority)
			{
				qsbSlot.HandleEvent(state, OldOrbList.IndexOf(affectingOrb));
			}
		}

		public static void SetDialogueCondition(string name, bool state)
		{
			if (!QSBCore.IsHost)
			{
				DebugLog.ToConsole("Warning - Cannot write to condition dict when not server!", MessageType.Warning);
				return;
			}

			DialogueConditions[name] = state;
		}

		public static void AddFactReveal(string id, bool saveGame, bool showNotification)
		{
			if (!QSBCore.IsHost)
			{
				DebugLog.ToConsole("Warning - Cannot write to fact list when not server!", MessageType.Warning);
				return;
			}

			if (ShipLogFacts.Any(x => x.Id == id))
			{
				return;
			}

			ShipLogFacts.Add(new FactReveal
			{
				Id = id,
				SaveGame = saveGame,
				ShowNotification = showNotification
			});
		}
	}
}
