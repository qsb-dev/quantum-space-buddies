using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Utility.Deterministic;

/// <summary>
/// TODO make this only do cache clearing on pre scene load when HOSTING instead of just all the time
/// </summary>
public static class DeterministicManager
{
    private static readonly Harmony _harmony = new(typeof(DeterministicManager).FullName);
    private static bool _patched;

    public static readonly Dictionary<Transform, (int SiblingIndex, Transform Parent)> Cache = new();

    public static void Init() =>
        QSBSceneManager.OnPreSceneLoad += (_, _) =>
        {
            DebugLog.DebugWrite("cleared cache");
            Cache.Clear();

            if (!_patched)
            {
                _harmony.PatchAll(typeof(OWRigidbodyPatches));
                _patched = true;
            }
        };

    public static void OnWorldObjectsAdded()
    {
        //DebugLog.DebugWrite($"cleared cache of {_cache.Count} entries");
        //_cache.Clear();

        if (_patched)
        {
            _harmony.UnpatchSelf();
            _patched = false;
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
            if (!Cache.TryGetValue(transform, out var data))
            {
                data = (transform.GetSiblingIndex(), transform.parent);
                Cache.Add(transform, data);
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