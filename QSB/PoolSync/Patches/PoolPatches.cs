using QSB.Patches;

namespace QSB.PoolSync.Patches
{
	internal class PoolPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			QSBCore.HarmonyHelper.AddPrefix<NomaiRemoteCameraPlatform>("Awake", typeof(PoolPatches), nameof(ReturnFalse));
			QSBCore.HarmonyHelper.AddPrefix<NomaiRemoteCameraPlatform>("Update", typeof(PoolPatches), nameof(ReturnFalse));
			QSBCore.HarmonyHelper.AddPrefix<NomaiRemoteCameraPlatform>("OnSocketableRemoved", typeof(PoolPatches), nameof(ReturnFalse));
			QSBCore.HarmonyHelper.AddPrefix<NomaiRemoteCameraPlatform>("OnSocketableDonePlacing", typeof(PoolPatches), nameof(ReturnFalse));
			QSBCore.HarmonyHelper.AddPrefix<NomaiRemoteCameraPlatform>("OnPedestalContact", typeof(PoolPatches), nameof(ReturnFalse));
			QSBCore.HarmonyHelper.AddPrefix<NomaiRemoteCameraStreaming>("FixedUpdate", typeof(PoolPatches), nameof(ReturnFalse));
			QSBCore.HarmonyHelper.AddPrefix<NomaiRemoteCameraStreaming>("OnSectorOccupantAdded", typeof(PoolPatches), nameof(ReturnFalse));
			QSBCore.HarmonyHelper.AddPrefix<NomaiRemoteCameraStreaming>("OnSectorOccupantRemoved", typeof(PoolPatches), nameof(ReturnFalse));
			QSBCore.HarmonyHelper.AddPrefix<NomaiRemoteCameraStreaming>("OnEntry", typeof(PoolPatches), nameof(ReturnFalse));
			QSBCore.HarmonyHelper.AddPrefix<NomaiRemoteCameraStreaming>("OnExit", typeof(PoolPatches), nameof(ReturnFalse));
		}

		public override void DoUnpatches()
		{
			QSBCore.HarmonyHelper.Unpatch<NomaiRemoteCameraPlatform>("Awake");
			QSBCore.HarmonyHelper.Unpatch<NomaiRemoteCameraPlatform>("Update");
			QSBCore.HarmonyHelper.Unpatch<NomaiRemoteCameraPlatform>("OnSocketableRemoved");
			QSBCore.HarmonyHelper.Unpatch<NomaiRemoteCameraPlatform>("OnSocketableDonePlacing");
			QSBCore.HarmonyHelper.Unpatch<NomaiRemoteCameraPlatform>("OnPedestalContact");
			QSBCore.HarmonyHelper.Unpatch<NomaiRemoteCameraStreaming>("FixedUpdate");
			QSBCore.HarmonyHelper.Unpatch<NomaiRemoteCameraStreaming>("OnSectorOccupantAdded");
			QSBCore.HarmonyHelper.Unpatch<NomaiRemoteCameraStreaming>("OnSectorOccupantRemoved");
			QSBCore.HarmonyHelper.Unpatch<NomaiRemoteCameraStreaming>("OnEntry");
			QSBCore.HarmonyHelper.Unpatch<NomaiRemoteCameraStreaming>("OnExit");
		}

		public static bool ReturnFalse() => false;
	}
}
