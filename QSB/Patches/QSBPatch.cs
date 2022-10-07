using HarmonyLib;

namespace QSB.Patches;

public abstract class QSBPatch
{
	public abstract QSBPatchTypes Type { get; }

	public virtual GameVendor PatchVendor => GameVendor.Epic | GameVendor.Steam | GameVendor.Gamepass;

	public void DoPatches(Harmony instance) => instance.PatchAll(GetType());

	/// <summary>
	/// this is true when a message is received remotely (OnReceiveRemote) or a player leaves (OnRemovePlayer)
	/// </summary>
	public static bool Remote;
}
