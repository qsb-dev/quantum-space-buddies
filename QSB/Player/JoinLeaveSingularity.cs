using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.Player
{
	public class JoinLeaveSingularity : MonoBehaviour
	{
		public static void Create(PlayerInfo player, bool joining)
		{
			var go = new GameObject(nameof(JoinLeaveSingularity));
			go.SetActive(false);
			var joinLeaveSingularity = go.AddComponent<JoinLeaveSingularity>();
			joinLeaveSingularity._player = player;
			joinLeaveSingularity._joining = joining;
			go.SetActive(true);
		}

		private PlayerInfo _player;
		private bool _joining;

		private SingularityWarpEffect _effect;

		private void Awake()
		{
			DebugLog.DebugWrite($"WARP {_player.TransformSync}");

			var playerGo = _player.Body;
			transform.parent = playerGo.transform.parent;
			transform.localPosition = playerGo.transform.localPosition;
			transform.localRotation = playerGo.transform.localRotation;
			transform.localScale = playerGo.transform.localScale;

			var fakePlayerGo = _player.Body.transform.Find("REMOTE_Traveller_HEA_Player_v2")
				.gameObject.InstantiateInactive();
			fakePlayerGo.transform.parent = transform;
			fakePlayerGo.transform.localPosition = Vector3.zero;
			fakePlayerGo.transform.localRotation = Quaternion.identity;
			fakePlayerGo.transform.localScale = Vector3.one;
			fakePlayerGo.SetActive(true);

			var effectGo = QSBWorldSync.GetUnityObjects<GravityCannonController>().First()._warpEffect
				.gameObject.InstantiateInactive();
			effectGo.transform.parent = transform;
			effectGo.transform.localPosition = Vector3.zero;
			effectGo.transform.localRotation = Quaternion.identity;
			effectGo.transform.localScale = Vector3.one;

			_effect = effectGo.GetComponent<SingularityWarpEffect>();
			_effect.enabled = true;
			_effect._singularity.enabled = true;

			_effect._singularity._startActive = false;
			_effect._singularity._muteSingularityEffectAudio = false;
			// var curve = AnimationCurve.EaseInOut(0, 0, .2f, 1);
			// _effect._singularity._creationCurve = curve;
			// _effect._singularity._destructionCurve = curve;

			var renderer = effectGo.GetComponent<OWRenderer>();
			// renderer.SetMaterialProperty(Shader.PropertyToID("_DistortFadeDist"), 3);
			// renderer.SetMaterialProperty(Shader.PropertyToID("_MassScale"), _joining ? -1 : 1);
			// renderer.SetMaterialProperty(Shader.PropertyToID("_MaxDistortRadius"), 10);
			// renderer.SetMaterialProperty(Shader.PropertyToID("_Radius"), 1);
			renderer.SetColor(_joining ? Color.white * 2 : Color.black);

			_effect._warpedObjectGeometry = fakePlayerGo;
			_effect.OnWarpComplete += OnWarpComplete;
			effectGo.SetActive(true);
		}

		private void Start()
		{
			_player.SetVisible(false);

			const float length = 1;
			if (_joining)
			{
				DebugLog.DebugWrite($"WARP IN {_player.TransformSync}");
				_effect.WarpObjectIn(length);
			}
			else
			{
				DebugLog.DebugWrite($"WARP OUT {_player.TransformSync}");
				_effect.WarpObjectOut(length);
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
