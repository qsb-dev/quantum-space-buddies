using QSB.EchoesOfTheEye.AirlockSync.VariableSync;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.AirlockSync.WorldObjects;

internal class QSBGhostAirlock : VariableSyncedWorldObject<GhostAirlock, AirlockVariableSyncer>
{
	public override void SendInitialState(uint to)
	{

	}
}