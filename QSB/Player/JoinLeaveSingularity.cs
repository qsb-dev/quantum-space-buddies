using QSB.Utility;
using System.Collections;
using UnityEngine;

namespace QSB.Player
{
	public class JoinLeaveSingularity : MonoBehaviour
	{
		private PlayerInfo _player;
		private bool _joining;

		public static void Create(PlayerInfo player, bool joining)
		{
			var joinLeaveSingularity = new GameObject(nameof(JoinLeaveSingularity))
				.AddComponent<JoinLeaveSingularity>();
			joinLeaveSingularity._player = player;
			joinLeaveSingularity._joining = joining;
		}

		private IEnumerator Start()
		{
			DebugLog.DebugWrite($"WARP {_player.TransformSync}");

			transform.parent = _player.TransformSync.ReferenceTransform;
			transform.localPosition = _player.Body.transform.localPosition;
			transform.localRotation = _player.Body.transform.localRotation;
			transform.localScale = _player.Body.transform.localScale;

			var SingularityWarpEffect = _player.Body.transform.Find("SingularityWarpEffect").gameObject;

			var effectGo = SingularityWarpEffect.InstantiateInactive();
			effectGo.transform.parent = transform;
			effectGo.transform.localPosition = Vector3.zero;
			effectGo.transform.localRotation = Quaternion.identity;
			effectGo.transform.localScale = Vector3.one;

			var effect = effectGo.GetComponent<SingularityWarpEffect>();
			var curve = AnimationCurve.EaseInOut(0, 0, .2f, 1);
			effect._singularity._creationCurve = curve;
			effect._singularity._destructionCurve = curve;

			var renderer = effectGo.GetComponent<OWRenderer>();
			renderer.SetMaterialProperty(Shader.PropertyToID("_DistortFadeDist"), 3);
			renderer.SetMaterialProperty(Shader.PropertyToID("_MassScale"), _joining ? -1 : 1);
			renderer.SetMaterialProperty(Shader.PropertyToID("_MaxDistortRadius"), 10);
			renderer.SetMaterialProperty(Shader.PropertyToID("_Radius"), 1);
			renderer.SetColor(_joining ? Color.white * 2 : Color.black);

			var warpedObjectGeometry = effect._warpedObjectGeometry.InstantiateInactive();
			warpedObjectGeometry.transform.parent = transform;
			warpedObjectGeometry.transform.localPosition = Vector3.zero;
			warpedObjectGeometry.transform.localRotation = Quaternion.identity;
			warpedObjectGeometry.transform.localScale = Vector3.one;
			effect._warpedObjectGeometry = warpedObjectGeometry;

			warpedObjectGeometry.SetActive(true);
			effectGo.SetActive(true);

			_player.SetVisible(false);

			effect.OnWarpComplete += OnWarpComplete;

			yield return new WaitForSeconds(1);

			const float length = 1;
			if (_joining)
			{
				DebugLog.DebugWrite($"WARP IN {_player.TransformSync}");
				effect.WarpObjectIn(length);
			}
			else
			{
				DebugLog.DebugWrite($"WARP OUT {_player.TransformSync}");
				effect.WarpObjectOut(length);
			}
		}

		private void OnWarpComplete()
		{
			DebugLog.DebugWrite($"WARP DONE {_player.TransformSync}");

			Destroy(gameObject);

			_player.SetVisible(true);
		}
	}
}
