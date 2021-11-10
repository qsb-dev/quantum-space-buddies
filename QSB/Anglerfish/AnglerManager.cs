using QSB.Anglerfish.WorldObjects;
using QSB.WorldSync;

namespace QSB.Anglerfish
{
	public class AnglerManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBAngler, AnglerfishController>();
	}
}
