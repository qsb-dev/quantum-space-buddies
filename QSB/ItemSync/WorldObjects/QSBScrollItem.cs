using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.ItemSync.WorldObjects
{
	class QSBScrollItem : QSBOWItem<ScrollItem>
	{
		public override void Init(ScrollItem attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			base.Init(attachedObject, id);
		}
	}
}
