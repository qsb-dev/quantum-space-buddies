using UnityEngine;

namespace QSB.Tools.ProbeLauncherTool
{
	public class QSBProbeLauncherTool : QSBTool
	{
		public GameObject PreLaunchProbeProxy;
		public ProbeLauncherEffects Effects;
		public SingularityWarpEffect ProbeRetrievalEffect;

		public void RetrieveProbe(bool playEffects)
		{
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

			// TODO : make this do underwater stuff correctly
			Effects.PlayLaunchClip(false);
			// TODO : this plays particles on everyone's launcher...
			Effects.PlayLaunchParticles(false);
		}
	}
}
