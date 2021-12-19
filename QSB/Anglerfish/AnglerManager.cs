using System.Collections.Generic;
using QSB.Anglerfish.WorldObjects;
using QSB.WorldSync;

namespace QSB.Anglerfish
{
	public class AnglerManager : WorldObjectManager
	{
		public static readonly List<AnglerfishController> Anglers = new();

		protected override void RebuildWorldObjects(OWScene scene)
		{
			Anglers.Clear();
			Anglers.AddRange(QSBWorldSync.GetUnityObjects<AnglerfishController>());
			QSBWorldSync.Init<QSBAngler, AnglerfishController>(Anglers);
		}
	}
}
