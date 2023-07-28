using QSB.Audio;
using QSB.Player;
using UnityEngine;

namespace QSB.Animation.Player.Thrusters;

public static class ThrusterManager
{
	public static void CreateRemotePlayerVFX(PlayerInfo player)
	{
		var newVfx = player.Body.transform.Find("REMOTE_PlayerVFX").gameObject;

		InitWashController(newVfx.transform.Find("ThrusterWash").gameObject, player);
		InitFlameControllers(newVfx, player);
		InitParticleControllers(newVfx, player);
	}

	public static void CreateRemotePlayerSFX(PlayerInfo player)
	{
		player.Body.GetComponentInChildren<QSBJetpackThrusterAudio>(true)?.Init(player);
	}

	private static void InitFlameControllers(GameObject root, PlayerInfo player)
	{
		var existingControllers = root.GetComponentsInChildren<RemoteThrusterFlameController>(true);
		foreach (var controller in existingControllers)
		{
			controller.Init(player);
		}
	}

	private static void InitParticleControllers(GameObject root, PlayerInfo player)
	{
		var existingBehaviours = root.GetComponentsInChildren<RemoteThrusterParticlesBehaviour>(true);
		foreach (var item in existingBehaviours)
		{
			item.Init(player);
		}
	}

	private static void InitWashController(GameObject root, PlayerInfo player)
	{
		var newObj = root.GetComponent<RemoteThrusterWashController>();
		newObj.Init(player);
	}
}