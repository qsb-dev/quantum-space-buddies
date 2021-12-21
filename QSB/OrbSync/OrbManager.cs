using QSB.OrbSync.WorldObjects;
using QSB.WorldSync;
using System.Collections.Generic;

namespace QSB.OrbSync
{
	public class OrbManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		public static readonly List<NomaiInterfaceOrb> Orbs = new();

		protected override void RebuildWorldObjects(OWScene scene)
		{
			Orbs.Clear();
			Orbs.AddRange(QSBWorldSync.GetUnityObjects<NomaiInterfaceOrb>());
			QSBWorldSync.Init<QSBOrb, NomaiInterfaceOrb>(Orbs);
		}
	}
}
