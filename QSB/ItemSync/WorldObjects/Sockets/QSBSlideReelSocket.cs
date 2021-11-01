using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.ItemSync.WorldObjects.Sockets
{
	internal class QSBSlideReelSocket : QSBOWItemSocket<SlideReelSocket>
	{
		public override void Init(SlideReelSocket attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			base.Init(attachedObject, id);
		}
	}
}
