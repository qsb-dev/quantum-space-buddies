using OWML.Common;
using OWML.Utils;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player.Messages;
using QSB.PlayerBodySetup.Local;
using QSB.PlayerBodySetup.Remote;
using QSB.Syncs;
using QSB.Syncs.Sectored.Transforms;
using QSB.Utility;
using System.Linq;
using UnityEngine;
using Gizmos = Popcron.Gizmos;

namespace QSB.Player.TransformSync;

[UsedInUnityProject]
public class PlayerTransformSync : SectoredTransformSync
{
	protected override bool IsPlayerObject => true;
	protected override bool AllowInactiveAttachedObject => true;

	private Transform _visibleCameraRoot;
	private Transform _networkCameraRoot => gameObject.transform.GetChild(0);

	private Transform _visibleRoastingSystem;
	private Transform _networkRoastingSystem => gameObject.transform.GetChild(1);

	private Transform _visibleStickPivot;
	private Transform _networkStickPivot => _networkRoastingSystem.GetChild(0).GetChild(0);

	private Transform _visibleStickTip;
	private Transform _networkStickTip => _networkStickPivot.GetChild(0);

	public override void OnStartClient()
	{
		var player = new PlayerInfo(this);
		QSBPlayerManager.PlayerList.SafeAdd(player);

		if (isLocalPlayer)
		{
			LocalInstance = this;
		}

		base.OnStartClient();
		QSBPatch.Remote = !isLocalPlayer;
		QSBPlayerManager.OnAddPlayer?.SafeInvoke(Player);
		QSBPatch.Remote = false;
		DebugLog.DebugWrite($"Create Player : {Player}", MessageType.Info);

		JoinLeaveSingularity.Create(Player, true);
	}

	public override void OnStartLocalPlayer() => LocalInstance = this;

	public override void OnStopClient()
	{
		JoinLeaveSingularity.Create(Player, false);

		// TODO : Maybe move this to a leave event...? Would ensure everything could finish up before removing the player
		QSBPatch.Remote = !isLocalPlayer;
		QSBPlayerManager.OnRemovePlayer?.SafeInvoke(Player);
		QSBPatch.Remote = false;
		base.OnStopClient();
		Player.HudMarker?.Remove();
		Player.MapMarker?.Remove();
		QSBPlayerManager.PlayerList.Remove(Player);
		DebugLog.DebugWrite($"Remove Player : {Player}", MessageType.Info);
	}

	protected override void Uninit()
	{
		base.Uninit();

		if (isLocalPlayer)
		{
			Player.IsReady = false;
			new PlayerReadyMessage(false).Send();
		}

		Player.Reset();
	}

	protected override void Init()
	{
		base.Init();

		var comps = GetComponents<QSBNetworkTransformChild>();
		comps.First(x => x.Target == _networkCameraRoot).AttachedTransform = _visibleCameraRoot;
		comps.First(x => x.Target == _networkRoastingSystem).AttachedTransform = _visibleRoastingSystem;
		comps.First(x => x.Target == _networkStickPivot).AttachedTransform = _visibleStickPivot;
		comps.First(x => x.Target == _networkStickTip).AttachedTransform = _visibleStickTip;
	}

	protected override Transform InitLocalTransform()
		=> LocalPlayerCreation.CreatePlayer(
			Player,
			SectorDetector,
			out _visibleCameraRoot,
			out _visibleRoastingSystem,
			out _visibleStickPivot,
			out _visibleStickTip);

	protected override Transform InitRemoteTransform()
		=> RemotePlayerCreation.CreatePlayer(
			Player,
			out _visibleCameraRoot,
			out _visibleRoastingSystem,
			out _visibleStickPivot,
			out _visibleStickTip);

	protected override void OnRenderObject()
	{
		if (!QSBCore.DrawLines
		    || !IsValid
		    || !ReferenceTransform)
		{
			return;
		}

		base.OnRenderObject();

		Gizmos.Cube(ReferenceTransform.TransformPoint(_networkRoastingSystem.position), ReferenceTransform.TransformRotation(_networkRoastingSystem.rotation), Vector3.one / 4, Color.red);
		Gizmos.Cube(ReferenceTransform.TransformPoint(_networkStickPivot.position), ReferenceTransform.TransformRotation(_networkStickPivot.rotation), Vector3.one / 4, Color.red);
		Gizmos.Cube(ReferenceTransform.TransformPoint(_networkStickTip.position), ReferenceTransform.TransformRotation(_networkStickTip.rotation), Vector3.one / 4, Color.red);
		Gizmos.Cube(ReferenceTransform.TransformPoint(_networkCameraRoot.position), ReferenceTransform.TransformRotation(_networkCameraRoot.rotation), Vector3.one / 4, Color.red);

		Gizmos.Cube(_visibleRoastingSystem.position, _visibleRoastingSystem.rotation, Vector3.one / 4, Color.magenta);
		Gizmos.Cube(_visibleStickPivot.position, _visibleStickPivot.rotation, Vector3.one / 4, Color.blue);
		Gizmos.Cube(_visibleStickTip.position, _visibleStickTip.rotation, Vector3.one / 4, Color.yellow);
		Gizmos.Cube(_visibleCameraRoot.position, _visibleCameraRoot.rotation, Vector3.one / 4, Color.grey);
	}

	protected override bool CheckReady() =>
		base.CheckReady() &&
		(Locator.GetPlayerTransform() || AttachedTransform);

	protected override bool CheckValid() => base.CheckValid() && !Player.IsDead;

	public static PlayerTransformSync LocalInstance { get; private set; }

	protected override bool UseInterpolation => true;
}