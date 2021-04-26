using QSB.Patches;

namespace QSB.TransformSync.Patches
{
	class TransformSyncPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			//QSBCore.HarmonyHelper.AddPrefix<LoadManager>("LoadSceneAsync", typeof(TransformSyncPatches), nameof(Deparent));
			//QSBCore.HarmonyHelper.AddPrefix<LoadManager>("LoadScene", typeof(TransformSyncPatches), nameof(Deparent));
			//QSBCore.HarmonyHelper.AddPrefix<LoadManager>("ReloadSceneAsync", typeof(TransformSyncPatches), nameof(Deparent));;
		}

		public override void DoUnpatches()
		{
			//QSBCore.HarmonyHelper.Unpatch<LoadManager>("LoadSceneAsync");
			//QSBCore.HarmonyHelper.Unpatch<LoadManager>("LoadScene");
			//QSBCore.HarmonyHelper.Unpatch<LoadManager>("ReloadSceneAsync");
		}

		//public static void Deparent() 
		//	=> QSBNetworkTransform.DeparentAllTransforms();
	}
}
