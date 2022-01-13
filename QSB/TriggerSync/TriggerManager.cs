using QSB.WorldSync;
using System.Linq;

namespace QSB.TriggerSync
{
	public class TriggerManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		protected override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBCharacterTrigger, OWTriggerVolume>(
				QSBWorldSync.GetUnityObjects<CharacterAnimController>()
					.Where(x => x.playerTrackingZone)
					.Select(x => x.playerTrackingZone)
			);

			QSBWorldSync.Init<QSBSolanumTrigger, OWTriggerVolume>(
				QSBWorldSync.GetUnityObjects<NomaiConversationManager>()
					.Select(x => x._watchPlayerVolume)
			);

			QSBWorldSync.Init<QSBVesselCageTrigger, OWTriggerVolume>(
				QSBWorldSync.GetUnityObjects<VesselWarpController>()
					.Select(x => x._cageTrigger)
			);

			QSBWorldSync.Init<QSBMaskZoneTrigger, OWTriggerVolume>(
				QSBWorldSync.GetUnityObjects<MaskZoneController>()
					.Select(x => x._maskZoneTrigger)
			);
		}
	}
}
