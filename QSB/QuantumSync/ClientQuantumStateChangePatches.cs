using QSB.Patches;

namespace QSB.QuantumSync
{
	public class ClientQuantumStateChangePatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

		public override void DoPatches() => QSBCore.Helper.HarmonyHelper.AddPrefix<SocketedQuantumObject>("ChangeQuantumState", typeof(ClientQuantumStateChangePatches), nameof(ChangeQuantumState));

		public static bool ChangeQuantumState() => false;
	}
}
