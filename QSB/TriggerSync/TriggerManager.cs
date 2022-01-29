using Cysharp.Threading.Tasks;
using QSB.TriggerSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.TriggerSync
{
	public class TriggerManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
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
