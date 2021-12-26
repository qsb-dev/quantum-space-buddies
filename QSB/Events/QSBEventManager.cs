using OWML.Common;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QSB.Events
{
	public static class QSBEventManager
	{
		public static bool Ready { get; private set; }

		private static readonly Type[] _types = typeof(IQSBEvent).GetDerivedTypes().ToArray();
		internal static readonly List<IQSBEvent> _eventList = new();

		public static void Init()
		{
			foreach (var type in _types)
			{
				_eventList.Add((IQSBEvent)Activator.CreateInstance(type));
			}

			if (UnitTestDetector.IsInUnitTest)
			{
				return;
			}

			_eventList.ForEach(ev => ev.SetupListener());

			Ready = true;

			DebugLog.DebugWrite("Event Manager ready.", MessageType.Success);
		}

		public static void Reset()
		{
			Ready = false;
			_eventList.ForEach(ev => ev.CloseListener());
			_eventList.Clear();
		}
	}
}
