using HarmonyLib;
using QSB.Animation.Player;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.EchoesOfTheEye.DreamWorld.Messages;

public class DreamWorldFakePlayer : MonoBehaviour
{
	private static readonly List<DreamWorldFakePlayer> _instances = new();

	public static void Create(PlayerInfo player)
	{
		var go = new GameObject($"player {player} DreamWorldFakePlayer");
		go.SetActive(false);
		go.AddComponent<DreamWorldFakePlayer>()._player = player;
		go.SetActive(true);
	}

	public static void Destroy(PlayerInfo player)
	{
		foreach (var dreamWorldFakePlayer in _instances)
		{
			if (dreamWorldFakePlayer._player == player)
			{
				Destroy(dreamWorldFakePlayer.gameObject);
			}
		}
	}

	private PlayerInfo _player;

	private void Awake()
	{
		_instances.SafeAdd(this);
		QSBPlayerManager.OnRemovePlayer += OnRemovePlayer;

		transform.parent = _player.TransformSync.ReferenceTransform;
		transform.localPosition = _player.TransformSync.transform.position;
		transform.localRotation = _player.TransformSync.transform.rotation;

		#region fake player

		var fakePlayer = _player.Body.transform.Find("REMOTE_Traveller_HEA_Player_v2").gameObject.InstantiateInactive();
		fakePlayer.transform.SetParent(transform, false);

		Destroy(fakePlayer.GetComponent<Animator>());
		Destroy(fakePlayer.GetComponent<PlayerHeadRotationSync>());

		var REMOTE_ItemCarryTool = fakePlayer.transform.Find(
			// TODO : kill me for my sins
			"Traveller_Rig_v01:Traveller_Trajectory_Jnt/" +
			"Traveller_Rig_v01:Traveller_ROOT_Jnt/" +
			"Traveller_Rig_v01:Traveller_Spine_01_Jnt/" +
			"Traveller_Rig_v01:Traveller_Spine_02_Jnt/" +
			"Traveller_Rig_v01:Traveller_Spine_Top_Jnt/" +
			"Traveller_Rig_v01:Traveller_RT_Arm_Clavicle_Jnt/" +
			"Traveller_Rig_v01:Traveller_RT_Arm_Shoulder_Jnt/" +
			"Traveller_Rig_v01:Traveller_RT_Arm_Elbow_Jnt/" +
			"Traveller_Rig_v01:Traveller_RT_Arm_Wrist_Jnt/" +
			"REMOTE_ItemCarryTool"
		).gameObject;
		Destroy(REMOTE_ItemCarryTool);

		fakePlayer.SetActive(true);

		#endregion
	}

	private void OnDestroy()
	{
		_instances.QuickRemove(this);
		QSBPlayerManager.OnRemovePlayer -= OnRemovePlayer;
	}

	private void OnRemovePlayer(PlayerInfo player)
	{
		if (player != _player)
		{
			return;
		}
		Destroy(gameObject);
	}
}

public class DreamWorldFakePlayerPatch : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	/// <summary>
	/// do this early to create the fake player BEFORE teleporting
	/// </summary>
	[HarmonyPatch(typeof(DreamWorldController), nameof(DreamWorldController.EnterDreamWorld))]
	[HarmonyPrefix]
	private static void EnterDreamWorld()
	{
		if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == ItemType.DreamLantern)
		{
			new DreamWorldFakePlayerMessage().Send();
		}
	}
}

public class DreamWorldFakePlayerMessage : QSBMessage
{
	public override void OnReceiveRemote()
	{
		DreamWorldFakePlayer.Create(QSBPlayerManager.GetPlayer(From));
	}
}
