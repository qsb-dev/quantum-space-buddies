using OWML.Common;
using QSB.ConversationSync.Patches;
using QSB.LogSync;
using QSB.Player.TransformSync;
using QSB.TriggerSync.WorldObjects;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.WorldSync
{
	public static class QSBWorldSync
	{
		public static WorldObjectManager[] Managers;

		/// <summary>
		/// Set when all WorldObjectManagers have called Init() on all their objects (AKA all the objects are created)
		/// </summary>
		public static bool AllObjectsAdded { get; private set; }
		/// <summary>
		/// Set when all WorldObjects have finished running Init()
		/// </summary>
		public static bool AllObjectsReady { get; private set; }

		public static void BuildWorldObjects(OWScene scene)
		{
			if (PlayerTransformSync.LocalInstance == null)
			{
				DebugLog.ToConsole($"Warning - Tried to build WorldObjects when LocalPlayer is not ready! Building when ready...", MessageType.Warning);
				QSBCore.UnityEvents.RunWhen(() => PlayerTransformSync.LocalInstance, () => BuildWorldObjects(scene));
				return;
			}

			GameInit();

			DoBuild(scene);
		}

		private static void DoBuild(OWScene scene)
		{
			foreach (var manager in Managers)
			{
				switch (manager.WorldObjectType)
				{
					case WorldObjectType.SolarSystem when QSBSceneManager.CurrentScene != OWScene.SolarSystem:
					case WorldObjectType.Eye when QSBSceneManager.CurrentScene != OWScene.EyeOfTheUniverse:
						DebugLog.DebugWrite($"skipping {manager.GetType().Name} as it is type {manager.WorldObjectType} and scene is {QSBSceneManager.CurrentScene}");
						continue;
				}

				try
				{
					DebugLog.DebugWrite($"Building {manager.GetType().Name}", MessageType.Info);
					manager.BuildWorldObjects(scene);
				}
				catch (Exception ex)
				{
					DebugLog.ToConsole($"Exception - Exception when trying to build WorldObjects of manager {manager.GetType().Name} : {ex}", MessageType.Error);
				}
			}

			QSBCore.UnityEvents.RunWhen(() => _numManagersReadying == 0, () =>
			{
				AllObjectsAdded = true;
				DebugLog.DebugWrite("World Objects added.", MessageType.Success);
				QSBCore.UnityEvents.RunWhen(() => _numObjectsReadying == 0, () =>
				{
					AllObjectsReady = true;
					DebugLog.DebugWrite("World Objects ready.", MessageType.Success);
				});
			});
		}

		internal static uint _numManagersReadying;
		internal static uint _numObjectsReadying;

		public static void RemoveWorldObjects()
		{
			GameReset();

			AllObjectsAdded = false;
			AllObjectsReady = false;
			_numManagersReadying = 0;
			_numObjectsReadying = 0;

			foreach (var item in WorldObjects)
			{
				try
				{
					item.OnRemoval();
				}
				catch (Exception e)
				{
					DebugLog.ToConsole($"Error - Exception in OnRemoval() for {item.GetType()}. {e}", MessageType.Error);
				}
			}

			WorldObjects.Clear();
			WorldObjectsToUnityObjects.Clear();

			foreach (var manager in Managers)
			{
				manager.UnbuildWorldObjects();
			}
		}

		// =======================================================================================================

		public static List<CharacterDialogueTree> OldDialogueTrees { get; } = new();
		public static Dictionary<string, bool> DialogueConditions { get; private set; } = new();
		private static Dictionary<string, bool> PersistentConditions { get; set; } = new();
		public static List<FactReveal> ShipLogFacts { get; } = new();

		private static readonly List<IWorldObject> WorldObjects = new();
		private static readonly Dictionary<MonoBehaviour, IWorldObject> WorldObjectsToUnityObjects = new();

		private static void GameInit()
		{
			DebugLog.DebugWrite($"GameInit QSBWorldSync", MessageType.Info);

			OldDialogueTrees.Clear();
			OldDialogueTrees.AddRange(GetUnityObjects<CharacterDialogueTree>().SortDeterministic());

			if (!QSBCore.IsHost)
			{
				return;
			}

			DebugLog.DebugWrite($"DIALOGUE CONDITIONS :");
			DialogueConditions = (Dictionary<string, bool>)DialogueConditionManager.SharedInstance._dictConditions;
			foreach (var item in DialogueConditions)
			{
				DebugLog.DebugWrite($"- {item.Key}, {item.Value}");
			}

			DebugLog.DebugWrite($"PERSISTENT CONDITIONS :");
			var dictConditions = PlayerData._currentGameSave.dictConditions;
			var syncedConditions = dictConditions.Where(x => ConversationPatches.PersistentConditionsToSync.Contains(x.Key));
			PersistentConditions = syncedConditions.ToDictionary(x => x.Key, x => x.Value);
			foreach (var item in PersistentConditions)
			{
				DebugLog.DebugWrite($"- {item.Key}, {item.Value}");
			}
		}

		private static void GameReset()
		{
			DebugLog.DebugWrite($"GameReset QSBWorldSync", MessageType.Info);

			OldDialogueTrees.Clear();
			DialogueConditions.Clear();
			PersistentConditions.Clear();
			ShipLogFacts.Clear();
		}

		public static IEnumerable<IWorldObject> GetWorldObjects() => WorldObjects;

		public static IEnumerable<TWorldObject> GetWorldObjects<TWorldObject>()
			where TWorldObject : IWorldObject
			=> WorldObjects.OfType<TWorldObject>();

		public static TWorldObject GetWorldObject<TWorldObject>(this int objectId)
			where TWorldObject : IWorldObject
		{
			if (!WorldObjects.IsInRange(objectId))
			{
				DebugLog.ToConsole($"Warning - Tried to find {typeof(TWorldObject).Name} id {objectId}. Count is {WorldObjects.Count}.", MessageType.Warning);
				return default;
			}

			if (WorldObjects[objectId] is not TWorldObject worldObject)
			{
				DebugLog.ToConsole($"Error - {typeof(TWorldObject).Name} id {objectId} is actually {WorldObjects[objectId].GetType().Name}.", MessageType.Error);
				return default;
			}

			return worldObject;
		}

		public static TWorldObject GetWorldObject<TWorldObject>(this MonoBehaviour unityObject)
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

			if (!WorldObjectsToUnityObjects.TryGetValue(unityObject, out var worldObject))
			{
				DebugLog.ToConsole($"Error - WorldObjectsToUnityObjects does not contain \"{unityObject.name}\"! TWorldObject:{typeof(TWorldObject).Name}, TUnityObject:{unityObject.GetType().Name}, Stacktrace:\r\n{Environment.StackTrace}", MessageType.Error);
				return default;
			}

			if (worldObject == null)
			{
				DebugLog.ToConsole($"Error - World object for unity object {unityObject.name} is null! TWorldObject:{typeof(TWorldObject).Name}, TUnityObject:{unityObject.GetType().Name}, Stacktrace:\r\n{Environment.StackTrace}", MessageType.Error);
				return default;
			}

			return (TWorldObject)worldObject;
		}

		/// <summary>
		/// not deterministic across platforms
		/// </summary>
		public static IEnumerable<TUnityObject> GetUnityObjects<TUnityObject>()
			where TUnityObject : MonoBehaviour
			=> Resources.FindObjectsOfTypeAll<TUnityObject>()
				.Where(x => x.gameObject.scene.name != null);

		public static void Init<TWorldObject, TUnityObject>()
			where TWorldObject : WorldObject<TUnityObject>, new()
			where TUnityObject : MonoBehaviour
		{
			var list = GetUnityObjects<TUnityObject>().SortDeterministic();
			Init<TWorldObject, TUnityObject>(list);
		}

		public static void Init<TWorldObject, TUnityObject>(params Type[] typesToExclude)
			where TWorldObject : WorldObject<TUnityObject>, new()
			where TUnityObject : MonoBehaviour
		{
			var list = GetUnityObjects<TUnityObject>()
				.Where(x => !typesToExclude.Contains(x.GetType()))
				.SortDeterministic();
			Init<TWorldObject, TUnityObject>(list);
		}

		/// <summary>
		/// make sure to sort the list!
		/// </summary>
		public static void Init<TWorldObject, TUnityObject>(IEnumerable<TUnityObject> listToInitFrom)
			where TWorldObject : WorldObject<TUnityObject>, new()
			where TUnityObject : MonoBehaviour
		{
			//DebugLog.DebugWrite($"{typeof(TWorldObject).Name} init : {listToInitFrom.Count()} instances.", MessageType.Info);
			foreach (var item in listToInitFrom)
			{
				var obj = new TWorldObject
				{
					AttachedObject = item,
					ObjectId = WorldObjects.Count
				};

				obj.Init();
				WorldObjects.Add(obj);
				WorldObjectsToUnityObjects.Add(item, obj);
			}
		}

		public static void Init<TWorldObject, TUnityObject>(Func<TUnityObject, OWTriggerVolume> triggerSelector)
			where TWorldObject : QSBTrigger<TUnityObject>, new()
			where TUnityObject : MonoBehaviour
		{
			var list = GetUnityObjects<TUnityObject>().SortDeterministic();
			foreach (var owner in list)
			{
				var item = triggerSelector(owner);
				if (!item)
				{
					continue;
				}

				var obj = new TWorldObject
				{
					AttachedObject = item,
					ObjectId = WorldObjects.Count,
					TriggerOwner = owner
				};

				obj.Init();
				WorldObjects.Add(obj);
				WorldObjectsToUnityObjects.Add(item, obj);
			}
		}

		public static void SetDialogueCondition(string name, bool state)
		{
			if (!QSBCore.IsHost)
			{
				DebugLog.ToConsole("Warning - Cannot write to dialogue condition dict when not server!", MessageType.Warning);
				return;
			}

			DialogueConditions[name] = state;
		}

		public static void SetPersistentCondition(string name, bool state)
		{
			if (!QSBCore.IsHost)
			{
				DebugLog.ToConsole("Warning - Cannot write to persistent condition dict when not server!", MessageType.Warning);
				return;
			}

			PersistentConditions[name] = state;
		}

		public static void AddFactReveal(string id, bool saveGame)
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
				SaveGame = saveGame
			});
		}
	}
}
