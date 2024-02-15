using Cysharp.Threading.Tasks;
using MonoMod.Utils;
using OWML.Common;
using QSB.ConversationSync.Messages;
using QSB.LogSync;
using QSB.LogSync.Messages;
using QSB.Messaging;
using QSB.Player.TransformSync;
using QSB.TriggerSync.WorldObjects;
using QSB.Utility;
using QSB.Utility.Deterministic;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace QSB.WorldSync;

public static class QSBWorldSync
{
	public static WorldObjectManager[] Managers;

	private static readonly Dictionary<string, List<IWorldObject>> _managerToBuiltObjects = new();
	public static readonly Dictionary<string, (string hash, int count)> ManagerHashes = new();

	/// <summary>
	/// Set when all WorldObjectManagers have called Init() on all their objects (AKA all the objects are created)
	/// </summary>
	public static bool AllObjectsAdded { get; private set; }
	/// <summary>
	/// Set when all WorldObjects have finished running Init()
	/// </summary>
	public static bool AllObjectsReady { get; private set; }

	private static CancellationTokenSource _cts;
	private static readonly Dictionary<WorldObjectManager, UniTask> _managersBuilding = new();
	private static readonly Dictionary<IWorldObject, UniTask> _objectsIniting = new();

	public static async UniTaskVoid BuildWorldObjects(OWScene scene)
	{
		if (_cts != null)
		{
			return;
		}

		_cts = new CancellationTokenSource();

		if (!PlayerTransformSync.LocalInstance)
		{
			DebugLog.ToConsole("Warning - Tried to build WorldObjects when LocalPlayer is not ready! Building when ready...", MessageType.Warning);
			await UniTask.WaitUntil(() => PlayerTransformSync.LocalInstance, cancellationToken: _cts.Token);
		}

		GameInit();

		foreach (var manager in Managers)
		{
			if (ShouldIgnoreManager(manager))
			{
				continue;
			}

			var task = UniTask.Create(async () =>
			{
				await manager.Try("building world objects", async () =>
				{
					await manager.BuildWorldObjects(scene, _cts.Token);
					DebugLog.DebugWrite($"Built {manager}", MessageType.Info);
				});
				_managersBuilding.Remove(manager);
			});
			if (!task.Status.IsCompleted())
			{
				_managersBuilding.Add(manager, task);
			}
		}

		await _managersBuilding.Values;
		if (_cts == null)
		{
			return;
		}

		AllObjectsAdded = true;
		DebugLog.DebugWrite("World Objects added.", MessageType.Success);

		DeterministicManager.OnWorldObjectsAdded();

		foreach (var item in _managerToBuiltObjects)
		{
			var worldObjects = item.Value;
			var objects = worldObjects.Select(x => x.GetType().Name);
			var hash = objects.GetMD5Hash();
			ManagerHashes[item.Key] = (hash, objects.Count());
		}

		if (!QSBCore.IsHost)
		{
			foreach (var item in ManagerHashes)
			{
				new WorldObjectsHashMessage(item.Key, item.Value.hash, item.Value.count).Send();
			}

			new RequestLinksMessage().Send();
		}

		await _objectsIniting.Values;
		if (_cts == null)
		{
			return;
		}

		AllObjectsReady = true;
		DebugLog.DebugWrite("World Objects ready.", MessageType.Success);

		if (!QSBCore.IsHost)
		{
			new RequestInitialStatesMessage().Send();
		}
	}

	public static void RemoveWorldObjects()
	{
		if (_cts == null)
		{
			return;
		}

		if (_managersBuilding.Count > 0)
		{
			DebugLog.DebugWrite($"{_managersBuilding.Count} managers still building", MessageType.Warning);
		}

		if (_objectsIniting.Count > 0)
		{
			DebugLog.DebugWrite($"{_objectsIniting.Count} objects still initing", MessageType.Warning);
		}

		_cts.Cancel();
		_cts.Dispose();
		_cts = null;

		_managersBuilding.Clear();
		_objectsIniting.Clear();
		AllObjectsAdded = false;
		AllObjectsReady = false;

		_managerToBuiltObjects.Clear();
		ManagerHashes.Clear();

		GameReset();

		foreach (var worldObject in WorldObjects)
		{
			worldObject.Try("removing", worldObject.OnRemoval);
			RequestInitialStatesMessage.SendInitialState -= worldObject.SendInitialState;
		}

		WorldObjects.Clear();
		UnityObjectsToWorldObjects.Clear();

		foreach (var manager in Managers)
		{
			if (ShouldIgnoreManager(manager))
			{
				continue;
			}

			manager.Try("unbuilding world objects", manager.UnbuildWorldObjects);
		}
	}

