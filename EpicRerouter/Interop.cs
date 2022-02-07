using HarmonyLib;
using UnityEngine;

namespace EpicRerouter
{
	public static class Interop
	{
		private static void Log(object msg) => Debug.LogError($"[interop] {msg}");

		public static void Go()
		{
			Log("go");

			Harmony.CreateAndPatchAll(typeof(Patches));
		}
	}
}
