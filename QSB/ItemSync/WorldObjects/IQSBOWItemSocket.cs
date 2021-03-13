using QSB.WorldSync;

namespace QSB.ItemSync.WorldObjects
{
	public interface IQSBOWItemSocket : IWorldObjectTypeSubset
	{
		bool AcceptsItem(IQSBOWItem item);
		bool PlaceIntoSocket(IQSBOWItem item);
		IQSBOWItem RemoveFromSocket();
	}
}
