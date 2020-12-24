using QSB.Patches;

namespace QSB.QuantumSync
{
	public class ClientQuantumStateChangePatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

		public override void DoPatches()
		{
			QSBCore.Helper.HarmonyHelper.AddPrefix<SocketedQuantumObject>("ChangeQuantumState", typeof(ClientQuantumStateChangePatches), nameof(ReturnFalsePatch));
			QSBCore.Helper.HarmonyHelper.AddPrefix<MultiStateQuantumObject>("ChangeQuantumState", typeof(ClientQuantumStateChangePatches), nameof(ReturnFalsePatch));
		}

		public static bool ReturnFalsePatch() => false;
	}
}