	private static bool ShouldIgnoreManager(WorldObjectManager manager)
	{
		if (manager.DlcOnly && !QSBCore.DLCInstalled)
		{
			return true;
		}

		switch (manager.WorldObjectScene)
		{
			case WorldObjectScene.SolarSystem when QSBSceneManager.CurrentScene != OWScene.SolarSystem:
			case WorldObjectScene.Eye when QSBSceneManager.CurrentScene != OWScene.EyeOfTheUniverse:
				return true;
		}

		return false;
	}

	// =======================================================================================================

	private static readonly Dictionary<string, bool> DialogueConditions = new();
	private static readonly Dictionary<string, bool> PersistentConditions = new();
	private static readonly List<FactReveal> ShipLogFacts = new();

	private static readonly List<IWorldObject> WorldObjects = new();
	private static readonly Dictionary<MonoBehaviour, IWorldObject> UnityObjectsToWorldObjects = new();

	static QSBWorldSync()
	{
		QSBSceneManager.OnPreSceneLoad += (_, _) =>
			RemoveWorldObjects();
		QSBSceneManager.OnPostSceneLoad += (_, loadScene) =>
		{
			if (QSBCore.IsInMultiplayer && loadScene.IsUniverseScene())
			{
				// So objects have time to be deleted, made, whatever
				// i.e. wait until Start has been called
				Delay.RunNextFrame(() => BuildWorldObjects(loadScene).Forget());
			}
		};

		RequestInitialStatesMessage.SendInitialState += to =>
		{
			DialogueConditions.ForEach(condition
				=> new DialogueConditionMessage(condition.Key, condition.Value) { To = to }.Send());

			ShipLogFacts.ForEach(fact
				=> new RevealFactMessage(fact.Id, fact.SaveGame, false) { To = to }.Send());
		};
	}

	private static void GameInit()
	{
		DebugLog.DebugWrite("GameInit QSBWorldSync", MessageType.Info);

		if (!QSBCore.IsHost)
		{
			return;
		}

		DialogueConditions.Clear();
		DialogueConditions.AddRange(DialogueConditionManager.SharedInstance._dictConditions);

		PersistentConditions.Clear();
		PersistentConditions.AddRange(PlayerData._currentGameSave.dictConditions);
	}

	private static void GameReset()
	{
		DebugLog.DebugWrite("GameReset QSBWorldSync", MessageType.Info);

		DialogueConditions.Clear();
		PersistentConditions.Clear();
		ShipLogFacts.Clear();
	}

	public static IEnumerable<IWorldObject> GetWorldObjects() => WorldObjects;

	public static IEnumerable<IWorldObject> GetWorldObjectsFromManager(string managerName) => _managerToBuiltObjects[managerName];

	public static IEnumerable<TWorldObject> GetWorldObjects<TWorldObject>()
		where TWorldObject : IWorldObject
		=> WorldObjects.OfType<TWorldObject>();

