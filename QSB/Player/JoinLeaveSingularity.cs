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
			if (!player.IsLocalPlayer)
			{
				return;
			}

			if (joining)
			{
				if (PlayerTransformSync.LocalInstance == null ||
					player.PlayerId < QSBPlayerManager.LocalPlayerId)
				{
					return;
				}

				await UniTask.WaitUntil(() => player.TransformSync.IsValid && player.TransformSync.ReferenceTransform);
			}

			DebugLog.DebugWrite($"WARP {player.TransformSync}");

			player.SetVisible(false);

			var go = new GameObject(nameof(JoinLeaveSingularity));
			go.transform.parent = player.TransformSync.ReferenceTransform;
			go.transform.localPosition = player.TransformSync.transform.position;
			go.transform.localRotation = player.TransformSync.transform.rotation;

			#region fake player

			player.SetVisible(true);
			var fakePlayer = player.Body.transform.Find("REMOTE_Traveller_HEA_Player_v2")
				.gameObject.InstantiateInactive();
			fakePlayer.transform.SetParent(go.transform, false);
			fakePlayer.SetActive(true);
			player.SetVisible(false);

			#endregion

			#region effect

			var effectGo = QSBWorldSync.GetUnityObjects<GravityCannonController>().First()._warpEffect
				.gameObject.InstantiateInactive();
			effectGo.transform.parent = go.transform;
			effectGo.transform.localPosition = Vector3.zero;
			effectGo.transform.localRotation = Quaternion.identity;
			effectGo.transform.localScale = Vector3.one;

			var effect = effectGo.GetComponent<SingularityWarpEffect>();
			effect.enabled = true;
			effect._warpedObjectGeometry = fakePlayer;

			effect._singularity.enabled = true;
			effect._singularity._startActive = false;
			effect._singularity._muteSingularityEffectAudio = false;
			effect._singularity._creationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
			effect._singularity._destructionCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

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

			effect.OnWarpComplete += () =>
			{
				DebugLog.DebugWrite($"WARP DONE {player.TransformSync}");

				Object.Destroy(go);

				player.SetVisible(true);
			};
			if (joining)
			{
				DebugLog.DebugWrite($"WARP IN {player.TransformSync}");
				effect.WarpObjectIn(0);
			}
			else
			{
				DebugLog.DebugWrite($"WARP OUT {player.TransformSync}");
				effect.WarpObjectOut(0);
			}
		}
	}
}
