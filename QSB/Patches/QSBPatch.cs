using QSB.Utility;
using System.Linq;

namespace QSB.Patches
{
	public abstract class QSBPatch
	{
		public abstract QSBPatchTypes Type { get; }

		public virtual void DoPatches()
		{
			var oldMethods = QSBPatchManager.HarmonyInstance.GetPatchedMethods();
			QSBPatchManager.HarmonyInstance.PatchAll(GetType());
			foreach (var method in QSBPatchManager.HarmonyInstance.GetPatchedMethods().Except(oldMethods))
			{
				DebugLog.DebugWrite($"- Patching {method.DeclaringType}.{method.Name}");
			}
		}

		public void DoUnpatches()
		{
		}
	}
}