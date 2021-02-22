using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSB.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QSBTests
{
	[TestClass]
	public class EventTests
	{
		[TestMethod]
		public void CheckUnreferencedEvents()
		{
			var qsbAssembly = Assembly.Load("QSB");
			var allEventTypes = qsbAssembly
				.GetTypes()
				.Where(x => typeof(IQSBEvent).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

			EventManager.Init();
			var eventInstances = (List<IQSBEvent>)typeof(EventManager)
				.GetField("_eventList", BindingFlags.NonPublic | BindingFlags.Static)
				.GetValue(typeof(EventManager));

			var failedTypes = new List<Type>();
			foreach (var type in allEventTypes)
			{
				if (!eventInstances.Any(x => x.GetType() == type))
				{
					failedTypes.Add(type);
				}
			}

			if (failedTypes.Count > 0)
			{
				Assert.Fail(string.Join(", ", failedTypes.Select(x => x.Name)));
			}
		}
	}
}
