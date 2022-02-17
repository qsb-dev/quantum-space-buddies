using Cysharp.Threading.Tasks;
using QSB.Player.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
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

			if (joining &&
				(PlayerTransformSync.LocalInstance == null ||
				player.PlayerId < QSBPlayerManager.LocalPlayerId))
			{
				// player was here before we joined
				return;
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

				fakePlayer = player.Body.InstantiateInactive();
				fakePlayer.transform.parent = transform;
				fakePlayer.transform.localPosition = Vector3.zero;
				fakePlayer.transform.localRotation = Quaternion.identity;
				fakePlayer.transform.localScale = Vector3.one;
				foreach (var component in fakePlayer.GetComponentsInChildren<Component>(true))
				{
					if (component is Behaviour behaviour)
					{
						behaviour.enabled = false;
					}
					else if (component is not (Transform or Renderer))
					{
						Object.Destroy(component);
					}
				}

				fakePlayer.SetActive(true);
			}

			#endregion

			#region effect

			var effect = QSBWorldSync.GetUnityObjects<GravityCannonController>().First()._warpEffect;
			effect = effect.gameObject.InstantiateInactive().GetComponent<SingularityWarpEffect>();
			effect.transform.parent = transform;
			effect.transform.localPosition = Vector3.zero;
			effect.transform.localRotation = Quaternion.identity;
			effect.transform.localScale = Vector3.one;

			effect.enabled = true;
			effect._warpedObjectGeometry = joining ? player.Body : fakePlayer;

			var singularity = effect._singularity;
			singularity.enabled = true;
			singularity._startActive = false;
			singularity._muteSingularityEffectAudio = false;
			singularity._creationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
			singularity._destructionCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

			var renderer = effect.GetComponent<Renderer>();
			renderer.material.SetFloat("_DistortFadeDist", 3);
			renderer.material.SetFloat("_MassScale", joining ? -1 : 1);
			renderer.material.SetFloat("_MaxDistortRadius", 10);
			renderer.transform.localScale = Vector3.one * 10;
			renderer.material.SetFloat("_Radius", 1);
			renderer.material.SetColor("_Color", joining ? Color.white : Color.black);

			effect.gameObject.SetActive(true);

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
