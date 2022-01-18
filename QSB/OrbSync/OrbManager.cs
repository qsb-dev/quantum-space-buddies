using QSB.OrbSync.WorldObjects;
using QSB.WorldSync;
using System.Collections.Generic;

namespace QSB.OrbSync
{
	public class OrbManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		public static readonly List<NomaiInterfaceOrb> Orbs = new();

		public override void BuildWorldObjects(OWScene scene)
		{
			Orbs.Clear();
			Orbs.AddRange(QSBWorldSync.GetUnityObjects<NomaiInterfaceOrb>());
			QSBWorldSync.Init<QSBOrb, NomaiInterfaceOrb>(Orbs);
		}
	}
}
