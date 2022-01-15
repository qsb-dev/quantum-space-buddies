using QSB.TriggerSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.TriggerSync
{
	public class TriggerManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		protected override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBCharacterTrigger, CharacterAnimController>(x => x.playerTrackingZone);
			QSBWorldSync.Init<QSBSolanumTrigger, NomaiConversationManager>(x => x._watchPlayerVolume);
			QSBWorldSync.Init<QSBShrineTrigger, QuantumShrine>(x => x._triggerVolume);
			QSBWorldSync.Init<QSBVesselCageTrigger, VesselWarpController>(x => x._cageTrigger);
			QSBWorldSync.Init<QSBInflationTrigger, CosmicInflationController>(x => x._smokeSphereTrigger);
			QSBWorldSync.Init<QSBMaskZoneTrigger, MaskZoneController>(x => x._maskZoneTrigger);
			QSBWorldSync.Init<QSBEyeShuttleTrigger, EyeShuttleController>(x => x._shuttleVolume);
		}
	}
}
