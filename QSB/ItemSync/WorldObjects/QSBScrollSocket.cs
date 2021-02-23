using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.ItemSync.WorldObjects
{
	class QSBScrollSocket : QSBOWItemSocket<ScrollSocket>
	{
		public override void Init(ScrollSocket attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			base.Init(attachedObject, id);
		}
	}
}
