using Cysharp.Threading.Tasks;
using QSB.Player.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.Player
{
	public static class JoinLeaveSingularity
	{
		public static async UniTaskVoid Create(PlayerInfo player, bool joining)
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

				await UniTask.WaitUntil(() => player.Body);
				player.Body.SetActive(false);
				await UniTask.WaitUntil(() => player.TransformSync.ReferenceTransform);
			}

			DebugLog.DebugWrite($"WARP {player.PlayerId}");

			var go = new GameObject(nameof(JoinLeaveSingularity));
			go.transform.parent = player.TransformSync.ReferenceTransform;
			go.transform.localPosition = player.TransformSync.transform.position;
			go.transform.localRotation = player.TransformSync.transform.rotation;

			#region fake player

			GameObject fakePlayer = null;
			if (!joining)
			{
				player.Body.SetActive(false);
				fakePlayer = Object.Instantiate(player.Body, go.transform);
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
			effect.transform.parent = go.transform;
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
				DebugLog.DebugWrite($"WARP IN {player.PlayerId}");
				effect.WarpObjectIn(0);
			}
			else
			{
				DebugLog.DebugWrite($"WARP OUT {player.PlayerId}");
				effect.WarpObjectOut(0);
			}

			var tcs = new UniTaskCompletionSource();
			effect.OnWarpComplete += () => tcs.TrySetResult();
			await tcs.Task;
			DebugLog.DebugWrite($"WARP DONE {player.PlayerId}");

			if (!joining)
			{
				Object.Destroy(fakePlayer);
			}

			await UniTask.WaitUntil(() => !singularity._owOneShotSource.isPlaying);

			Object.Destroy(go);
		}
	}
}
