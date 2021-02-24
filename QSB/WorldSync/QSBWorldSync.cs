using OWML.Common;
using QSB.OrbSync.WorldObjects;
using QSB.TransformSync;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace QSB.WorldSync
{
	public static class QSBWorldSync
	{
		public static List<NomaiOrbTransformSync> OrbSyncList { get; } = new List<NomaiOrbTransformSync>();
		public static List<NomaiInterfaceOrb> OldOrbList { get; set; } = new List<NomaiInterfaceOrb>();
		public static List<CharacterDialogueTree> OldDialogueTrees { get; set; } = new List<CharacterDialogueTree>();
		public static Dictionary<string, bool> DialogueConditions { get; } = new Dictionary<string, bool>();
		public static List<FactReveal> ShipLogFacts { get; } = new List<FactReveal>();

		private static readonly List<IWorldObject> WorldObjects = new List<IWorldObject>();

		public static IEnumerable<TWorldObject> GetWorldObjects<TWorldObject>()
			=> WorldObjects.OfType<TWorldObject>();

		public static TWorldObject GetWorldFromId<TWorldObject>(int id)
			=> GetWorldObjects<TWorldObject>().ToList()[id];

		public static TWorldObject GetWorldFromUnity<TWorldObject, TUnityObject>(TUnityObject unityObject)
			where TWorldObject : WorldObject<TUnityObject>
			where TUnityObject : MonoBehaviour
		{
			var allWorldObjects = GetWorldObjects<TWorldObject>();
			if (allWorldObjects.Count() == 0)
			{
				DebugLog.ToConsole($"Error - No worldobjects exist of type {typeof(TWorldObject).Name}!", MessageType.Error);
				return null;
			}
			var correctWorldObject = allWorldObjects.First(x => x.AttachedObject == unityObject);
			return correctWorldObject;
		}

		public static int GetIdFromUnity<TWorldObject, TUnityObject>(TUnityObject unityObject)
			where TWorldObject : WorldObject<TUnityObject>
			where TUnityObject : MonoBehaviour
			=> GetWorldFromUnity<TWorldObject, TUnityObject>(unityObject).ObjectId;

		public static int GetIdFromTypeSubset<TTypeSubset>(TTypeSubset typeSubset)
			where TTypeSubset : IWorldObjectTypeSubset
			=> GetWorldObjects<TTypeSubset>().ToList().IndexOf(typeSubset);

		public static void RemoveWorldObjects<TWorldObject>()
		{
			var itemsToRemove = WorldObjects.Where(x => x is TWorldObject);
			foreach (var item in itemsToRemove)
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
			DebugLog.DebugWrite($"Removing {typeof(TWorldObject).Name} : {WorldObjects.Count(x => x is TWorldObject)} instances.");
			WorldObjects.RemoveAll(x => x is TWorldObject);
		}

		public static List<TUnityObject> Init<TWorldObject, TUnityObject>()
			where TWorldObject : WorldObject<TUnityObject>
			where TUnityObject : MonoBehaviour
		{
			RemoveWorldObjects<TWorldObject>();
			var list = Resources.FindObjectsOfTypeAll<TUnityObject>().ToList();
			DebugLog.DebugWrite($"{typeof(TWorldObject).Name} init : {list.Count} instances.", MessageType.Info);
			for (var id = 0; id < list.Count; id++)
			{
				var obj = CreateWorldObject<TWorldObject>();
				obj.Init(list[id], id);
			}
			return list;
		}

		private static TWorldObject CreateWorldObject<TWorldObject>()
			where TWorldObject : IWorldObject
		{
			var worldObject = (TWorldObject)Activator.CreateInstance(typeof(TWorldObject));
			WorldObjects.Add(worldObject);
			return worldObject;
		}

		public static void RaiseEvent(object instance, string eventName)
		{
			if (!(instance.GetType()
				.GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic)?
				.GetValue(instance) is MulticastDelegate multiDelegate))
			{
				return;
			}
			var delegateList = multiDelegate.GetInvocationList().ToList();
			foreach (var del in delegateList)
			{
				del.DynamicInvoke(instance);
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
			var orbSync = OrbSyncList.First(x => x.AttachedOrb == affectingOrb);
			if (orbSync.HasAuthority)
			{
				qsbSlot.HandleEvent(state, OldOrbList.IndexOf(affectingOrb));
			}
		}

		public static void SetDialogueCondition(string name, bool state)
		{
			if (!QSBCore.IsServer)
			{
				DebugLog.ToConsole("Warning - Cannot write to condition dict when not server!", MessageType.Warning);
				return;
			}
			DialogueConditions[name] = state;
		}

		public static void AddFactReveal(string id, bool saveGame, bool showNotification)
		{
			if (!QSBCore.IsServer)
			{
				DebugLog.ToConsole("Warning - Cannot write to fact list when not server!", MessageType.Warning);
				return;
			}
			if (ShipLogFacts.Any(x => x.Id == id))
			{
				DebugLog.ToConsole($"Warning - Fact id {id} already in list!", MessageType.Warning);
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