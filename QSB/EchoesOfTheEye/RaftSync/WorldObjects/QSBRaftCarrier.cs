using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.RaftSync.WorldObjects;

public abstract class QSBRaftCarrier<T> : WorldObject<T>, IQSBRaftCarrier where T : RaftCarrier
{
	public override void SendInitialState(uint to)
	{
		// todo SendInitialState
	}
}

public interface IQSBRaftCarrier : IWorldObject { }
