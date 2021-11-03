using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.SatelliteSync
{
	class SatelliteProjectorManager : MonoBehaviour
	{
		public static SatelliteProjectorManager Instance { get; private set; }

		public SatelliteSnapshotController Projector { get; private set; }

		public void Start()
		{
			Instance = this;
			QSBSceneManager.OnUniverseSceneLoaded += OnSceneLoaded;
		}

		public void OnDestroy()
		{
			QSBSceneManager.OnUniverseSceneLoaded -= OnSceneLoaded;
		}

		private void OnSceneLoaded(OWScene oldScene, OWScene newScene)
		{
			if (newScene == OWScene.SolarSystem)
			{
				Projector = Resources.FindObjectsOfTypeAll<SatelliteSnapshotController>().First();
				Projector._loopingSource.spatialBlend = 1f;
				Projector._oneShotSource.spatialBlend = 1f;
			}
		}

		public void RemoteEnter()
		{
			DebugLog.DebugWrite($"Remote Enter");

			Projector.enabled = true;
			Projector._interactVolume.DisableInteraction();

			if (Projector._showSplashTexture)
			{
				Projector._splashObject.SetActive(false);
				Projector._diagramObject.SetActive(true);
				Projector._projectionScreen.gameObject.SetActive(false);
			}

			if (Projector._fadeLight != null)
			{
				Projector._fadeLight.StartFade(0f, 2f, 0f);
			}

			var audioClip = Projector._oneShotSource.PlayOneShot(AudioType.TH_ProjectorActivate, 1f);
			Projector._loopingSource.FadeIn(audioClip.length, false, false, 1f);
		}

		public void RemoteExit()
		{
			DebugLog.DebugWrite($"Remote Exit");

			Projector.enabled = false;
			Projector._interactVolume.ResetInteraction();

			if (Projector._showSplashTexture)
			{
				Projector._splashObject.SetActive(true);
				Projector._diagramObject.SetActive(false);
				Projector._projectionScreen.gameObject.SetActive(false);
			}

			if (Projector._fadeLight != null)
			{
				Projector._fadeLight.StartFade(Projector._initLightIntensity, 2f, 0f);
			}

			var audioClip = Projector._oneShotSource.PlayOneShot(AudioType.TH_ProjectorStop, 1f);
			Projector._loopingSource.FadeOut(audioClip.length, OWAudioSource.FadeOutCompleteAction.STOP, 0f);
		}

		public void RemoteTakeSnapshot(bool forward)
		{
			Projector._satelliteCamera.transform.localEulerAngles = forward
				? Projector._initCamLocalRot
				: Projector._initCamLocalRot + new Vector3(0f, 180f, 0f);

			Projector.RenderSnapshot();
		}
	}
}
