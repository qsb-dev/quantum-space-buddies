using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.RaftSync.WorldObjects;

public class QSBRaftCarrier<T> : WorldObject<T>, IQSBRaftCarrier
	where T : RaftCarrier
{
}
