using HarmonyLib;
using System;

namespace EpicRerouter.ModSide
{
	[HarmonyPatch(typeof(EpicPlatformManager))]
	public static class Patches
	{
		public static void Apply()
		{
			try
			{
				Harmony.CreateAndPatchAll(typeof(Patches));
			}
			catch (Exception e)
			{
				Interop.Log(e);
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch("instance", MethodType.Getter)]
		private static bool GetInstance()
		{
			Interop.Log("instance get called");
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch("instance", MethodType.Setter)]
		private static bool SetInstance()
		{
			Interop.Log("instance set called");
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch("platformInterface", MethodType.Getter)]
		private static bool GetPlatformInterface()
		{
			Interop.Log("platformInterface get called");
			return false;
		}
	}
}
