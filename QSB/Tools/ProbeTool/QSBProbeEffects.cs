using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.Tools.ProbeTool;

internal class QSBProbeEffects : MonoBehaviour
{
	public OWAudioSource _flightLoopAudio;
	public OWAudioSource _anchorAudio;
	public ParticleSystem _anchorParticles;

	private QSBSurveyorProbe _probe;

	private void Awake()
	{
		_probe = QSBWorldSync.GetUnityObjects<QSBSurveyorProbe>().First(x => gameObject.transform.IsChildOf(x.transform));
		if (_probe == null)
		{
			DebugLog.ToConsole($"Error - Couldn't find QSBProbe!", OWML.Common.MessageType.Error);
		}

		_probe.OnLaunchProbe += OnLaunch;
		_probe.OnAnchorProbe += OnAnchor;
		_probe.OnUnanchorProbe += OnUnanchor;
		_probe.OnStartRetrieveProbe += OnStartRetrieve;
		_probe.OnTakeSnapshot += OnTakeSnapshot;
	}

	private void OnDestroy()
	{
		_probe.OnLaunchProbe -= OnLaunch;
		_probe.OnAnchorProbe -= OnAnchor;
		_probe.OnUnanchorProbe -= OnUnanchor;
		_probe.OnStartRetrieveProbe -= OnStartRetrieve;
		_probe.OnTakeSnapshot -= OnTakeSnapshot;
	}

	private void OnLaunch() => _flightLoopAudio.FadeIn(0.1f, true, true);

	private void OnAnchor()
	{
		// TODO : Come up with some other way of doing this

		//if (this._fluidDetector.InFluidType(FluidVolume.Type.WATER))
		//{
		//	this._underwaterAnchorParticles.Play();
		//}
		//else
		//{
		_anchorParticles.Play();
		//}
		_flightLoopAudio.FadeOut(0.5f);
		_anchorAudio.PlayOneShot(AudioType.ToolProbeAttach);
	}

	private void OnUnanchor()
		=> _flightLoopAudio.FadeIn(0.5f);

	private void OnStartRetrieve(float retrieveLength)
	{
		_flightLoopAudio.FadeOut(retrieveLength);
		_anchorAudio.PlayOneShot(AudioType.ToolProbeRetrieve);
	}

	private void OnTakeSnapshot()
		=> _anchorAudio.PlayOneShot(AudioType.ToolProbeTakePhoto);
}