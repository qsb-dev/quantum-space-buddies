using QSB.Patches;
using System.Reflection;

namespace QSB.QuantumSync.Patches
{
	public class ClientQuantumPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

		public override void DoPatches()
		{
			Prefix(nameof(QuantumMoon_ChangeQuantumState));
			Postfix(nameof(QuantumMoon_Start));
		}

		public static void QuantumMoon_Start(QuantumMoon __instance)
			=> __instance.GetType().GetMethod("SetSurfaceState", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { -1 });

		public static bool QuantumMoon_ChangeQuantumState() 
			=> false;
	}
}