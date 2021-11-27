using HarmonyLib;

namespace QSB.Patches
{
	public abstract class QSBPatch
	{
		public abstract QSBPatchTypes Type { get; }

		public void DoPatches(Harmony instance) => instance.PatchAll(GetType());
	}
}