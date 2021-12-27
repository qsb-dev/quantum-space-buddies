using QSB.Player;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace QSB.Tools.ProbeTool
{
	internal static class ProbeCreator
	{
		public static void CreateProbe(Transform newProbe, PlayerInfo player)
		{
			var qsbProbe = newProbe.gameObject.AddComponent<QSBProbe>();
			player.Probe = qsbProbe;
			qsbProbe.SetOwner(player);

			// Probe_Body
			Object.Destroy(newProbe.GetComponent<ProbeAnchor>());
			Object.Destroy(newProbe.GetComponent<AlignWithFluid>());
			Object.Destroy(newProbe.GetComponent<MapMarker>());
			Object.Destroy(newProbe.GetComponent<ProbeSeeking>());
			Object.Destroy(newProbe.GetComponent<SurveyorProbe>());
			Object.Destroy(newProbe.GetComponent<ProbeHUDMarker>());
			Object.Destroy(newProbe.GetComponent<LightFlickerController>());
			Object.Destroy(newProbe.GetComponent<OWRigidbody>());
			Object.Destroy(newProbe.GetComponent<Rigidbody>());
			Object.Destroy(newProbe.GetComponent<CenterOfTheUniverseOffsetApplier>());

			// ProbeDetector
			//Object.Destroy(newProbe.Find("ProbeDetector").gameObject);
			var probeDetector = newProbe.Find("ProbeDetector").gameObject;
			probeDetector.tag = "Untagged";
			Object.Destroy(probeDetector.GetComponent<ForceApplier>());
			Object.Destroy(probeDetector.GetComponent<ProbeFluidDetector>());
			Object.Destroy(probeDetector.GetComponent<SectorDetector>());
			Object.Destroy(probeDetector.GetComponent<AlignmentForceDetector>());
			Object.Destroy(probeDetector.GetComponent<OxygenDetector>());
			Object.Destroy(probeDetector.GetComponent<RulesetDetector>());
			Object.Destroy(probeDetector.GetComponent<ProbeDestructionDetector>());
			Object.Destroy(probeDetector.GetComponent<VisionDetector>());

			// CameraPivot
			var cameraPivot = newProbe.Find("CameraPivot");
			Object.Destroy(cameraPivot.GetComponent<ProbeHorizonTracker>());

			// TODO : Sync probe animations

			// CameraPivot/Geometry/Props_HEA_Probe_ANIM
			var animRoot = cameraPivot.Find("Geometry").Find("Props_HEA_Probe_ANIM");
			Object.Destroy(animRoot.GetComponent<ProbeAnimatorController>());
			Object.Destroy(animRoot.GetComponent<Animator>());

			// TODO : Set up QSB cameras for these two cameras - that's why im not just destroying the GOs here

			// CameraPivot/ForwardCamera
			var forwardCamera = cameraPivot.Find("ForwardCamera");
			Object.Destroy(forwardCamera.GetComponent<PostProcessingBehaviour>());
			Object.Destroy(forwardCamera.GetComponent<NoiseImageEffect>());
			Object.Destroy(forwardCamera.GetComponent<PlanetaryFogImageEffect>());
			Object.Destroy(forwardCamera.GetComponent<ProbeCamera>());
			Object.Destroy(forwardCamera.GetComponent<OWCamera>());
			Object.Destroy(forwardCamera.GetComponent<Camera>());
			var oldForwardSpotlight = forwardCamera.GetComponent<ProbeSpotlight>();
			var newForwardSpotlight = forwardCamera.gameObject.AddComponent<QSBProbeSpotlight>();
			newForwardSpotlight._id = oldForwardSpotlight._id;
			newForwardSpotlight._fadeInLength = oldForwardSpotlight._fadeInLength;
			newForwardSpotlight._intensity = 0.8f;
			Object.Destroy(oldForwardSpotlight);

			// CameraPivot/RotatingCameraPivot/RotatingCamera
			var rotatingCamera = cameraPivot.Find("RotatingCameraPivot").Find("RotatingCamera");
			Object.Destroy(rotatingCamera.GetComponent<PostProcessingBehaviour>());
			Object.Destroy(rotatingCamera.GetComponent<NoiseImageEffect>());
			Object.Destroy(rotatingCamera.GetComponent<PlanetaryFogImageEffect>());
			Object.Destroy(rotatingCamera.GetComponent<ProbeCamera>());
			Object.Destroy(rotatingCamera.GetComponent<OWCamera>());
			Object.Destroy(rotatingCamera.GetComponent<Camera>());

			// ProbeEffects
			var probeEffects = newProbe.Find("ProbeEffects");
			var oldEffects = probeEffects.GetComponent<ProbeEffects>();
			var newEffects = probeEffects.gameObject.AddComponent<QSBProbeEffects>();
			newEffects._flightLoopAudio = oldEffects._flightLoopAudio;
			newEffects._anchorAudio = oldEffects._anchorAudio;
			newEffects._anchorParticles = oldEffects._anchorParticles;
			Object.Destroy(oldEffects);

			Object.Destroy(probeEffects.Find("CloudsEffectBubble").gameObject);
			Object.Destroy(probeEffects.Find("SandEffectBubble").gameObject);
			Object.Destroy(probeEffects.Find("ProbeElectricityEffect").gameObject);

			// AmbientLight_Probe
			var ambientLight = newProbe.Find("AmbientLight_Probe");
			var oldAmbLantern = ambientLight.GetComponent<ProbeLantern>();
			var newAmbLantern = ambientLight.gameObject.AddComponent<QSBProbeLantern>();
			newAmbLantern._fadeInDuration = oldAmbLantern._fadeInDuration;
			newAmbLantern._fadeInCurve = oldAmbLantern._fadeInCurve;
			newAmbLantern._fadeOutCurve = oldAmbLantern._fadeOutCurve;
			newAmbLantern._emissiveRenderer = oldAmbLantern._emissiveRenderer;
			newAmbLantern._originalRange = 60f;
			Object.Destroy(oldAmbLantern);

			// Lantern
			var lantern = newProbe.Find("Lantern");
			var oldLantern = lantern.GetComponent<ProbeLantern>();
			var newLantern = lantern.gameObject.AddComponent<QSBProbeLantern>();
			newLantern._fadeInDuration = oldLantern._fadeInDuration;
			newLantern._fadeInCurve = oldLantern._fadeInCurve;
			newLantern._fadeOutCurve = oldLantern._fadeOutCurve;
			newLantern._emissiveRenderer = oldLantern._emissiveRenderer;
			newLantern._originalRange = 35f;
			Object.Destroy(oldLantern);

			// RearCamera
			var rearCamera = newProbe.Find("RearCamera");
			Object.Destroy(rearCamera.GetComponent<PostProcessingBehaviour>());
			Object.Destroy(rearCamera.GetComponent<NoiseImageEffect>());
			Object.Destroy(rearCamera.GetComponent<PlanetaryFogImageEffect>());
			Object.Destroy(rearCamera.GetComponent<ProbeCamera>());
			Object.Destroy(rearCamera.GetComponent<OWCamera>());
			Object.Destroy(rearCamera.GetComponent<Camera>());
			var oldRearSpotlight = rearCamera.GetComponent<ProbeSpotlight>();
			var newRearSpotlight = rearCamera.gameObject.AddComponent<QSBProbeSpotlight>();
			newRearSpotlight._id = oldRearSpotlight._id;
			newRearSpotlight._fadeInLength = oldRearSpotlight._fadeInLength;
			newRearSpotlight._intensity = 0.8f;
			Object.Destroy(oldRearSpotlight);

			// PlaneOffsetMarker_Probe
			Object.Destroy(newProbe.Find("PlaneOffsetMarker_Probe").gameObject);

			newProbe.Find("RecallEffect").gameObject.GetComponent<SingularityController>().enabled = true;
			newProbe.Find("RecallEffect").gameObject.GetComponent<SingularityWarpEffect>().enabled = true;
			newProbe.Find("RecallEffect").name = "RemoteProbeRecallEffect";

			newProbe.gameObject.SetActive(true);
		}
	}
}
