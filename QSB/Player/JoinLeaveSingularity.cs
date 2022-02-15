using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.Player
{
	public static class JoinLeaveSingularity
	{
		public static void Create(PlayerInfo player, bool joining)
		{
			DebugLog.DebugWrite($"{player.TransformSync} join/leave singularity: (joining = {joining})");

			var go = new GameObject(nameof(JoinLeaveSingularity));

			var playerGo = player.Body;
			playerGo.SetActive(false);
			go.transform.parent = playerGo.transform.parent;
			go.transform.localPosition = playerGo.transform.localPosition;
			go.transform.localRotation = playerGo.transform.localRotation;
			go.transform.localScale = playerGo.transform.localScale;

			var fakePlayerGo = playerGo.InstantiateInactive();
			fakePlayerGo.transform.parent = go.transform;
			fakePlayerGo.transform.localPosition = Vector3.zero;
			fakePlayerGo.transform.localRotation = Quaternion.identity;
			fakePlayerGo.transform.localScale = Vector3.one;

			foreach (var component in fakePlayerGo.GetComponents<Component>())
			{
				if (component is not (Transform or Renderer))
				{
					Object.Destroy(component);
				}
			}

			fakePlayerGo.SetActive(true);

			var referenceEffect = joining ?
				QSBWorldSync.GetUnityObjects<ProbeLauncher>()
					.Select(x => x._probeRetrievalEffect)
					.First(x => x) :
				QSBWorldSync.GetUnityObjects<SurveyorProbe>()
					.Select(x => x._warpEffect)
					.First(x => x);
			var effectGo = referenceEffect.gameObject.InstantiateInactive();
			effectGo.transform.parent = go.transform;
			effectGo.transform.localPosition = Vector3.zero;
			effectGo.transform.localRotation = Quaternion.identity;
			effectGo.transform.localScale = Vector3.one;

			var effect = effectGo.GetComponent<SingularityWarpEffect>();
			effect._warpedObjectGeometry = fakePlayerGo;
			effectGo.SetActive(true);

			effect.OnWarpComplete += () =>
			{
				DebugLog.DebugWrite($"{player.TransformSync} warp complete");

				Object.Destroy(go);

				if (playerGo)
				{
					playerGo.SetActive(true);
				}
			};
			const float length = 3;
			if (joining)
			{
				DebugLog.DebugWrite($"{player.TransformSync} warp in (white hole)");
				effect.WarpObjectIn(length);
			}
			else
			{
				DebugLog.DebugWrite($"{player.TransformSync} warp out (black hole)");
				effect.WarpObjectOut(length);
			}
		}
	}
}
