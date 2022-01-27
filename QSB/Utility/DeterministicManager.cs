using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Utility
{
	[HarmonyPatch]
	public static class DeterministicManager
	{
		public static void Init() =>
			Harmony.CreateAndPatchAll(typeof(DeterministicManager));

		/// <summary>
		/// called after all objects ready
		/// </summary>
		public static void ClearCache() => _cache.Clear();

		private static readonly Dictionary<Transform, (int SiblingIndex, Transform Parent)> _cache = new();

		[HarmonyPrefix]
		[HarmonyPatch(typeof(OWRigidbody), nameof(OWRigidbody.Awake))]
		private static void OWRigidbody_Awake(OWRigidbody __instance)
		{
			var transform = __instance.transform;
			_cache.Add(transform, (transform.GetSiblingIndex(), transform.parent));
		}

		/// <summary>
		/// only call this before all objects ready
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

				sb.Append(data.SiblingIndex);
				transform = data.Parent;
			}

			sb.Append(transform.name);
			return sb.ToString();
		}

		/// <summary>
		/// only call this before all objects ready
		/// </summary>
		public static IEnumerable<T> SortDeterministic<T>(this IEnumerable<T> components) where T : Component
			=> components.OrderBy(DeterministicPath);
	}
}
