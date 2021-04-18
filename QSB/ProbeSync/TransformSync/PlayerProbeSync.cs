using OWML.Common;
using QSB.Player;
using QSB.Tools;
using QSB.TransformSync;
using QSB.Utility;
using UnityEngine;

namespace QSB.ProbeSync.TransformSync
{
	public class PlayerProbeSync : SyncObjectTransformSync
	{
		private Transform _disabledSocket;

		private Transform GetProbe() =>
			Locator.GetProbe().transform.Find("CameraPivot").Find("Geometry");

		protected override Transform InitLocalTransform()
		{
			SectorSync.SetSectorDetector(Locator.GetProbe().GetSectorDetector());
			var body = GetProbe();

			SetSocket(Player.CameraBody.transform);
			Player.ProbeBody = body.gameObject;

			return body;
		}

		protected override Transform InitRemoteTransform()
		{
			var probe = GetProbe();

			if (probe == null)
			{
				DebugLog.ToConsole("Error - Probe is null!", MessageType.Error);
				return default;
			}

			var body = probe.InstantiateInactive();
			body.name = "RemoteProbeTransform";

			Destroy(body.GetComponentInChildren<ProbeAnimatorController>());

			PlayerToolsManager.CreateProbe(body, Player);

			QSBCore.UnityEvents.RunWhen(
				() => Player.ProbeLauncher != null,
				() => SetSocket(Player.ProbeLauncher.ToolGameObject.transform));
			Player.ProbeBody = body.gameObject;

			return body;
		}

		private void SetSocket(Transform socket)
		{
			DebugLog.DebugWrite($"Set DisabledSocket of id:{PlayerId}.");
			_disabledSocket = socket;
		}

		/*
		protected override void UpdateTransform()
		{
			base.UpdateTransform();
			if (Player == null)
			{
				DebugLog.ToConsole($"Player is null for {AttachedNetId}!", MessageType.Error);
				return;
			}
			if (_disabledSocket == null)
			{
				DebugLog.ToConsole($"DisabledSocket is null for {PlayerId}! (ProbeLauncher null? : {Player.ProbeLauncher == null})", MessageType.Error);
				return;
			}
			if (Player.PlayerStates.ProbeActive || ReferenceSector?.AttachedObject == null)
			{
				return;
			}
			if (HasAuthority)
			{
				transform.position = ReferenceSector.Transform.InverseTransformPoint(_disabledSocket.position);
				return;
			}
			if (SyncedTransform.position == Vector3.zero)
			{
				return;
			}
			SyncedTransform.localPosition = ReferenceSector.Transform.InverseTransformPoint(_disabledSocket.position);
		}
		*/

		public override bool IsReady => Locator.GetProbe() != null
			&& Player != null
			&& QSBPlayerManager.PlayerExists(Player.PlayerId)
			&& Player.PlayerStates.IsReady
			&& NetId.Value != uint.MaxValue
			&& NetId.Value != 0U;
	}
}