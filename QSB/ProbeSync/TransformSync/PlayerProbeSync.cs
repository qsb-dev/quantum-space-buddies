using OWML.Common;
using OWML.Utils;
using QSB.SectorSync;
using QSB.Syncs.Sectored.Transforms;
using QSB.Tools;
using QSB.Tools.ProbeLauncherTool;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ProbeSync.TransformSync
{
	public class PlayerProbeSync : SectoredTransformSync
	{
		protected override float DistanceLeeway => 10f;
		public override bool UseInterpolation => true;
		public override TargetType Type => TargetType.Probe;
		public override bool IgnoreDisabledAttachedObject => true;

		public static PlayerProbeSync LocalInstance { get; private set; }

		public override void OnStartAuthority() => LocalInstance = this;

		protected override Component InitLocalTransform()
		{
			QSBCore.UnityEvents.RunWhen(() => WorldObjectManager.AllReady, () => SectorSync.Init(Locator.GetProbe().GetSectorDetector(), this));

			var body = Locator.GetProbe().transform;
			Player.ProbeBody = body.gameObject;

			if (Player.Body == null)
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

		protected override Component InitRemoteTransform()
		{
			var probe = Locator.GetProbe().transform;

			if (probe == null)
			{
				DebugLog.ToConsole("Error - Probe is null!", MessageType.Error);
				return default;
			}

			var body = probe.gameObject.activeSelf 
				? probe.InstantiateInactive() 
				: Instantiate(probe);

			body.name = "RemoteProbeTransform";

			PlayerToolsManager.CreateProbe(body, Player);

			Player.ProbeBody = body.gameObject;

			return body;
		}

		protected override bool UpdateTransform()
		{
			if (!base.UpdateTransform())
			{
				return false;
			}

			if (HasAuthority)
			{
				if (!AttachedObject.gameObject.activeInHierarchy)
				{
					var probeOWRigidbody = Locator.GetProbe().GetComponent<SurveyorProbe>().GetOWRigidbody();
					if (probeOWRigidbody == null)
					{
						DebugLog.ToConsole($"Warning - Could not find OWRigidbody of local probe.", MessageType.Warning);
					}

					var probeLauncher = Player.LocalProbeLauncher;
					// TODO : make this sync to the *active* probe launcher's _launcherTransform
					var launcherTransform = probeLauncher.GetValue<Transform>("_launcherTransform");
					probeOWRigidbody.SetPosition(launcherTransform.position);
					probeOWRigidbody.SetRotation(launcherTransform.rotation);

					_intermediaryTransform.EncodePosition(AttachedObject.transform.position);
					_intermediaryTransform.EncodeRotation(AttachedObject.transform.rotation);

					var currentReferenceSector = ReferenceSector;
					var playerReferenceSector = Player.TransformSync.ReferenceSector;

					if (currentReferenceSector != playerReferenceSector)
					{
						SetReferenceSector(playerReferenceSector);
					}
				}
			}

			return true;
		}

		public override bool IsReady => Locator.GetProbe() != null;
	}
}