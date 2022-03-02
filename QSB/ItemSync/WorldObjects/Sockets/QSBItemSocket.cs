using QSB.ItemSync.WorldObjects.Items;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ItemSync.WorldObjects.Sockets
{
	internal class QSBItemSocket : WorldObject<OWItemSocket>
	{
		public override void SendInitialState(uint to)
		{
			// todo SendInitialState
		}

		public bool IsSocketOccupied()
			=> AttachedObject.IsSocketOccupied();

		public bool PlaceIntoSocket(IQSBItem item)
		{
			if (!AttachedObject.AcceptsItem((OWItem)item.AttachedObject) || AttachedObject._socketedItem != null)
			{
				DebugLog.ToConsole($"Warning - Tried to place item {item} into socket {this} but couldn't. This socket either doesn't accept this item, or already has an item in it!", OWML.Common.MessageType.Warning);
				return false;
			}

			AttachedObject._socketedItem = (OWItem)item.AttachedObject;
			item.SocketItem(AttachedObject._socketTransform, AttachedObject._sector);
			AttachedObject._socketedItem.PlaySocketAnimation();

			this.RaiseEvent(nameof(AttachedObject.OnSocketablePlaced), AttachedObject._socketedItem);

			AttachedObject.enabled = true;
			return true;
		}

		public void RemoveFromSocket()
			=> AttachedObject.RemoveFromSocket();
	}
}