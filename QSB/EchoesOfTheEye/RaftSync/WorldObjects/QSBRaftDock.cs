using QSB.ItemSync.WorldObjects;

namespace QSB.EchoesOfTheEye.RaftSync.WorldObjects;

public class QSBRaftDock : QSBRaftCarrier<RaftDock>, IQSBDropTarget
{
	IItemDropTarget IQSBDropTarget.AttachedObject => AttachedObject;
}
