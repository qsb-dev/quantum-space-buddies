using UnityEngine;

namespace QSB.ItemSync.WorldObjects
{
	internal class QSBScrollItem : QSBOWItem<ScrollItem>
	{
		public override void Init(ScrollItem attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			base.Init(attachedObject, id);
		}
	}
}
