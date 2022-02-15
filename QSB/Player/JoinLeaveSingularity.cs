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

		private void Awake()
		{
			DebugLog.DebugWrite($"WARP {_player.TransformSync}");

			transform.parent = _player.Body.transform.parent;
			transform.localPosition = _player.Body.transform.localPosition;
			transform.localRotation = _player.Body.transform.localRotation;
			transform.localScale = _player.Body.transform.localScale;

			#region fake player

			var fakePlayer = _player.Body.InstantiateInactive();
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
					Destroy(component);
				}
			}

			fakePlayer.SetActive(true);

			#endregion

			_player.SetVisible(false);

			#region effect

			var referenceEffect = QSBWorldSync.GetUnityObjects<GravityCannonController>().First()._warpEffect;
			var effectGo = referenceEffect.gameObject.InstantiateInactive();
			effectGo.transform.parent = transform;
			effectGo.transform.localPosition = Vector3.zero;
			effectGo.transform.localRotation = Quaternion.identity;
			effectGo.transform.localScale = Vector3.one;

			var effect = effectGo.GetComponent<SingularityWarpEffect>();
			effect.enabled = true;
			effect._warpedObjectGeometry = fakePlayer;
			effect.OnWarpComplete += OnWarpComplete;

			effect._singularity.enabled = true;
			effect._singularity._startActive = false;
			effect._singularity._muteSingularityEffectAudio = false;
			effect._singularity._creationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
			effect._singularity._destructionCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

			var renderer = effectGo.GetComponent<Renderer>();
			renderer.material.SetFloat("_DistortFadeDist", 3);
			renderer.material.SetFloat("_MassScale", _joining ? -1 : 1);
			renderer.material.SetFloat("_MaxDistortRadius", 10);
			renderer.transform.localScale = Vector3.one * 10;
			renderer.material.SetFloat("_Radius", 1);
			renderer.material.SetColor("_Color", _joining ? Color.white : Color.black);

			effectGo.SetActive(true);

			#endregion

			Delay.RunNextFrame(() =>
			{
				if (_joining)
				{
					DebugLog.DebugWrite($"WARP IN {_player.TransformSync}");
					effect.WarpObjectIn(0);
				}
				else
				{
					DebugLog.DebugWrite($"WARP OUT {_player.TransformSync}");
					effect.WarpObjectOut(0);
				}
			});
		}

		private void Update()
		{
			if (!_player.Body)
			{
				enabled = false;
				return;
			}

			transform.parent = _player.Body.transform.parent;
			transform.localPosition = _player.Body.transform.localPosition;
			transform.localRotation = _player.Body.transform.localRotation;
			transform.localScale = _player.Body.transform.localScale;
		}

		private void OnWarpComplete()
		{
			DebugLog.DebugWrite($"WARP DONE {_player.TransformSync}");

			Destroy(gameObject);

			_player.SetVisible(true);
		}
	}
}
