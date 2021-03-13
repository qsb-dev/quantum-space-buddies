using QSB.Patches;
using System.Reflection;

namespace QSB.QuantumSync.Patches
{
	public class ClientQuantumPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

		public override void DoPatches()
		{
			QSBCore.HarmonyHelper.AddPrefix<QuantumMoon>("ChangeQuantumState", typeof(ClientQuantumPatches), nameof(ReturnFalsePatch));
			QSBCore.HarmonyHelper.AddPostfix<QuantumMoon>("Start", typeof(ClientQuantumPatches), nameof(Moon_CollapseOnStart));
		}

		public override void DoUnpatches()
		{
			QSBCore.HarmonyHelper.Unpatch<QuantumMoon>("ChangeQuantumState");
			QSBCore.HarmonyHelper.Unpatch<QuantumMoon>("Start");
		}

		public static void Moon_CollapseOnStart(QuantumMoon __instance)
			=> __instance.GetType().GetMethod("SetSurfaceState", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { -1 });

		public static bool ReturnFalsePatch() => false;
	}
}