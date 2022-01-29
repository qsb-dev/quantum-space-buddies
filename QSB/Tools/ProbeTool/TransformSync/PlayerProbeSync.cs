using OWML.Common;
using QSB.SectorSync;
using QSB.Syncs.Sectored.Transforms;
using QSB.Tools.ProbeLauncherTool;
using QSB.Utility;
using UnityEngine;

namespace QSB.Tools.ProbeTool.TransformSync
{
	public class PlayerProbeSync : SectoredTransformSync
	{
		protected override float DistanceLeeway => 10f;
		protected override bool UseInterpolation => true;
		protected override bool AllowInactiveAttachedObject => true;
		protected override bool IsPlayerObject => true;

		public static PlayerProbeSync LocalInstance { get; private set; }

		public override void OnStartAuthority() => LocalInstance = this;

		protected override Transform InitLocalTransform()
		{
			SectorDetector.Init(Locator.GetProbe().GetSectorDetector(), TargetType.Probe);

			var body = Locator.GetProbe().transform;
			Player.ProbeBody = body.gameObject;

			if (!Player.Body)
			{
				DebugLog.ToConsole($"Warning - Player.Body is null!", MessageType.Warning);
				return null;
			}

			var listener = Player.Body.AddComponent<ProbeListener>();
			listener.Init(Locator.GetProbe());

			var launcherListener = Player.Body.AddComponent<ProbeLauncherListener>();
			launcherListener.Init(Player.LocalProbeLauncher);

			return body;
		}

		protected override Transform InitRemoteTransform()
		{
			var probe = Locator.GetProbe().transform;

			if (!probe)
			{
				DebugLog.ToConsole("Error - Probe is null!", MessageType.Error);
				return default;
			}

			var body = probe.gameObject.activeSelf
				? probe.InstantiateInactive()
				: Instantiate(probe);

			body.name = "RemoteProbeTransform";

			ProbeCreator.CreateProbe(body, Player);

			Player.ProbeBody = body.gameObject;

			return body;
		}

		protected override void GetFromAttached()
		{
			if (!AttachedTransform.gameObject.activeInHierarchy)
			{
				var probeBody = Locator.GetProbe().GetOWRigidbody();
				if (!probeBody)
				{
					DebugLog.ToConsole($"Warning - Could not find OWRigidbody of local probe.", MessageType.Warning);
				}

				var probeLauncher = Player.LocalProbeLauncher;
				// TODO : make this sync to the *active* probe launcher's _launcherTransform
				var launcherTransform = probeLauncher._launcherTransform;
				probeBody.SetPosition(launcherTransform.position);
				probeBody.SetRotation(launcherTransform.rotation);

				SetReferenceSector(Player.TransformSync.ReferenceSector);
			}

			base.GetFromAttached();
		}

		protected override bool CheckReady() => base.CheckReady()
			&& (Locator.GetProbe() || AttachedTransform);
	}
}
