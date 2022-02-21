using Cysharp.Threading.Tasks;
using QSB.Animation.Player;
using QSB.Player.TransformSync;
using QSB.Utility;
using System.Threading;
using UnityEngine;

namespace QSB.Player
{
	public static class JoinLeaveSingularity
	{
		public static void Create(PlayerInfo player, bool joining)
		{
			if (player.IsLocalPlayer)
			{
				return;
			}

			if (joining)
			{
				if (PlayerTransformSync.LocalInstance == null ||
					player.PlayerId < QSBPlayerManager.LocalPlayerId)
				{
					// player was here before we joined
					return;
				}
			}
			else
			{
				if (!player.Visible)
				{
					return;
				}
			}

			var go = new GameObject($"player {player.PlayerId} JoinLeaveSingularity");
			var ct = go.GetCancellationTokenOnDestroy();
			UniTask.Create(async () =>
			{
				DebugLog.DebugWrite($"{go.name}: WARP TASK");

				await go.name.Try("running warp task",
					() => Run(go.transform, player, joining, ct));
				Object.Destroy(go);

				DebugLog.DebugWrite($"{go.name}: WARP TASK DONE");
			});
		}

		private static async UniTask Run(Transform transform, PlayerInfo player, bool joining, CancellationToken ct)
		{
			if (joining)
			{
				await UniTask.WaitUntil(() => player.Body, cancellationToken: ct);
				player.Body.SetActive(false);
				await UniTask.WaitUntil(() => player.TransformSync.ReferenceTransform, cancellationToken: ct);
			}

			transform.parent = player.TransformSync.ReferenceTransform;
			transform.localPosition = player.TransformSync.transform.position;
			transform.localRotation = player.TransformSync.transform.rotation;

			#region fake player

			GameObject fakePlayer = null;
			if (!joining)
			{
				player.Body.SetActive(false);

				fakePlayer = player.Body.transform.Find("REMOTE_Traveller_HEA_Player_v2").gameObject.InstantiateInactive();
				fakePlayer.transform.SetParent(transform, false);

				Object.Destroy(fakePlayer.GetComponent<Animator>());
				Object.Destroy(fakePlayer.GetComponent<PlayerHeadRotationSync>());

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
				Object.Destroy(REMOTE_ItemCarryTool);

				fakePlayer.SetActive(true);
			}

			#endregion

			#region effect

			var effectGo = player.Body.transform.Find("JoinLeaveSingularity").gameObject.InstantiateInactive();
			effectGo.transform.SetParent(transform, false);

			var effect = effectGo.GetComponent<SingularityWarpEffect>();
			effect._warpedObjectGeometry = joining ? player.Body : fakePlayer;

			var singularity = effect._singularity;
			singularity._creationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
			singularity._destructionCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

			var renderer = effectGo.GetComponent<Renderer>();
			renderer.material.SetFloat("_DistortFadeDist", 3);
			renderer.material.SetFloat("_MassScale", joining ? -1 : 1);
			renderer.material.SetFloat("_MaxDistortRadius", 10);
			renderer.transform.localScale = Vector3.one * 10;
			renderer.material.SetFloat("_Radius", 1);
			renderer.material.SetColor("_Color", joining ? Color.white : Color.black);

			effectGo.SetActive(true);

			#endregion

			await UniTask.WaitForEndOfFrame();

			if (joining)
			{
				player.Body.SetActive(true);
				DebugLog.DebugWrite($"{transform.name}: WARP IN");
				effect.WarpObjectIn(0);
			}
			else
			{
				DebugLog.DebugWrite($"{transform.name}: WARP OUT");
				effect.WarpObjectOut(0);
			}

			effect.OnWarpComplete += () =>
			{
				DebugLog.DebugWrite($"{transform.name}: WARP DONE");

				if (!joining)
				{
					Object.Destroy(fakePlayer);
				}
			};
			await UniTask.WaitUntil(() => !effect.enabled && !singularity._owOneShotSource.isPlaying, cancellationToken: ct);
		}
	}
}
