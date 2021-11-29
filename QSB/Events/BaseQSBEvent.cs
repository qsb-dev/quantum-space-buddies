using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.Events
{
	public abstract class BaseQSBEvent : IQSBEvent
	{
		protected static int _msgType = 0;

		public abstract void SetupListener();
		public abstract void CloseListener();
	}
}
