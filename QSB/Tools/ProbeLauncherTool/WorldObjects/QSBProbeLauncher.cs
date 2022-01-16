using QSB.Messaging;
using QSB.Tools.ProbeLauncherTool.Messages;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Tools.ProbeLauncherTool.WorldObjects
{
	internal class QSBProbeLauncher : WorldObject<ProbeLauncher>
	{
		private float _probeRetrievalLength;
		private GameObject _preLaunchProbeProxy;
		private ProbeLauncherEffects _effects;
		private SingularityWarpEffect _probeRetrievalEffect;

		public override void Init()
		{
			_probeRetrievalLength = AttachedObject._probeRetrievalLength;
			_preLaunchProbeProxy = AttachedObject._preLaunchProbeProxy;
			_effects = AttachedObject._effects;
			_probeRetrievalEffect = AttachedObject._probeRetrievalEffect;

			AttachedObject.OnLaunchProbe += OnLaunchProbe;
		}

		public override void OnRemoval() =>
			AttachedObject.OnLaunchProbe -= OnLaunchProbe;

		private void OnLaunchProbe(SurveyorProbe probe) =>
			this.SendMessage(new LaunchProbeMessage());

		public void RetrieveProbe(bool playEffects)
		{
			_preLaunchProbeProxy.SetActive(true);
			if (playEffects)
			{
				_effects.PlayRetrievalClip();
				_probeRetrievalEffect.WarpObjectIn(_probeRetrievalLength);
			}
		}

		public void LaunchProbe()
		{
			_preLaunchProbeProxy.SetActive(false);

			// TODO : make this do underwater stuff correctly
			_effects.PlayLaunchClip(false);
			_effects.PlayLaunchParticles(false);
		}
	}
}
