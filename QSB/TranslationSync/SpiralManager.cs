using QSB.TranslationSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.TranslationSync
{
	internal class SpiralManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBWallText, NomaiWallText>();
			QSBWorldSync.Init<QSBComputer, NomaiComputer>();
			QSBWorldSync.Init<QSBVesselComputer, NomaiVesselComputer>();
		}
	}
}
