using OWML.Utils;
using QSB.ProbeSync;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Tools.ProbeLauncherTool.WorldObjects
{
	class QSBProbeLauncher : WorldObject<ProbeLauncher>
	{
		private float _probeRetrievalLength;
		private GameObject _preLaunchProbeProxy;
		private ProbeLauncherEffects _effects;
		private SingularityWarpEffect _probeRetrievalEffect;

		public override void Init(ProbeLauncher launcher, int id)
		{
			ObjectId = id;
			AttachedObject = launcher;
			_probeRetrievalLength = AttachedObject.GetValue<float>("_probeRetrievalLength");
			_preLaunchProbeProxy = AttachedObject.GetValue<GameObject>("_preLaunchProbeProxy");
			_effects = AttachedObject.GetValue<ProbeLauncherEffects>("_effects");
			_probeRetrievalEffect = AttachedObject.GetValue<SingularityWarpEffect>("_probeRetrievalEffect");
		}

		public void RetrieveProbe(bool playEffects)
		{
			DebugLog.DebugWrite($"{ObjectId} ({AttachedObject.name}) RETRIEVE");

			_preLaunchProbeProxy.SetActive(true);
			if (playEffects)
			{
				_effects.PlayRetrievalClip();
				_probeRetrievalEffect.WarpObjectIn(_probeRetrievalLength);
			}
		}
	}
}
