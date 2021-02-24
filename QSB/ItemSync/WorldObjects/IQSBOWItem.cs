using QSB.WorldSync;

namespace QSB.ItemSync.WorldObjects
{
	public interface IQSBOWItem : IWorldObjectTypeSubset
	{
		uint HoldingPlayer { get; set; }
	}
}
