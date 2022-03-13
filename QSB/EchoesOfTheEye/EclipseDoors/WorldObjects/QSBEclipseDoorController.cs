using QSB.EchoesOfTheEye.EclipseDoors.VariableSync;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.EclipseDoors.WorldObjects;

internal class QSBEclipseDoorController : VariableSyncedWorldObject<EclipseDoorController, EclipseDoorVariableSyncer>
{
	public override void SendInitialState(uint to)
	{

	}

	public override string ReturnLabel()
		=> $"{base.ReturnLabel()}\r\n- SyncerValue:{Syncer.Value}\r\n- HasAuth:{Syncer.netIdentity.hasAuthority}";
}
