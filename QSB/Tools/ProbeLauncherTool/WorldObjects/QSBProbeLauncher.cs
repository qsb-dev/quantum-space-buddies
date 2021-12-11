using OWML.Utils;
using QSB.Events;
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
			_probeRetrievalLength = AttachedObject.GetValue<float>("_probeRetrievalLength");
			_preLaunchProbeProxy = AttachedObject.GetValue<GameObject>("_preLaunchProbeProxy");
			_effects = AttachedObject.GetValue<ProbeLauncherEffects>("_effects");
			_probeRetrievalEffect = AttachedObject.GetValue<SingularityWarpEffect>("_probeRetrievalEffect");

			AttachedObject.OnLaunchProbe += (SurveyorProbe probe) => QSBEventManager.FireEvent(EventNames.QSBLaunchProbe, this);
		}

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
