using QSB.Tools.TranslatorTool.TranslationSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.Tools.TranslatorTool.TranslationSync
{
	internal class SpiralManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		public override void BuildWorldObjects(OWScene scene)
		{
			// wait for all late initializers (which includes nomai text) to finish
			StartDelayedReady();
			QSBCore.UnityEvents.RunWhen(() => LateInitializerManager.isDoneInitializing, () =>
			{
				FinishDelayedReady();
				QSBWorldSync.Init<QSBNomaiText, NomaiText>(typeof(GhostWallText));
			});
		}
	}
}
