using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.SatelliteSync;

public class SatelliteProjectorManager : MonoBehaviour, IAddComponentOnStart
{
	public static SatelliteProjectorManager Instance { get; private set; }

	public SatelliteSnapshotController Projector { get; private set; }
	public RenderTexture SatelliteCameraSnapshot
	{
		get
		{
			if (_satelliteCameraSnapshot == null)
			{
				_satelliteCameraSnapshot = new RenderTexture(512, 512, 16)
				{
					name = "SatelliteCameraSnapshot",
					hideFlags = HideFlags.HideAndDontSave
				};
				_satelliteCameraSnapshot.Create();
			}

			return _satelliteCameraSnapshot;
		}
	}

	private static RenderTexture _satelliteCameraSnapshot;

	private void Awake()
	{
		Instance = this;
		QSBSceneManager.OnUniverseSceneLoaded += OnSceneLoaded;
		QSBNetworkManager.singleton.OnClientConnected += OnConnected;
	}

	public void OnDestroy() => QSBSceneManager.OnUniverseSceneLoaded -= OnSceneLoaded;

	public void OnConnected()
	{
		if (QSBSceneManager.CurrentScene == OWScene.SolarSystem)
		{
			Projector._snapshotTexture = SatelliteCameraSnapshot;
			Projector._satelliteCamera.targetTexture = Projector._snapshotTexture;
		}
	}

	private void OnSceneLoaded(OWScene oldScene, OWScene newScene)
	{
		if (newScene == OWScene.SolarSystem)
		{
			Projector = QSBWorldSync.GetUnityObject<SatelliteSnapshotController>();
			Projector._loopingSource.spatialBlend = 1f;
			Projector._oneShotSource.spatialBlend = 1f;

			Projector._snapshotTexture = SatelliteCameraSnapshot;
			Projector._satelliteCamera.targetTexture = Projector._snapshotTexture;
		}
	}

	public void RemoteEnter()
	{
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
			Projector._fadeLight.StartFade(0f, 2f);
		}

		var audioClip = Projector._oneShotSource.PlayOneShot(AudioType.TH_ProjectorActivate);
		Projector._loopingSource.FadeIn(audioClip.length);
	}

	public void RemoteExit()
	{
		Projector.enabled = false;
		Projector._interactVolume.EnableInteraction();
		Projector._interactVolume.ResetInteraction();

		if (Projector._showSplashTexture)
		{
			Projector._splashObject.SetActive(true);
			Projector._diagramObject.SetActive(false);
			Projector._projectionScreen.gameObject.SetActive(false);
		}

		if (Projector._fadeLight != null)
		{
			Projector._fadeLight.StartFade(Projector._initLightIntensity, 2f);
		}

		var audioClip = Projector._oneShotSource.PlayOneShot(AudioType.TH_ProjectorStop);
		Projector._loopingSource.FadeOut(audioClip.length);
	}

	public void RemoteTakeSnapshot(bool forward)
	{
		Projector._satelliteCamera.transform.localEulerAngles = forward
			? Projector._initCamLocalRot
			: Projector._initCamLocalRot + new Vector3(0f, 180f, 0f);

		Projector.RenderSnapshot();
	}
}