using QSB.Tools.TranslatorTool.TranslationSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.Tools.TranslatorTool.TranslationSync
{
	internal class SpiralManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		protected override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBWallText, NomaiWallText>();
			QSBWorldSync.Init<QSBComputer, NomaiComputer>();
			QSBWorldSync.Init<QSBVesselComputer, NomaiVesselComputer>();
		}
	}
}
