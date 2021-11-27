using QSB.ItemSync.WorldObjects.Items;
using QSB.WorldSync;

namespace QSB.ItemSync.WorldObjects.Sockets
{
	public interface IQSBOWItemSocket : IWorldObjectTypeSubset
	{
		bool AcceptsItem(IQSBOWItem item);
		bool IsSocketOccupied();
		bool PlaceIntoSocket(IQSBOWItem item);
		IQSBOWItem RemoveFromSocket();
	}
}
