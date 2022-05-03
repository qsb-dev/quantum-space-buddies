using QSB.WorldSync;

namespace QSB.ItemSync.WorldObjects;

public interface IQSBDropTarget : IWorldObject
{
	new IItemDropTarget AttachedObject { get; }
}
