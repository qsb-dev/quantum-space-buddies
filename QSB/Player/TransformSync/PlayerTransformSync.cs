using OWML.Common;
using QSB.Messaging;
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

	private bool _hasRanOnStartClient;

	public override void OnStartClient()
	{
		if (_hasRanOnStartClient)
		{
			DebugLog.ToConsole($"ERROR - OnStartClient is being called AGAIN for {Player.PlayerId}'s PlayerTransformSync!", MessageType.Error);
			return;
		}

		_hasRanOnStartClient = true;
		if (QSBPlayerManager.PlayerList.Any(x => x.TransformSync == this))
		{
			// this really shouldnt happen...
			DebugLog.ToConsole($"Error - A PlayerInfo already exists with TransformSync {name}", MessageType.Error);
			Destroy(gameObject); // probably bad
			return;
		}

		var player = new PlayerInfo(this);
		QSBPlayerManager.PlayerList.SafeAdd(player);
		base.OnStartClient();
		QSBPlayerManager.OnAddPlayer?.Invoke(Player);
		DebugLog.DebugWrite($"Create Player : {Player}", MessageType.Info);

		JoinLeaveSingularity.Create(Player, true);
	}

	public override void OnStartLocalPlayer()
	{
		if (LocalInstance != null)
		{
			DebugLog.ToConsole($"ERROR - LocalInstance is already non-null in OnStartLocalPlayer!", MessageType.Error);
			Destroy(gameObject); // probably bad
			return;
		}

		LocalInstance = this;
	}

	public override void OnStopLocalPlayer() => LocalInstance = null;

	public override void OnStopClient()
	{
		DebugLog.DebugWrite($"OnStopClient");

		JoinLeaveSingularity.Create(Player, false);

		// TODO : Maybe move this to a leave event...? Would ensure everything could finish up before removing the player
		QSBPlayerManager.OnRemovePlayer?.Invoke(Player);
		base.OnStopClient();
		Player.HudMarker?.Remove();
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
		if (!QSBCore.DebugSettings.DrawLines
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