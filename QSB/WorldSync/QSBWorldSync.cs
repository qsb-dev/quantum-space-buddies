using OWML.Common;
using QSB.OrbSync.TransformSync;
using QSB.OrbSync.WorldObjects;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.WorldSync
{
	public static class QSBWorldSync
	{
		public static List<NomaiInterfaceOrb> OldOrbList { get; set; } = new();
		public static List<CharacterDialogueTree> OldDialogueTrees { get; set; } = new();
		public static Dictionary<string, bool> DialogueConditions { get; } = new();
		public static List<FactReveal> ShipLogFacts { get; } = new();

		private static readonly List<IWorldObject> WorldObjects = new();
		private static readonly Dictionary<MonoBehaviour, IWorldObject> WorldObjectsToUnityObjects = new();

		public static IEnumerable<TWorldObject> GetWorldObjects<TWorldObject>()
			where TWorldObject : IWorldObject
			=> WorldObjects.OfType<TWorldObject>();

		public static TWorldObject GetWorldFromId<TWorldObject>(int id)
			where TWorldObject : IWorldObject
		{
			var worldObjects = GetWorldObjects<TWorldObject>().ToList();
			if (id < 0 || id >= worldObjects.Count)
			{
				DebugLog.ToConsole($"Warning - Tried to find {typeof(TWorldObject).Name} id {id}. Count is {worldObjects.Count}.", MessageType.Warning);
				return default;
			}

			return worldObjects[id];
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

		public static void RemoveWorldObjects()
		{
			if (WorldObjects.Count == 0)
			{
				DebugLog.ToConsole($"Warning - Trying to remove WorldObjects, but there are no WorldObjects!", MessageType.Warning);
				return;
			}

			foreach (var item in WorldObjects)
			{
				try
				{
					item.OnRemoval();
				}
				catch (Exception e)
				{
					DebugLog.ToConsole($"Error - Exception in OnRemoval() for {item.GetType()}. Message : {e.Message}, Stack trace : {e.StackTrace}", MessageType.Error);
				}
			}

			WorldObjects.Clear();
			WorldObjectsToUnityObjects.Clear();
		}

		public static IEnumerable<TUnityObject> GetUnityObjects<TUnityObject>()
			where TUnityObject : MonoBehaviour
			=> Resources.FindObjectsOfTypeAll<TUnityObject>()
				.Where(x => x.gameObject.scene.name != null);

		public static void Init<TWorldObject, TUnityObject>()
			where TWorldObject : WorldObject<TUnityObject>, new()
			where TUnityObject : MonoBehaviour
		{
			var list = GetUnityObjects<TUnityObject>().ToList();
			//DebugLog.DebugWrite($"{typeof(TWorldObject).Name} init : {list.Count} instances.", MessageType.Info);
			for (var id = 0; id < list.Count; id++)
			{
				var obj = new TWorldObject
				{
					AttachedObject = list[id],
					ObjectId = WorldObjects.Count
				};
				obj.Init();
				WorldObjects.Add(obj);
				WorldObjectsToUnityObjects.Add(list[id], obj);
			}
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
