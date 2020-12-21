using QSB.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.QuantumSync
{
	public class ServerQuantumPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnServerClientConnect;

		public override void DoPatches() 
		{
			QSBCore.Helper.HarmonyHelper.AddPrefix<ShapeVisibilityTracker>("IsVisibleUsingCameraFrustum", typeof(ServerQuantumPatches), nameof(IsVisibleUsingCameraFrustrum));
		}

		// ShapeVisibilityTracker patches

		public bool IsVisibleUsingCameraFrustrum()
		{
			return false;
		}


	}
}
