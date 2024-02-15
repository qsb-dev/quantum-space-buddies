using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Utility.Deterministic;

/// <summary>
/// holds parenting information used for reliably sorting objects based on path.
///
/// NOTE: sibling indexes MAY be slightly different between vendors because of extra switch optimization gameobjects,
/// but the order is still the same, so sorting is still deterministic 
/// </summary>
public static class DeterministicManager
{
	private static readonly Harmony _harmony = new(typeof(DeterministicManager).FullName);
	private static bool _patched;
	
	public static readonly Dictionary<Transform, (int SiblingIndex, Transform Parent)> ParentCache = new();

	public static void Init()
	{
		QSBSceneManager.OnPreSceneLoad += (_, _) =>
		{
			DebugLog.DebugWrite("cleared deterministic parent cache");
			ParentCache.Clear();
			
			if (!_patched)
			{
				_harmony.PatchAll(typeof(DeterministicRigidbodyPatches));
				_patched = true;
			}
		};
	}
	
	/// <summary>
	/// unpatch DeterministicRigidbodyPatches so rigidbodies added/activated later dont get counted towards the cache.
	/// also breaks with e.g. QuantumInstrument since transform is added to the cache twice (once by body and once by instrument)
	/// </summary>
	public static void OnWorldObjectsAdded()
	{
		if (_patched)
		{
			_harmony.UnpatchSelf();
			_patched = false;
		}
	}

	/// <summary>
	/// only world object managers call this, to do it as early as possible to capture parents before they change
	/// </summary>
	public static string DeterministicPath(this Component component)
	{
		var sb = new StringBuilder();
		var transform = component.transform;
		while (true)
		{
			if (!ParentCache.TryGetValue(transform, out var data))
			{
				data = (transform.GetSiblingIndex(), transform.parent);
				ParentCache.Add(transform, data);
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
	/// only world object managers call this, to do it as early as possible to capture parents before they change
	/// </summary>
	public static IEnumerable<T> SortDeterministic<T>(this IEnumerable<T> components) where T : Component
		=> components.OrderBy(DeterministicPath);
}
