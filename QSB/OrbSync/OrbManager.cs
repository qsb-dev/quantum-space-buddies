using OWML.Common;
using QSB.OrbSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;

namespace QSB.OrbSync
{
	public class OrbManager : WorldObjectManager
	{
		public static readonly List<NomaiInterfaceOrb> Orbs = new();

		protected override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBOrbSlot, NomaiInterfaceSlot>();
			DebugLog.DebugWrite($"Finished slot build with {QSBWorldSync.GetWorldObjects<QSBOrbSlot>().Count()} slots.", MessageType.Success);

			Orbs.Clear();
			Orbs.AddRange(QSBWorldSync.GetUnityObjects<NomaiInterfaceOrb>());
			QSBWorldSync.Init<QSBOrb, NomaiInterfaceOrb>();
			DebugLog.DebugWrite($"Finished orb build with {Orbs.Count} orbs.", MessageType.Success);
		}
	}
}
