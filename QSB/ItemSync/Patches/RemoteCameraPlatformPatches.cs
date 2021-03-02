using QSB.ItemSync.WorldObjects;
using QSB.Patches;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.ItemSync.Patches
{
	class RemoteCameraPlatformPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			QSBCore.Helper.HarmonyHelper.AddPrefix<NomaiRemoteCameraPlatform>("Update", typeof(RemoteCameraPlatformPatches), nameof(Platform_Update));
		}

		public override void DoUnpatches()
		{

		}

		public static bool Platform_Update(NomaiRemoteCameraPlatform __instance)
		{
			var worldObject = QSBWorldSync.GetWorldFromUnity<QSBNomaiRemoteCameraPlatform, NomaiRemoteCameraPlatform>(__instance);
			worldObject.CustomUpdate();
			return false;
		}
	}
}
