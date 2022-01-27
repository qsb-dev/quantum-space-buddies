using HarmonyLib;
using QSB.WorldSync;
using System.Collections.Generic;
using System.IO;
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
		public static void ClearCache()
		{
			using (var file = File.CreateText(Path.Combine(QSBCore.Helper.Manifest.ModFolderPath, "world objects.csv")))
			{
				file.WriteLine("world object,deterministic path,instance id");
				foreach (var worldObject in QSBWorldSync.GetWorldObjects())
				{
					file.Write(worldObject.GetType().Name);
					file.Write(',');
					file.Write('"');
					file.Write(worldObject.AttachedObject.DeterministicPath().Replace("\"", "\"\""));
					file.Write('"');
					file.Write(',');
					file.Write(worldObject.AttachedObject.GetInstanceID());
					file.WriteLine();
				}
			}

			using (var file = File.CreateText(Path.Combine(QSBCore.Helper.Manifest.ModFolderPath, "cache.csv")))
			{
				file.WriteLine("name,sibling index,parent");
				foreach (var (transform, (siblingIndex, parent)) in _cache)
				{
					file.Write('"');
					file.Write(transform.name.Replace("\"", "\"\""));
					file.Write('"');
					file.Write(',');
					file.Write(siblingIndex);
					file.Write(',');
					file.Write('"');
					file.Write(parent ? parent.name.Replace("\"", "\"\"") : "<no parent>");
					file.Write('"');
					file.WriteLine();
				}
			}

			DebugLog.DebugWrite($"cleared cache of {_cache.Count} entries");
			_cache.Clear();
		}

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
		/// only call this before all objects ready
		/// </summary>
		public static IEnumerable<T> SortDeterministic<T>(this IEnumerable<T> components) where T : Component
			=> components.OrderBy(DeterministicPath);
	}
}
