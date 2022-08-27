using QSB.Utility;
using UnityEngine;

namespace QSB.Tools.ProbeLauncherTool;

[UsedInUnityProject]
public class QSBProbeLauncherTool : QSBTool
{
	public GameObject PreLaunchProbeProxy;
	public ProbeLauncherEffects Effects;
	public SingularityWarpEffect ProbeRetrievalEffect;

	private void VerifyAudioSource()
	{
		if (Effects._owAudioSource == null)
		{
			Effects._owAudioSource = Player.AudioController._repairToolSource;
		}
	}

	public void RetrieveProbe(bool playEffects)
	{
		VerifyAudioSource();

		PreLaunchProbeProxy.SetActive(true);
		if (playEffects)
		{
			Effects.PlayRetrievalClip();
			ProbeRetrievalEffect.WarpObjectIn(0.3f);
		}
	}

	public void LaunchProbe()
	{
		PreLaunchProbeProxy.SetActive(false);

		VerifyAudioSource();

		// TODO : make this do underwater stuff correctly
		Effects.PlayLaunchClip(false);
		Effects.PlayLaunchParticles(false);
	}

	public void ChangeMode()
	{
		VerifyAudioSource();

		Effects.PlayChangeModeClip();
	}

	public void TakeSnapshot()
	{
		VerifyAudioSource();

		// Vanilla method uses the global player audio controller -> bad
		Effects._owAudioSource.PlayOneShot(AudioType.ToolProbeTakePhoto, 1f);

		// Also make the probe itself play the sound effect
		if (Player.Probe.IsLaunched()) Player.Probe.TakeSnapshot();
	}
}