using HarmonyLib;
using System;

namespace QSB.Patches;

public abstract class QSBPatch
{
	public abstract QSBPatchTypes Type { get; }

	public void DoPatches(Harmony instance) => instance.PatchAll(GetType());

	#region remote calls

	protected static bool Remote { get; private set; }
	protected static object RemoteData { get; private set; }

	public static void RemoteCall(Action call, object data = null)
	{
		RemoteData = data;
		Remote = true;
		call();
		Remote = false;
		RemoteData = null;
	}

	public static T RemoteCall<T>(Func<T> call, object data = null)
	{
		RemoteData = data;
		Remote = true;
		var t = call();
		Remote = false;
		RemoteData = null;
		return t;
	}

	#endregion
}
