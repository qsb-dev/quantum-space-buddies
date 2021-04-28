using OWML.Common;
using QSB.Player;
using QSB.Tools;
using QSB.TransformSync;
using QSB.Utility;
using UnityEngine;

namespace QSB.ProbeSync.TransformSync
{
	public class PlayerProbeSync : SectoredTransformSync
	{
		public static PlayerProbeSync LocalInstance { get; private set; }

		protected override float DistanceLeeway => 10f;
		public override bool UseInterpolation => true;

		public override void OnStartAuthority()
		{
			DebugLog.DebugWrite($"OnStartAuthority probe");
			LocalInstance = this;
		}

		private Transform GetProbe() =>
			Locator.GetProbe().transform.Find("CameraPivot").Find("Geometry");

		protected override GameObject InitLocalTransform()
		{
			SectorSync.SetSectorDetector(Locator.GetProbe().GetSectorDetector());
			var body = GetProbe();

			Player.ProbeBody = body.gameObject;

			return body.gameObject;
		}

		protected override GameObject InitRemoteTransform()
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

			Player.ProbeBody = body.gameObject;

			return body.gameObject;
		}

		public override bool IsReady => Locator.GetProbe() != null
			&& Player != null
			&& QSBPlayerManager.PlayerExists(Player.PlayerId)
			&& Player.PlayerStates.IsReady
			&& NetId.Value != uint.MaxValue
			&& NetId.Value != 0U;
	}
}