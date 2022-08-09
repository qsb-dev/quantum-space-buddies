using HarmonyLib;
using QSB.Utility;
using System;

namespace QSB.Patches;

public abstract class QSBPatch
{
	public abstract QSBPatchTypes Type { get; }

	public virtual GameVendor PatchVendor => GameVendor.Epic | GameVendor.Steam | GameVendor.Gamepass;

	public void DoPatches(Harmony instance) => instance.PatchAll(GetType());

	#region remote calls

	protected static bool Remote { get; private set; }
	protected static object RemoteData { get; private set; }

	public static void RemoteCall(Action call, object data = null)
	{
		Remote = true;
		RemoteData = data;
		nameof(QSBPatch).Try("doing remote call", call);
		Remote = false;
		RemoteData = null;
	}

	public static T RemoteCall<T>(Func<T> call, object data = null)
	{
		Remote = true;
		RemoteData = data;
		var t = default(T);
		nameof(QSBPatch).Try("doing remote call", () => t = call());
		Remote = false;
		RemoteData = null;
		return t;
	}

	#endregion
}
