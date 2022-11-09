using HarmonyLib;
using QSB.WorldSync;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Utility;

/// <summary>
/// TODO make this only do cache clearing on pre scene load when HOSTING instead of just all the time
/// </summary>
public static class DeterministicManager
{
	private static readonly Harmony _harmony = new(typeof(DeterministicManager).FullName);
	private static bool _patched;

	private static readonly Dictionary<Transform, (int SiblingIndex, Transform Parent)> _cache = new();

	public static void Init() =>
		QSBSceneManager.OnPreSceneLoad += (_, _) =>
		{
			DebugLog.DebugWrite("cleared cache");
			_cache.Clear();

			if (!_patched)
			{
				_harmony.PatchAll(typeof(OWRigidbodyPatches));
				_patched = true;
			}
		};

	public static void OnWorldObjectsAdded()
	{
		if (QSBCore.DebugSettings.DumpWorldObjects)
		{
			using (var file = File.CreateText(Path.Combine(QSBCore.Helper.Manifest.ModFolderPath, "world objects.csv")))
			{
				file.WriteLine("world object,deterministic path");
				foreach (var worldObject in QSBWorldSync.GetWorldObjects())
				{
					file.Write('"');
					file.Write(worldObject);
					file.Write('"');
					file.Write(',');
					file.Write('"');
					file.Write(worldObject.AttachedObject.DeterministicPath().Replace("\"", "\"\""));
					file.Write('"');
					file.WriteLine();
				}
			}

			using (var file = File.CreateText(Path.Combine(QSBCore.Helper.Manifest.ModFolderPath, "cache.csv")))
			{
				file.WriteLine("name,instance id,sibling index,parent,parent instance id");
				foreach (var (transform, (siblingIndex, parent)) in _cache)
				{
					file.Write('"');
					file.Write(transform.name.Replace("\"", "\"\""));
					file.Write('"');
					file.Write(',');
					file.Write(transform.GetInstanceID());
					file.Write(',');
					file.Write(siblingIndex);
					file.Write(',');
					file.Write('"');
					file.Write(parent ? parent.name.Replace("\"", "\"\"") : default);
					file.Write('"');
					file.Write(',');
					file.Write(parent ? parent.GetInstanceID() : default);
					file.WriteLine();
				}
			}
		}

		DebugLog.DebugWrite($"cleared cache of {_cache.Count} entries");
		_cache.Clear();

		if (_patched)
		{
			_harmony.UnpatchSelf();
			_patched = false;
		}
	}

	[HarmonyPatch(typeof(OWRigidbody))]
	private static class OWRigidbodyPatches
	{
		private static readonly Dictionary<OWRigidbody, Transform> _setParentQueue = new();

