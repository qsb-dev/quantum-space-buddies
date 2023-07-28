using HarmonyLib;
using Microsoft.Xbox;
using QSB.Patches;
using QSB.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.SaveSync.Patches;

[HarmonyPatch(typeof(Gdk))]
public class GdkPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;
	public override GameVendor PatchVendor => GameVendor.Gamepass;

	[HarmonyPrefix]
	[HarmonyPatch("QueryBlobsCompleted")]
	public static bool QueryBlobsCompleted(int hresult, Dictionary<string, uint> blobs)
	{
		if (!Gdk.Succeeded(hresult, "Query blobs"))
		{
			DebugLog.DebugWrite("[GDK] Query blobs failed!");
			return false;
		}

		Debug.Log(string.Format("[GDK] Save system setup complete. Blobs returned: {0}", blobs.Count));
		foreach (var keyValuePair in blobs)
		{
			DebugLog.DebugWrite(keyValuePair.Key);
		}

		QSBMSStoreProfileManager.SharedInstance.InvokeProfileSignInComplete();
		QSBMSStoreProfileManager.SharedInstance.InvokeSaveSetupComplete();
		SpinnerUI.Hide();
		return false;
	}
}
