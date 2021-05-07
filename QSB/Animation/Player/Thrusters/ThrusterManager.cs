using OWML.Utils;
using QSB.Player;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Animation.Player.Thrusters
{
	class ThrusterManager
	{
		public static GameObject CreateRemotePlayerVFX(PlayerInfo player)
		{
			DebugLog.DebugWrite($"Create PlayerVFX for player {player.PlayerId}");
			var localPlayerVfx = GameObject.Find("PlayerVFX");
			var newVfx = UnityEngine.Object.Instantiate(localPlayerVfx);

			CreateParticlesController(newVfx);
			CreateThrusterWashController(newVfx.transform.Find("ThrusterWash").gameObject, player);

			return newVfx;
		}

		private static void CreateParticlesController(GameObject root)
		{

		}

		private static void CreateThrusterWashController(GameObject root, PlayerInfo player)
		{
			var old = root.GetComponent<ThrusterWashController>();
			var oldDistanceScale = old.GetValue<AnimationCurve>("_emissionDistanceScale");
			var oldThrusterScale = old.GetValue<AnimationCurve>("_emissionThrusterScale");
			var defaultParticleSystem = old.GetValue<ParticleSystem>("_defaultParticleSystem");

			UnityEngine.Object.Destroy(old);

			var newObj = root.AddComponent<RemoteThrusterWashController>();
			newObj.InitFromOld(oldDistanceScale, oldThrusterScale, defaultParticleSystem, player);
		}

		private static void CreateThrusterFlameController(GameObject root)
		{

		}
	}
}
