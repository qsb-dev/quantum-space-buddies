using QSB.Player;
using UnityEngine;
using UnityEngine.Rendering;

namespace QSB.Tools.ProbeLauncherTool
{
	internal static class ProbeLauncherCreator
	{
		private static readonly Vector3 ProbeLauncherOffset = new(0.5745087f, -0.26f, 0.4453125f);

		internal static void CreateProbeLauncher(PlayerInfo player)
		{
			var ProbeLauncher = GameObject.Find("PlayerCamera/ProbeLauncher");

			// Create new ProbeLauncher
			var REMOTE_ProbeLauncher = new GameObject("REMOTE_ProbeLauncher");
			REMOTE_ProbeLauncher.SetActive(false);

			// Copy children of ProbeLauncher
			var Props_HEA_ProbeLauncher = ProbeLauncher.transform.Find("Props_HEA_ProbeLauncher");
			var REMOTE_Props_HEA_ProbeLauncher = Object.Instantiate(Props_HEA_ProbeLauncher, REMOTE_ProbeLauncher.transform, false);

			var LaunchParticleEffect_Underwater = ProbeLauncher.transform.Find("LaunchParticleEffect_Underwater");
			var REMOTE_LaunchParticleEffect_Underwater = Object.Instantiate(LaunchParticleEffect_Underwater, REMOTE_ProbeLauncher.transform, false);

			var LaunchParticleEffect = ProbeLauncher.transform.Find("LaunchParticleEffect");
			var REMOTE_LaunchParticleEffect = Object.Instantiate(LaunchParticleEffect, REMOTE_ProbeLauncher.transform, false);

			// Set up effects
			var effects = REMOTE_ProbeLauncher.AddComponent<ProbeLauncherEffects>();
			effects._launchParticles = REMOTE_LaunchParticleEffect.GetComponent<ParticleSystem>();
			effects._underwaterLaunchParticles = REMOTE_LaunchParticleEffect_Underwater.GetComponent<ParticleSystem>();
			effects._owAudioSource = player.AudioController._repairToolSource;

			var recallEffect = REMOTE_Props_HEA_ProbeLauncher.Find("RecallEffect");

			var arrow = REMOTE_Props_HEA_ProbeLauncher.Find("PressureGauge_Arrow");
			arrow.GetComponent<MeshRenderer>().material = PlayerToolsManager.Props_HEA_PlayerTool_mat;
			arrow.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.On;

			var chassis = REMOTE_Props_HEA_ProbeLauncher.Find("ProbeLauncherChassis");
			chassis.GetComponent<MeshRenderer>().material = PlayerToolsManager.Props_HEA_PlayerTool_mat;
			chassis.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.On;
			Object.Destroy(REMOTE_Props_HEA_ProbeLauncher.Find("Props_HEA_ProbeLauncher_Prepass").gameObject);

			var preLaunchProbe = REMOTE_Props_HEA_ProbeLauncher.Find("Props_HEA_Probe_Prelaunch");
			Object.Destroy(preLaunchProbe.Find("Props_HEA_Probe_Prelaunch_Prepass").gameObject);

			// fuck you unity
			var materials = preLaunchProbe.GetComponent<MeshRenderer>().materials;
			materials[0] = PlayerToolsManager.Props_HEA_PlayerTool_mat;
			materials[1] = PlayerToolsManager.Props_HEA_Lightbulb_OFF_mat;
			preLaunchProbe.GetComponent<MeshRenderer>().materials = materials;

			preLaunchProbe.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.On;

			var tool = REMOTE_ProbeLauncher.AddComponent<QSBProbeLauncherTool>();
			var spring = new DampedSpringQuat
			{
				velocity = Vector4.zero,
				settings = new DampedSpringSettings
				{
					springConstant = 50f,
					dampingCoefficient = 8.485282f,
					mass = 1
				}
			};

			tool.MoveSpring = spring;
			tool.StowTransform = PlayerToolsManager.StowTransform;
			tool.HoldTransform = PlayerToolsManager.HoldTransform;
			tool.ArrivalDegrees = 5f;
			tool.Type = ToolType.ProbeLauncher;
			tool.ToolGameObject = REMOTE_Props_HEA_ProbeLauncher.gameObject;
			tool.Player = player;
			tool.PreLaunchProbeProxy = preLaunchProbe.gameObject;
			tool.ProbeRetrievalEffect = recallEffect.GetComponent<SingularityWarpEffect>();
			tool.Effects = effects;

			REMOTE_ProbeLauncher.transform.parent = player.CameraBody.transform;
			REMOTE_ProbeLauncher.transform.localPosition = ProbeLauncherOffset;

			//UnityEvents.FireInNUpdates(() => REMOTE_ProbeLauncher.SetActive(true), 5);
			REMOTE_ProbeLauncher.SetActive(true);
		}
	}
}
