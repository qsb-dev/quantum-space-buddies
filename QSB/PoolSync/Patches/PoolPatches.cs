using QSB.Patches;

namespace QSB.PoolSync.Patches
{
	internal class PoolPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			Prefix(nameof(NomaiRemoteCameraPlatform_Awake));
			Prefix(nameof(NomaiRemoteCameraPlatform_Update));
			Prefix(nameof(NomaiRemoteCameraPlatform_OnSocketableRemoved));
			Prefix(nameof(NomaiRemoteCameraPlatform_OnSocketableDonePlacing));
			Prefix(nameof(NomaiRemoteCameraPlatform_OnPedestalContact));
			Prefix(nameof(NomaiRemoteCameraStreaming_FixedUpdate));
			Prefix(nameof(NomaiRemoteCameraStreaming_OnSectorOccupantAdded));
			Prefix(nameof(NomaiRemoteCameraStreaming_OnSectorOccupantRemoved));
			Prefix(nameof(NomaiRemoteCameraStreaming_OnEntry));
			Prefix(nameof(NomaiRemoteCameraStreaming_OnExit));
		}

		public static bool NomaiRemoteCameraPlatform_Awake() => false;
		public static bool NomaiRemoteCameraPlatform_Update() => false;
		public static bool NomaiRemoteCameraPlatform_OnSocketableRemoved() => false;
		public static bool NomaiRemoteCameraPlatform_OnSocketableDonePlacing() => false;
		public static bool NomaiRemoteCameraPlatform_OnPedestalContact() => false;
		public static bool NomaiRemoteCameraStreaming_FixedUpdate() => false;
		public static bool NomaiRemoteCameraStreaming_OnSectorOccupantAdded() => false;
		public static bool NomaiRemoteCameraStreaming_OnSectorOccupantRemoved() => false;
		public static bool NomaiRemoteCameraStreaming_OnEntry() => false;
		public static bool NomaiRemoteCameraStreaming_OnExit() => false;
	}
}
