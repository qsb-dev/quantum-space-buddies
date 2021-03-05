using OWML.Utils;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects
{
	class QSBNomaiRemoteCameraPlatform : WorldObject<NomaiRemoteCameraPlatform>
	{
		private bool _active
		{
			get => AttachedObject.GetValue<bool>("_active");
			set => AttachedObject.SetValue("_active", value);
		}

		private QSBNomaiRemoteCameraPlatform _slavePlatform
		{
			get => QSBWorldSync.GetWorldFromUnity<QSBNomaiRemoteCameraPlatform, NomaiRemoteCameraPlatform>(AttachedObject.GetValue<NomaiRemoteCameraPlatform>("_slavePlatform"));
			set
			{
				if (value == null)
				{
					AttachedObject.SetValue("_slavePlatform", null);
				}
				else
				{
					AttachedObject.SetValue("_slavePlatform", (value as QSBNomaiRemoteCameraPlatform).AttachedObject);
				}
			}
		}

		private PedestalAnimator _pedestalAnimator
		{
			get => AttachedObject.GetValue<PedestalAnimator>("_pedestalAnimator");
			set => AttachedObject.SetValue("_pedestalAnimator", value);
		}

		private PedestalAnimator _transitionPedestalAnimator
		{
			get => AttachedObject.GetValue<PedestalAnimator>("_transitionPedestalAnimator");
			set => AttachedObject.SetValue("_transitionPedestalAnimator", value);
		}

		private Shape _connectionBounds
		{
			get => AttachedObject.GetValue<Shape>("_connectionBounds");
			set => AttachedObject.SetValue("_connectionBounds", value);
		}

		private float _poolT
		{
			get => AttachedObject.GetValue<float>("_poolT");
			set => AttachedObject.SetValue("_poolT", value);
		}

		private NomaiRemoteCameraPlatform.State _platformState
		{
			get => AttachedObject.GetValue<NomaiRemoteCameraPlatform.State>("_platformState");
			set => AttachedObject.SetValue("_platformState", value);
		}

		private float _transitionFade
		{
			get => AttachedObject.GetValue<float>("_transitionFade");
			set => AttachedObject.SetValue("_transitionFade", value);
		}

		private SharedStone _sharedStone
		{
			get => AttachedObject.GetValue<SharedStone>("_sharedStone");
			set => AttachedObject.SetValue("_sharedStone", value);
		}

		private float _poolFillLength
		{
			get => AttachedObject.GetValue<float>("_poolFillLength");
			set => AttachedObject.SetValue("_poolFillLength", value);
		}

		private float _poolEmptyLength
		{
			get => AttachedObject.GetValue<float>("_poolEmptyLength");
			set => AttachedObject.SetValue("_poolEmptyLength", value);
		}

		private float _fadeInLength
		{
			get => AttachedObject.GetValue<float>("_fadeInLength");
			set => AttachedObject.SetValue("_fadeInLength", value);
		}

		private float _fadeOutLength
		{
			get => AttachedObject.GetValue<float>("_fadeOutLength");
			set => AttachedObject.SetValue("_fadeOutLength", value);
		}

		private OWCamera _playerCamera
		{
			get => AttachedObject.GetValue<OWCamera>("_playerCamera");
			set => AttachedObject.SetValue("_playerCamera", value);
		}

		private GameObject _hologramGroup
		{
			get => AttachedObject.GetValue<GameObject>("_hologramGroup");
			set => AttachedObject.SetValue("_hologramGroup", value);
		}

		private bool _showPlayerRipples
		{
			get => AttachedObject.GetValue<bool>("_showPlayerRipples");
			set => AttachedObject.SetValue("_showPlayerRipples", value);
		}

		private Transform _activePlayerHolo
		{
			get => AttachedObject.GetValue<Transform>("_activePlayerHolo");
			set => AttachedObject.SetValue("_activePlayerHolo", value);
		}

		private Transform _playerHologram
		{
			get => AttachedObject.GetValue<Transform>("_playerHologram");
			set => AttachedObject.SetValue("_playerHologram", value);
		}

		private OWAudioSource _ambientAudioSource
		{
			get => AttachedObject.GetValue<OWAudioSource>("_ambientAudioSource");
			set => AttachedObject.SetValue("_ambientAudioSource", value);
		}

		public override void Init(NomaiRemoteCameraPlatform attachedPlatform, int id)
		{
			ObjectId = id;
			AttachedObject = attachedPlatform;
		}

		private void Disconnect() 
			=> AttachedObject.GetType()
				.GetMethod("Disconnect", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(AttachedObject, null);

		private void OnLeaveBounds()
			=> AttachedObject.GetType()
				.GetMethod("OnLeaveBounds", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(AttachedObject, null);

		private void UpdatePoolRenderer()
			=> AttachedObject.GetType()
				.GetMethod("UpdatePoolRenderer", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(AttachedObject, null);

		private void UpdateRendererFade()
			=> AttachedObject.GetType()
				.GetMethod("UpdateRendererFade", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(AttachedObject, null);

		private void UpdateHologramTransforms()
			=> AttachedObject.GetType()
				.GetMethod("UpdateHologramTransforms", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(AttachedObject, null);

		private void VerifySectorOccupancy()
			=> AttachedObject.GetType()
				.GetMethod("UpdateHologramTransforms", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(AttachedObject, null);

		private void SwitchToRemoteCamera()
			=> AttachedObject.GetType()
				.GetMethod("SwitchToRemoteCamera", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(AttachedObject, null);

		private void SwitchToPlayerCamera()
			=> AttachedObject.GetType()
				.GetMethod("SwitchToPlayerCamera", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(AttachedObject, null);

		private bool wasInLocally;

		public void CustomUpdate()
		{
			bool dontDisable = false;
			if (_active)
			{
				if (_slavePlatform != null && !_slavePlatform.AttachedObject.gameObject.activeInHierarchy)
				{
					DebugLog.DebugWrite($"{AttachedObject.name} Slave platform disabled - disconnecting.");
					Disconnect();
					if (_pedestalAnimator != null)
					{
						_pedestalAnimator.PlayOpen();
					}
					if (_transitionPedestalAnimator != null)
					{
						_transitionPedestalAnimator.PlayOpen();
					}
					_active = false;
				}
				else if (!QSBPlayerManager.PlayerList.Any(x => _connectionBounds.PointInside(x.Camera.transform.position)))
				{
					DebugLog.DebugWrite($"{AttachedObject.name} OnLeaveBounds");
					OnLeaveBounds();
				}
				else if (!_connectionBounds.PointInside(_playerCamera.transform.position) && wasInLocally)
				{
					// we have left the platform, but other people are still on it - just exit without disconnecting the two platforms!
					DebugLog.DebugWrite($"{AttachedObject.name} Local leave");
					SwitchToPlayerCamera();
					_hologramGroup.SetActive(false);
					wasInLocally = false;
				}
				else if (_connectionBounds.PointInside(_playerCamera.transform.position) 
					&& (_platformState == NomaiRemoteCameraPlatform.State.Connected 
						|| _platformState == NomaiRemoteCameraPlatform.State.Connecting_FadeIn  
						|| _platformState == NomaiRemoteCameraPlatform.State.Connecting_FadeOut)
					&& !wasInLocally)
				{
					// local player entered platform while someone else was still on it - just enter!
					DebugLog.DebugWrite($"{AttachedObject.name} Local enter");
					SwitchToRemoteCamera();
					_hologramGroup.SetActive(true);
					wasInLocally = true;
				}
				dontDisable = QSBPlayerManager.PlayerList.Any(x => _connectionBounds.PointInside(x.Camera.transform.position));
			}
			_poolT = _active
				? Mathf.MoveTowards(_poolT, 1f, Time.deltaTime / _poolFillLength)
				: Mathf.MoveTowards(_poolT, 0f, Time.deltaTime / _poolEmptyLength);
			UpdatePoolRenderer();
			switch (_platformState)
			{
				case NomaiRemoteCameraPlatform.State.Connecting_FadeIn:
					// Fade out props on the current platform?
					_transitionFade = Mathf.MoveTowards(_transitionFade, 1f, Time.deltaTime / _fadeInLength);
					UpdateRendererFade();
					if (_transitionFade == 1f)
					{
						_slavePlatform._poolT = _poolT;
						_slavePlatform._showPlayerRipples = true;
						_slavePlatform._activePlayerHolo = _playerHologram.GetChild(0);
						_slavePlatform.UpdatePoolRenderer();
						_transitionFade = 0f;
						UpdateRendererFade();
						_slavePlatform._transitionFade = 1f;
						_slavePlatform.UpdateRendererFade();
						SwitchToRemoteCamera();
						_hologramGroup.SetActive(true);
						UpdateHologramTransforms();
						_ambientAudioSource.FadeIn(3f, true, false, 1f);
						Locator.GetAudioMixer().MixRemoteCameraPlatform(_fadeInLength);
						DebugLog.DebugWrite($"{AttachedObject.name} Switch to connecting_fadeout.");
						_platformState = NomaiRemoteCameraPlatform.State.Connecting_FadeOut;
					}
					break;
				case NomaiRemoteCameraPlatform.State.Connecting_FadeOut:
					// Fade in props on the target platform?
					_slavePlatform._transitionFade = Mathf.MoveTowards(_slavePlatform._transitionFade, 0f, Time.deltaTime / _fadeInLength);
					_slavePlatform.UpdateRendererFade();
					UpdateHologramTransforms();
					_slavePlatform._poolT = _poolT;
					_slavePlatform.UpdatePoolRenderer();
					if (_slavePlatform._transitionFade == 0f)
					{
						DebugLog.DebugWrite($"{AttachedObject.name} Switch to connected");
						_platformState = NomaiRemoteCameraPlatform.State.Connected;
					}
					break;
				case NomaiRemoteCameraPlatform.State.Connected:
					// When connection is established
					VerifySectorOccupancy();
					UpdateHologramTransforms();
					_slavePlatform._poolT = _poolT;
					_slavePlatform.UpdatePoolRenderer();
					break;
				case NomaiRemoteCameraPlatform.State.Disconnecting_FadeIn:
					// Fade out props on the target platform?
					_slavePlatform._transitionFade = Mathf.MoveTowards(_slavePlatform._transitionFade, 1f, Time.deltaTime / _fadeOutLength);
					_slavePlatform.UpdateRendererFade();
					UpdateHologramTransforms();
					_slavePlatform._poolT = _poolT;
					_slavePlatform.UpdatePoolRenderer();
					if (_slavePlatform._transitionFade == 1f)
					{
						_slavePlatform._poolT = (_slavePlatform._sharedStone != null) ? 1f : 0f;
						_slavePlatform._showPlayerRipples = false;
						_slavePlatform._activePlayerHolo = null;
						_slavePlatform.UpdatePoolRenderer();
						_slavePlatform._transitionFade = 0f;
						_slavePlatform.UpdateRendererFade();
						_transitionFade = 1f;
						UpdateRendererFade();
						SwitchToPlayerCamera();
						_hologramGroup.SetActive(false);
						DebugLog.DebugWrite($"{AttachedObject.name} Switch to disconnecting_fadeout");
						_platformState = NomaiRemoteCameraPlatform.State.Disconnecting_FadeOut;
					}
					break;
				case NomaiRemoteCameraPlatform.State.Disconnecting_FadeOut:
					// Fade in props on the current platform?
					_transitionFade = Mathf.MoveTowards(_transitionFade, 0f, Time.deltaTime / _fadeOutLength);
					UpdateRendererFade();
					if (dontDisable)
					{
						DebugLog.DebugWrite($"{AttachedObject.name} DONT DISABLE!");
						break;
					}
					if (_transitionFade == 0f)
					{
						if (_sharedStone == null)
						{
							_slavePlatform = null;
						}
						DebugLog.DebugWrite($"{AttachedObject.name} Switch to disconnected");
						_platformState = NomaiRemoteCameraPlatform.State.Disconnected;
					}
					break;
			}
			if (_platformState == NomaiRemoteCameraPlatform.State.Disconnected && !_active && _poolT == 0f)
			{
				AttachedObject.enabled = false;
			}
		}
	}
}
