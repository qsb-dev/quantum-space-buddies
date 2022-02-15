using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace QSB.Player
{
	public class JoinLeaveSingularity : MonoBehaviour
	{
		public static void Create(PlayerInfo player, bool joining)
		{
			var joinLeaveSingularity = new GameObject(nameof(JoinLeaveSingularity))
				.AddComponent<JoinLeaveSingularity>();
			joinLeaveSingularity._player = player;
			joinLeaveSingularity._joining = joining;
		}

		private PlayerInfo _player;
		private bool _joining;

		private SingularityWarpEffect CreateEffect()
		{
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

			var referenceEffect = _joining ?
				QSBWorldSync.GetUnityObjects<GravityCannonController>().First()._warpEffect :
				QSBWorldSync.GetUnityObjects<NomaiShuttleController>().First()._warpEffect;
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

			// var renderer = effectGo.GetComponent<OWRenderer>();
			// renderer.SetMaterialProperty(Shader.PropertyToID("_DistortFadeDist"), 3);
			// renderer.SetMaterialProperty(Shader.PropertyToID("_MassScale"), _joining ? -1 : 1);
			// renderer.SetMaterialProperty(Shader.PropertyToID("_MaxDistortRadius"), 10);
			// renderer.SetMaterialProperty(Shader.PropertyToID("_Radius"), 1);
			// renderer.SetColor(_joining ? Color.white : Color.black);

			effectGo.SetActive(true);

			return effect;
		}

		private IEnumerator Start()
		{
			DebugLog.DebugWrite($"WARP {_player.TransformSync}");

			transform.parent = _player.Body.transform.parent;
			transform.localPosition = _player.Body.transform.localPosition;
			transform.localRotation = _player.Body.transform.localRotation;
			transform.localScale = _player.Body.transform.localScale;

			var effect = CreateEffect();
			_player.SetVisible(false);
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

		private void Update()
		{
			var playerGo = _player.Body;
			if (!playerGo)
			{
				enabled = false;
				return;
			}

			transform.parent = playerGo.transform.parent;
			transform.localPosition = playerGo.transform.localPosition;
			transform.localRotation = playerGo.transform.localRotation;
			transform.localScale = playerGo.transform.localScale;
		}

		private void OnWarpComplete()
		{
			DebugLog.DebugWrite($"WARP DONE {_player.TransformSync}");

			Destroy(gameObject);

			_player.SetVisible(true);
		}

		private void OnRenderObject()
		{
			Popcron.Gizmos.Cube(transform.position, transform.rotation, Vector3.one * 3, Color.cyan);
		}
	}
}
