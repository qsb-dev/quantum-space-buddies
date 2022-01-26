using QSB.Messaging;
using QSB.Tools.ProbeLauncherTool.Messages;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Tools.ProbeLauncherTool.WorldObjects
{
	internal class QSBProbeLauncher : WorldObject<ProbeLauncher>
	{
		public override void Init() =>
			AttachedObject.OnLaunchProbe += OnLaunchProbe;

		public override void OnRemoval() =>
			AttachedObject.OnLaunchProbe -= OnLaunchProbe;

		public override void SendInitialState(uint to)
		{
			if (QSBCore.IsHost)
			{
				if (AttachedObject._preLaunchProbeProxy.activeSelf)
				{
					this.SendMessage(new RetrieveProbeMessage(false));
				}
				else
				{
					this.SendMessage(new LaunchProbeMessage());
				}
			}
		}

		private void OnLaunchProbe(SurveyorProbe probe) =>
			this.SendMessage(new LaunchProbeMessage());

		public void RetrieveProbe(bool playEffects)
		{
			if (AttachedObject._preLaunchProbeProxy.activeSelf)
			{
				return;
			}

			AttachedObject._preLaunchProbeProxy.SetActive(true);
			if (playEffects)
			{
				AttachedObject._effects.PlayRetrievalClip();
				AttachedObject._probeRetrievalEffect.WarpObjectIn(AttachedObject._probeRetrievalLength);
			}
		}

		public void LaunchProbe()
		{
			if (!AttachedObject._preLaunchProbeProxy.activeSelf)
			{
				return;
			}

			AttachedObject._preLaunchProbeProxy.SetActive(false);

			// TODO : make this do underwater stuff correctly
			AttachedObject._effects.PlayLaunchClip(false);
			AttachedObject._effects.PlayLaunchParticles(false);
		}
	}
}
