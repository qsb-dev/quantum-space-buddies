using OWML.Common;
using QSB.Syncs.Sectored.Transforms;
using QSB.Tools.ProbeLauncherTool;
using QSB.Utility;
using UnityEngine;

namespace QSB.Tools.ProbeTool.TransformSync;

public class PlayerProbeSync : SectoredTransformSync
{
	/// <summary>
	/// normally prints error when attached object is null.
	/// this overrides it so that doesn't happen, since the probe can be destroyed.
	/// </summary>
	protected override bool CheckValid() => AttachedTransform && base.CheckValid();

	protected override float DistanceChangeThreshold => 10f;
	protected override bool UseInterpolation => true;
	protected override bool AllowInactiveAttachedObject => true;
	protected override bool IsPlayerObject => true;

	public static PlayerProbeSync LocalInstance { get; private set; }

	public override void OnStartAuthority() => LocalInstance = this;

	protected override Transform InitLocalTransform()
	{
		SectorDetector.Init(Locator.GetProbe().GetSectorDetector());

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
		=> ProbeCreator.CreateProbe(Player);

	protected override void GetFromAttached()
	{
		if (!AttachedTransform.gameObject.activeInHierarchy)
		{
			var probeBody = Locator.GetProbe().GetOWRigidbody();
			if (!probeBody)
			{
				DebugLog.ToConsole($"Warning - Could not find OWRigidbody of local probe.", MessageType.Warning);
			}

			var probeLauncher = Player.ProbeLauncherEquipped;
			var launcherTransform = probeLauncher == null
				? Player.LocalProbeLauncher._launcherTransform
				: probeLauncher.AttachedObject._launcherTransform;

			probeBody.SetPosition(launcherTransform.position);
			probeBody.SetRotation(launcherTransform.rotation);

			SetReferenceSector(Player.TransformSync.ReferenceSector);
		}

		base.GetFromAttached();
	}

	protected override bool CheckReady() =>
		base.CheckReady() &&
		(Locator.GetProbe() || AttachedTransform);
}