	public static IEnumerable<IWorldObject> GetWorldObjects(Type type)
		=> WorldObjects.Where(type.IsInstanceOfType);

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
			DebugLog.ToConsole($"Error - WorldObject id {objectId} is {WorldObjects[objectId].GetType().Name}, expected {typeof(TWorldObject).Name}.", MessageType.Error);
			return default;
		}

		return worldObject;
	}

	public static TWorldObject GetWorldObject<TWorldObject>(this MonoBehaviour unityObject)
		where TWorldObject : IWorldObject
	{
		if (!unityObject)
		{
			DebugLog.ToConsole($"Error - Trying to run GetWorldFromUnity with a null unity object! TWorldObject:{typeof(TWorldObject).Name}, TUnityObject:NULL, Stacktrace:\r\n{Environment.StackTrace}", MessageType.Error);
			return default;
		}

		if (!UnityObjectsToWorldObjects.TryGetValue(unityObject, out var worldObject))
		{
			DebugLog.ToConsole($"Error - UnityObjectsToWorldObjects does not contain \"{unityObject.name}\"! TWorldObject:{typeof(TWorldObject).Name}, TUnityObject:{unityObject.GetType().Name}, Stacktrace:\r\n{Environment.StackTrace}", MessageType.Error);
			return default;
		}

		return (TWorldObject)worldObject;
	}

	public static bool TryGetWorldObject<TWorldObject>(this MonoBehaviour unityObject, out TWorldObject worldObject)
		where TWorldObject : IWorldObject
	{
		if (!unityObject)
		{
			DebugLog.ToConsole($"Error - Trying to run GetWorldFromUnity with a null unity object! TWorldObject:{typeof(TWorldObject).Name}, TUnityObject:NULL, Stacktrace:\r\n{Environment.StackTrace}", MessageType.Error);
			worldObject = default;
			return false;
		}

		if (!UnityObjectsToWorldObjects.TryGetValue(unityObject, out var iWorldObject))
		{
			worldObject = default;
			return false;
		}

		worldObject = (TWorldObject)iWorldObject;
		return true;
	}

	/// <summary>
	/// not deterministic across platforms.
	/// iterates thru all objects and throws error if there isn't exactly 1.
	/// </summary>
	public static TUnityObject GetUnityObject<TUnityObject>()
		where TUnityObject : MonoBehaviour
		=> GetUnityObjects<TUnityObject>().Single();

	/// <summary>
	/// not deterministic across platforms.
	/// excludes prefabs and DontDestroyOnLoad objects.
	/// </summary>
	public static IEnumerable<TUnityObject> GetUnityObjects<TUnityObject>()
		where TUnityObject : MonoBehaviour
		=> Resources.FindObjectsOfTypeAll<TUnityObject>()
			.Where(x => x.gameObject.scene.name is not (null or "DontDestroyOnLoad"));

	// https://stackoverflow.com/a/48570616
	private static string NameOfCallingClass()
	{
		string fullName;
		Type declaringType;
		var skipFrames = 2;
		do
		{
			var method = new StackFrame(skipFrames, false).GetMethod();
			declaringType = method.DeclaringType;
			if (declaringType == null)
			{
				return method.Name;
			}

			skipFrames++;
			fullName = CleanupFullName(declaringType.FullName);
		}
		while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase) || declaringType == typeof(QSBWorldSync));

		return fullName;
	}

	private static string CleanupFullName(string fullName)
	{
		var ret = fullName;

		var indexOfPlus = fullName.LastIndexOf('+');
		if (indexOfPlus != -1)
		{
			ret = fullName.Remove(indexOfPlus);
		}

		var indexOfDot = ret.LastIndexOf('.');
		if (indexOfDot != -1)
		{
			ret = ret.Substring(indexOfDot + 1);
		}

		return ret;
	}

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
		foreach (var item in listToInitFrom)
		{
			var obj = new TWorldObject
			{
				AttachedObject = item,
				ObjectId = WorldObjects.Count
			};
			AddAndInit(obj, item);
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
			AddAndInit(obj, item);
		}
	}

	private static void AddAndInit<TWorldObject, TUnityObject>(TWorldObject worldObject, TUnityObject unityObject)
		where TWorldObject : WorldObject<TUnityObject>
		where TUnityObject : MonoBehaviour
	{
		if (!UnityObjectsToWorldObjects.TryAdd(unityObject, worldObject))
		{
			DebugLog.ToConsole($"Error - UnityObjectsToWorldObjects already contains \"{unityObject.name}\"! TWorldObject:{typeof(TWorldObject).Name}, TUnityObject:{unityObject.GetType().Name}, Stacktrace:\r\n{Environment.StackTrace}", MessageType.Error);
			return;
		}

		var className = NameOfCallingClass();
		if (!_managerToBuiltObjects.ContainsKey(className))
		{
			_managerToBuiltObjects.Add(className, new List<IWorldObject> { worldObject });
		}
		else
		{
			_managerToBuiltObjects[className].Add(worldObject);
		}

		WorldObjects.Add(worldObject);
		RequestInitialStatesMessage.SendInitialState += worldObject.SendInitialState;

		var task = UniTask.Create(async () =>
		{
			await worldObject.Try("initing", () => worldObject.Init(_cts.Token));
			_objectsIniting.Remove(worldObject);
		});
		if (!task.Status.IsCompleted())
		{
			_objectsIniting.Add(worldObject, task);
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
