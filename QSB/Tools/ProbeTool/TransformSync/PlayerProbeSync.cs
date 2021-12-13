using OWML.Common;
using OWML.Utils;
using QSB.SectorSync;
using QSB.Syncs;
using QSB.Syncs.Sectored.Transforms;
using QSB.Tools.ProbeLauncherTool;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Tools.ProbeTool.TransformSync
{
	public class PlayerProbeSync : SectoredTransformSync
	{
		protected override float DistanceLeeway => 10f;
		public override bool UseInterpolation => true;
		public override bool IgnoreDisabledAttachedObject => true;
		public override bool IsPlayerObject => true;

		public static PlayerProbeSync LocalInstance { get; private set; }

		public override void OnStartAuthority() => LocalInstance = this;

		protected override Transform InitLocalTransform()
		{
			QSBCore.UnityEvents.RunWhen(() => WorldObjectManager.AllObjectsReady, () => SectorSync.Init(Locator.GetProbe().GetSectorDetector(), TargetType.Probe));

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

		protected override Transform InitRemoteTransform()
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

			ProbeCreator.CreateProbe(body, Player);

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

					if (ReferenceTransform != null)
					{
						transform.position = ReferenceTransform.EncodePos(AttachedObject.transform.position);
						transform.rotation = ReferenceTransform.EncodeRot(AttachedObject.transform.rotation);
					}
					else
					{
						transform.position = Vector3.zero;
						transform.rotation = Quaternion.identity;
					}

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
