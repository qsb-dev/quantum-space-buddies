using QSB.Animation.Player;
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

		#region fake player

		_player.Body.SetActive(false);

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