		[HarmonyPrefix]
		[HarmonyPatch(nameof(OWRigidbody.Awake))]
		private static bool Awake(OWRigidbody __instance)
		{
			__instance._transform = __instance.transform;
			_cache.Add(__instance._transform, (__instance._transform.GetSiblingIndex(), __instance._transform.parent));
			if (!__instance._scaleRoot)
			{
				__instance._scaleRoot = __instance._transform;
			}

			CenterOfTheUniverse.TrackRigidbody(__instance);
			__instance._offsetApplier = __instance.gameObject.GetAddComponent<CenterOfTheUniverseOffsetApplier>();
			__instance._offsetApplier.Init(__instance);
			if (__instance._simulateInSector)
			{
				__instance._simulateInSector.OnSectorOccupantsUpdated += __instance.OnSectorOccupantsUpdated;
			}

			__instance._origParent = __instance._transform.parent;
			__instance._origParentBody = __instance._origParent ? __instance._origParent.GetAttachedOWRigidbody() : null;
			if (__instance._transform.parent)
			{
				_setParentQueue[__instance] = null;
			}

			__instance._rigidbody = __instance.GetRequiredComponent<Rigidbody>();
			__instance._rigidbody.interpolation = RigidbodyInterpolation.None;
			if (!__instance._autoGenerateCenterOfMass)
			{
				__instance._rigidbody.centerOfMass = __instance._centerOfMass;
			}

			if (__instance.IsSimulatedKinematic())
			{
				__instance.EnableKinematicSimulation();
			}

			__instance._origCenterOfMass = __instance.RunningKinematicSimulation() ? __instance._kinematicRigidbody.centerOfMass : __instance._rigidbody.centerOfMass;
			__instance._referenceFrame = new ReferenceFrame(__instance);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(OWRigidbody.Start))]
		private static void Start(OWRigidbody __instance)
		{
			if (_setParentQueue.TryGetValue(__instance, out var parent))
			{
				__instance._transform.parent = parent;
				_setParentQueue.Remove(__instance);
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(OWRigidbody.OnDestroy))]
		private static void OnDestroy(OWRigidbody __instance)
		{
			_cache.Remove(__instance._transform);
			_setParentQueue.Remove(__instance);
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(OWRigidbody.Suspend), typeof(Transform), typeof(OWRigidbody))]
		private static bool Suspend(OWRigidbody __instance, Transform suspensionParent, OWRigidbody suspensionBody)
		{
			if (!__instance._suspended || __instance._unsuspendNextUpdate)
			{
				__instance._suspensionBody = suspensionBody;
				var direction = __instance.GetVelocity() - suspensionBody.GetPointVelocity(__instance._transform.position);
				__instance._cachedRelativeVelocity = suspensionBody.transform.InverseTransformDirection(direction);
				__instance._cachedAngularVelocity = __instance.RunningKinematicSimulation() ? __instance._kinematicRigidbody.angularVelocity : __instance._rigidbody.angularVelocity;
				__instance.enabled = false;
				__instance._offsetApplier.enabled = false;
				if (__instance.RunningKinematicSimulation())
				{
					__instance._kinematicRigidbody.enabled = false;
				}
				else
				{
					__instance.MakeKinematic();
				}

				if (_setParentQueue.ContainsKey(__instance))
				{
					_setParentQueue[__instance] = suspensionParent;
				}
				else
				{
					__instance._transform.parent = suspensionParent;
				}

				__instance._suspended = true;
				__instance._unsuspendNextUpdate = false;
				if (!Physics.autoSyncTransforms)
				{
					Physics.SyncTransforms();
				}

				if (__instance._childColliders == null)
				{
					__instance._childColliders = __instance.GetComponentsInChildren<Collider>();
					foreach (var childCollider in __instance._childColliders)
					{
						childCollider.gameObject.GetAddComponent<OWCollider>().ListenForParentBodySuspension();
					}
				}

				__instance.RaiseEvent(nameof(__instance.OnSuspendOWRigidbody), __instance);
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(OWRigidbody.ChangeSuspensionBody))]
		private static bool ChangeSuspensionBody(OWRigidbody __instance, OWRigidbody newSuspensionBody)
		{
			if (__instance._suspended)
			{
				__instance._cachedRelativeVelocity = Vector3.zero;
				__instance._suspensionBody = newSuspensionBody;
				if (_setParentQueue.ContainsKey(__instance))
				{
					_setParentQueue[__instance] = newSuspensionBody.transform;
				}
				else
				{
					__instance._transform.parent = newSuspensionBody.transform;
				}
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(OWRigidbody.UnsuspendImmediate))]
		private static bool UnsuspendImmediate(OWRigidbody __instance, bool restoreCachedVelocity)
		{
			if (__instance._suspended)
			{
				if (__instance.RunningKinematicSimulation())
				{
					__instance._kinematicRigidbody.enabled = true;
				}
				else
				{
					__instance.MakeNonKinematic();
				}

				__instance.enabled = true;
				if (_setParentQueue.ContainsKey(__instance))
				{
					_setParentQueue[__instance] = null;
				}
				else
				{
					__instance._transform.parent = null;
				}

				if (!Physics.autoSyncTransforms)
				{
					Physics.SyncTransforms();
				}

				var cachedVelocity = restoreCachedVelocity ? __instance._suspensionBody.transform.TransformDirection(__instance._cachedRelativeVelocity) : Vector3.zero;
				__instance.SetVelocity(__instance._suspensionBody.GetPointVelocity(__instance._transform.position) + cachedVelocity);
				__instance.SetAngularVelocity(restoreCachedVelocity ? __instance._cachedAngularVelocity : Vector3.zero);
				__instance._suspended = false;
				__instance._suspensionBody = null;
				__instance.RaiseEvent(nameof(__instance.OnUnsuspendOWRigidbody), __instance);
			}

			return false;
		}
	}

	/// <summary>
	/// only call this before world objects added
	/// </summary>
	public static string DeterministicPath(this Component component)
	{
		var sb = new StringBuilder();
		var transform = component.transform;
		while (true)
		{
			if (!_cache.TryGetValue(transform, out var data))
			{
				data = (transform.GetSiblingIndex(), transform.parent);
				_cache.Add(transform, data);
			}

			if (!data.Parent)
			{
				break;
			}

			sb.Append(transform.name);
			sb.Append(' ');
			sb.Append(data.SiblingIndex);
			sb.Append(' ');
			transform = data.Parent;
		}

		sb.Append(transform.name);
		return sb.ToString();
	}

	/// <summary>
	/// only call this before world objects added
	/// </summary>
	public static IEnumerable<T> SortDeterministic<T>(this IEnumerable<T> components) where T : Component
		=> components.OrderBy(DeterministicPath);
}