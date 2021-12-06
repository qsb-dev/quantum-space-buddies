using QSB.Events;
using QSB.TornadoSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.TornadoSync
{
	public class TornadoManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene) =>
			QSBWorldSync.Init<QSBTornado, TornadoController>();

		public static void FireResync()
		{
			QSBWorldSync.GetWorldObjects<QSBTornado>().ForEach(tornado
				=> QSBEventManager.FireEvent(EventNames.QSBTornadoFormState, tornado));

			var gdBody = Locator._giantsDeep.GetOWRigidbody();
			// cannon
			var cannon = Locator._orbitalProbeCannon.GetRequiredComponent<OrbitalProbeLaunchController>();
			QSBEventManager.FireEvent(EventNames.QSBBodyResync, cannon.GetAttachedOWRigidbody(), gdBody);
			foreach (var fake in cannon._fakeDebrisBodies)
			{
				if (fake)
				{
					QSBEventManager.FireEvent(EventNames.QSBBodyResync,
						fake.GetAttachedOWRigidbody(), gdBody);
				}
			}
			foreach (var real in cannon._realDebrisSectorProxies)
			{
				QSBEventManager.FireEvent(EventNames.QSBBodyResync,
					real.transform.root.GetAttachedOWRigidbody(), gdBody);
			}
			QSBEventManager.FireEvent(EventNames.QSBBodyResync, cannon._probeBody, gdBody);

			// islands
			foreach (var island in QSBWorldSync.GetUnityObjects<IslandController>())
			{
				QSBEventManager.FireEvent(EventNames.QSBBodyResync, island._islandBody, gdBody);
			}
		}
	}
}
