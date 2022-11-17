using QSB.ItemSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.RaftSync.WorldObjects;

public class QSBRaftDock : WorldObject<RaftDock>, IQSBDropTarget
{
	IItemDropTarget IQSBDropTarget.AttachedObject => AttachedObject;
}
