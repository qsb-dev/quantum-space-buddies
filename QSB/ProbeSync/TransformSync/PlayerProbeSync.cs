using OWML.Common;
using OWML.Utils;
using QSB.Player;
using QSB.SectorSync;
using QSB.Syncs.TransformSync;
using QSB.Tools;
using QSB.Utility;
using UnityEngine;

namespace QSB.ProbeSync.TransformSync
{
	public class PlayerProbeSync : SectoredTransformSync
	{
		public static PlayerProbeSync LocalInstance { get; private set; }

		public override void OnStartAuthority()
		{
			DebugLog.DebugWrite($"OnStartAuthority probe");
			LocalInstance = this;
		}

		protected override Component InitLocalTransform()
		{
			SectorSync.Init(Locator.GetProbe().GetSectorDetector(), this);

			var body = Locator.GetProbe().transform;
			Player.ProbeBody = body.gameObject;

			if (Player.Body == null)
			{
				DebugLog.ToConsole($"Warning - Player.Body is null!", MessageType.Warning);
				return null;
			}

			var listener = Player.Body.AddComponent<ProbeListener>();
			listener.Init(Locator.GetProbe());

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

			var body = probe.InstantiateInactive();
			body.name = "RemoteProbeTransform";

			PlayerToolsManager.CreateProbe(body, Player);

			Player.ProbeBody = body.gameObject;

			return body;
		}

		// TODO : maybe just add a field like "useinterpolation" for still updating if the attachedobject is disabled
		public override void Update()
		{
			if (!_isInitialized && IsReady)
			{
				Init();
			}
			else if (_isInitialized && !IsReady)
			{
				_isInitialized = false;
				return;
			}

			if (!_isInitialized)
			{
				return;
			}

			if (AttachedObject == null)
			{
				DebugLog.ToConsole($"Warning - AttachedObject {_logName} is null.", MessageType.Warning);
				_isInitialized = false;
				return;
			}

			if (ReferenceTransform != null && ReferenceTransform.position == Vector3.zero)
			{
				DebugLog.ToConsole($"Warning - {_logName}'s ReferenceTransform is at (0,0,0). ReferenceTransform:{ReferenceTransform.name}, AttachedObject:{AttachedObject.name}", MessageType.Warning);
			}

			if (ReferenceTransform == null)
			{
				DebugLog.ToConsole($"Warning - {_logName}'s ReferenceTransform is null. AttachedObject:{AttachedObject.name}", MessageType.Warning);
				return;
			}

			UpdateTransform();
		}

		protected override void UpdateTransform()
		{
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

					return;
				}
			}

			base.UpdateTransform();
		}

		public override bool IsReady => Locator.GetProbe() != null
			&& Player != null
			&& QSBPlayerManager.PlayerExists(Player.PlayerId)
			&& Player.PlayerStates.IsReady
			&& NetId.Value != uint.MaxValue
			&& NetId.Value != 0U;

		protected override float DistanceLeeway => 10f;

		public override bool UseInterpolation => true;

		public override TargetType Type => TargetType.Probe;
	}
}