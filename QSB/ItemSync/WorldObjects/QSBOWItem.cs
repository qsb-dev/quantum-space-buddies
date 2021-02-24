using QSB.WorldSync;

namespace QSB.ItemSync.WorldObjects
{
	internal class QSBOWItem<T> : WorldObject<T>, IQSBOWItem
		where T : OWItem
	{
		public uint HoldingPlayer { get; set; }

		public override void Init(T attachedObject, int id) { }
	}
}
