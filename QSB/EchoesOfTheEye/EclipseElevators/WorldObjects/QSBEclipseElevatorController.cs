using QSB.EchoesOfTheEye.EclipseElevators.VariableSync;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.EclipseElevators.WorldObjects;

internal class QSBEclipseElevatorController : VariableSyncedWorldObject<EclipseElevatorController, EclipseElevatorVariableSyncer>
{
	public override void SendInitialState(uint to)
	{

	}

	public override string ReturnLabel()
		=> $"{base.ReturnLabel()}\r\n- SyncerValue:{Syncer.Value}\r\n- HasAuth:{Syncer.netIdentity.hasAuthority}";
}
