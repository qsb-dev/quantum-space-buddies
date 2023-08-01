using OWML.Common;
using QSB.Animation.Player;
using QSB.Messaging;
using QSB.Player;
using QSB.Player.Messages;
using QSB.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace QSB.PoolSync;

public class CustomNomaiRemoteCameraPlatform : NomaiShared
{
	public static List<CustomNomaiRemoteCameraPlatform> CustomPlatformList;
	private static MaterialPropertyBlock s_matPropBlock;
	private static int s_propID_Fade;
	private static int s_propID_HeightMaskScale;
	private static int s_propID_WaveScale;
	private static int s_propID_Ripple2Position;
	private static int s_propID_Ripple2Params;
	public string _dataPointID => _oldPlatform._dataPointID;
	public Sector _visualSector => _oldPlatform._visualSector;
	public Sector _visualSector2 => _oldPlatform._visualSector2;
	public Shape _connectionBounds => _oldPlatform._connectionBounds;
	public MeshRenderer _poolRenderer => _oldPlatform._poolRenderer;
	public float _poolFillLength => _oldPlatform._poolFillLength;
	public float _poolEmptyLength => _oldPlatform._poolEmptyLength;
	public AnimationCurve _poolHeightCurve => _oldPlatform._poolHeightCurve;
	public AnimationCurve _poolMaskCurve => _oldPlatform._poolMaskCurve;
	public AnimationCurve _poolWaveHeightCurve => _oldPlatform._poolWaveHeightCurve;
	public Renderer[] _transitionRenderers => _oldPlatform._transitionRenderers;
	public PedestalAnimator _transitionPedestalAnimator => _oldPlatform._transitionPedestalAnimator;
	public GameObject _transitionStone => _oldPlatform._transitionStone;
	public GameObject _hologramGroup => _oldPlatform._hologramGroup;
	public Transform _playerHologram => _oldPlatform._playerHologram;
	public Transform _stoneHologram => _oldPlatform._stoneHologram;
	public float _fadeInLength => _oldPlatform._fadeInLength;
	public float _fadeOutLength => _oldPlatform._fadeOutLength;
	public OWAudioSource _ambientAudioSource => _oldPlatform._ambientAudioSource;
	public OWAudioSource _oneShotAudioSource => _oldPlatform._oneShotAudioSource;
	public DarkZone _darkZone => _oldPlatform._darkZone;
	private OWCamera _playerCamera;
	private CustomNomaiRemoteCamera _ownedCamera;
	private SharedStoneSocket _socket;
	private PedestalAnimator _pedestalAnimator;
	private bool _platformActive;
	private float _poolT;
	private bool _showPlayerRipples;
	private Transform _activePlayerHolo;
	private float _transitionFade;
	private CustomNomaiRemoteCameraPlatform _slavePlatform;
	private List<Sector> _alreadyOccupiedSectors;
	private NomaiRemoteCameraPlatform _oldPlatform;
	private bool _anyoneStillOnPlatform;
	private bool _wasLocalInBounds;
	private CameraState _cameraState;
	private readonly Dictionary<PlayerInfo, GameObject> _playerToHologram = new();

	private void Awake()
	{
		_oldPlatform = GetComponent<NomaiRemoteCameraPlatform>();
		_id = _oldPlatform._id;
		_sharedStone = _oldPlatform._sharedStone;
		_ownedCamera = GetComponentInChildren<CustomNomaiRemoteCamera>();
		_alreadyOccupiedSectors = new List<Sector>(16);
		_cameraState = CameraState.Disconnected;
		_platformActive = false;
		_poolT = 0f;
		_showPlayerRipples = false;
		_activePlayerHolo = null;
		_transitionFade = 0f;
		if (s_matPropBlock == null)
		{
			s_matPropBlock = new MaterialPropertyBlock();
			s_propID_Fade = Shader.PropertyToID("_Fade");
			s_propID_HeightMaskScale = Shader.PropertyToID("_HeightMaskScale");
			s_propID_WaveScale = Shader.PropertyToID("_WaveScale");
			s_propID_Ripple2Position = Shader.PropertyToID("_Ripple2Position");
			s_propID_Ripple2Params = Shader.PropertyToID("_Ripple2Params");
		}

		_socket = GetComponentInChildren<SharedStoneSocket>();
		if (_socket != null)
		{
			_pedestalAnimator = _socket.GetPedestalAnimator();
		}
		else
		{
			Debug.LogWarning("SharedStoneSocket not found!", this);
		}

		UpdatePoolRenderer();
		_hologramGroup.SetActive(false);
		UpdateRendererFade();
		_transitionStone.SetActive(false);
		_hologramGroup.transform.SetParent(null);
	}

	private void Start()
	{
		if (CustomPlatformList == null)
		{
			CustomPlatformList = new List<CustomNomaiRemoteCameraPlatform>(32);
		}

		CustomPlatformList.Add(this);
		_playerCamera = Locator.GetPlayerCamera();
		if (_socket != null)
		{
			_socket.OnSocketableRemoved += OnSocketableRemoved;
			_socket.OnSocketableDonePlacing += OnSocketableDonePlacing;
		}

		enabled = false;
		QSBPlayerManager.OnRemovePlayer += OnRemovePlayer;
	}

	private void OnDestroy()
	{
		if (_socket != null)
		{
			_socket.OnSocketableRemoved -= OnSocketableRemoved;
			_socket.OnSocketableDonePlacing -= OnSocketableDonePlacing;
		}

		if (CustomPlatformList != null)
		{
			CustomPlatformList.Remove(this);
		}

		if (_cameraState is CameraState.Connected or CameraState.Connecting_FadeIn or CameraState.Connecting_FadeOut)
		{
			DisconnectCamera();
			SwitchToPlayerCamera();
		}

		QSBPlayerManager.OnRemovePlayer -= OnRemovePlayer;
	}

	private void LateUpdate()
	{
		// can't put this stuff in Update/UpdateHologramTransforms as
		// manual bone rotations need to happen after the animator has changed them
		if ((_platformActive && _anyoneStillOnPlatform) || _cameraState == CameraState.Disconnecting_FadeIn)
		{
			foreach (var item in _playerToHologram)
			{
				var hologram = item.Value.transform;
				var anim = hologram.GetChild(0).gameObject.GetComponent<Animator>();
				var cameraRotation = item.Key.CameraBody.transform.localRotation.eulerAngles;
				var rotation = Quaternion.Euler(-cameraRotation.y, -cameraRotation.z, cameraRotation.x); // wtf why
				anim.GetBoneTransform(HumanBodyBones.Head).localRotation = rotation;
			}
		}
	}

	private void Update()
	{
		if (_platformActive)
		{
			var localInBounds = _connectionBounds.PointInside(_playerCamera.transform.position);
			_anyoneStillOnPlatform = QSBCore.IsInMultiplayer
				? QSBPlayerManager.PlayerList.Any(x => _connectionBounds.PointInside(x.Camera.transform.position))
				: localInBounds;
			if (!localInBounds && _wasLocalInBounds)
			{
				OnLeaveBounds();
				_wasLocalInBounds = false;
			}
			else if (localInBounds && !_wasLocalInBounds)
			{
				OnEnterBounds();
				_wasLocalInBounds = true;
			}
			else if (!_anyoneStillOnPlatform && !_wasLocalInBounds)
			{
				OnLeaveBounds();
			}

			if (_anyoneStillOnPlatform)
			{
				UpdateHologramTransforms();
			}
		}

		if (_platformActive)
		{
			UpdatePools(1f, _poolFillLength);
		}
		else
		{
			UpdatePools(0f, _poolEmptyLength);
		}

		switch (_cameraState)
		{
			case CameraState.WaitingForPedestalContact:
				if (_pedestalAnimator.HasMadeContact())
				{
					_cameraState = CameraState.Disconnected;
					ConnectCamera();
				}

				break;
			case CameraState.Connecting_FadeIn:
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
					_ambientAudioSource.FadeIn(3f, true);
					Locator.GetAudioMixer().MixRemoteCameraPlatform(_fadeInLength);
					_cameraState = CameraState.Connecting_FadeOut;
				}

				break;
			case CameraState.Connecting_FadeOut:
				_slavePlatform._transitionFade = Mathf.MoveTowards(_slavePlatform._transitionFade, 0f, Time.deltaTime / _fadeInLength);
				_slavePlatform.UpdateRendererFade();
				_slavePlatform._poolT = _poolT;
				_slavePlatform.UpdatePoolRenderer();
				if (_slavePlatform._transitionFade == 0f)
				{
					_cameraState = CameraState.Connected;
				}

				break;
			case CameraState.Connected:
				VerifySectorOccupancy();
				_slavePlatform._poolT = _poolT;
				_slavePlatform.UpdatePoolRenderer();
				break;
			case CameraState.Disconnecting_FadeIn:
				_slavePlatform._transitionFade = Mathf.MoveTowards(_slavePlatform._transitionFade, 1f, Time.deltaTime / _fadeOutLength);
				_slavePlatform.UpdateRendererFade();
				UpdateHologramTransforms();
				_slavePlatform._poolT = _poolT;
				_slavePlatform.UpdatePoolRenderer();
				if (_slavePlatform._transitionFade == 1f)
				{
					_slavePlatform._poolT = (!(_slavePlatform._sharedStone == null)) ? 1f : 0f;
					_slavePlatform._showPlayerRipples = false;
					_slavePlatform._activePlayerHolo = null;
					_slavePlatform.UpdatePoolRenderer();
					_slavePlatform._transitionFade = 0f;
					_slavePlatform.UpdateRendererFade();
					_transitionFade = 1f;
					UpdateRendererFade();
					SwitchToPlayerCamera();
					_hologramGroup.SetActive(false);
					_cameraState = CameraState.Disconnecting_FadeOut;
				}

				break;
			case CameraState.Disconnecting_FadeOut:
				_transitionFade = Mathf.MoveTowards(_transitionFade, 0f, Time.deltaTime / _fadeOutLength);
				UpdateRendererFade();
				if (_transitionFade == 0f)
				{
					_cameraState = CameraState.Disconnected;
				}

				break;
		}

		if (_cameraState == CameraState.Disconnected && !_platformActive && _poolT == 0f)
		{
			enabled = false;
		}
	}

	private void UpdatePools(float target, float length)
	{
		_poolT = Mathf.MoveTowards(_poolT, target, Time.deltaTime / length);
		if (OWMath.ApproxEquals(_poolT, target))
		{
			if (_slavePlatform != null && target == 0f)
			{
				_slavePlatform = null;
			}

			return;
		}

		UpdatePoolRenderer();
		_slavePlatform._poolT = _poolT;
		_slavePlatform.UpdatePoolRenderer();
	}

	private void VerifySectorOccupancy()
	{
		if (_slavePlatform._visualSector != null && !_slavePlatform._visualSector.ContainsOccupant(DynamicOccupant.Player))
		{
			DebugLog.ToConsole($"Warning - Player was somehow removed from the {_slavePlatform.name}'s visual sectors!  Re-adding...", MessageType.Warning);
			_slavePlatform._visualSector.AddOccupant(Locator.GetPlayerSectorDetector());
			var parentSector = _slavePlatform._visualSector.GetParentSector();
			while (parentSector != null)
			{
				if (!parentSector.ContainsOccupant(DynamicOccupant.Player))
				{
					parentSector.AddOccupant(Locator.GetPlayerSectorDetector());
				}

				parentSector = parentSector.GetParentSector();
			}
		}

		if (_slavePlatform._visualSector2 != null && !_slavePlatform._visualSector2.ContainsOccupant(DynamicOccupant.Player))
		{
			_slavePlatform._visualSector2.AddOccupant(Locator.GetPlayerSectorDetector());
		}
	}

	private void UpdatePoolRenderer()
	{
		_poolRenderer.transform.localPosition = new Vector3(0f, _poolHeightCurve.Evaluate(_poolT), 0f);
		_poolRenderer.material.SetFloat(s_propID_HeightMaskScale, _poolMaskCurve.Evaluate(_poolT));
		var waveScale = _poolRenderer.sharedMaterial.GetVector(s_propID_WaveScale);
		waveScale.y = _poolWaveHeightCurve.Evaluate(_poolT);
		_poolRenderer.material.SetVector(s_propID_WaveScale, waveScale);
		var ripplePosition = _poolRenderer.sharedMaterial.GetVector(s_propID_Ripple2Position);
		var rippleParams = _poolRenderer.sharedMaterial.GetVector(s_propID_Ripple2Params);
		if (_showPlayerRipples)
		{
			var playerPosition = _poolRenderer.transform.InverseTransformPoint(_activePlayerHolo.position);
			ripplePosition.x = playerPosition.x;
			ripplePosition.y = playerPosition.z;
			rippleParams.x = 0.5f;
		}

		_poolRenderer.material.SetVector(s_propID_Ripple2Position, ripplePosition);
		_poolRenderer.material.SetVector(s_propID_Ripple2Params, rippleParams);
	}

	private void UpdateRendererFade()
	{
		s_matPropBlock.SetFloat(s_propID_Fade, 1f - _transitionFade);
		for (var i = 0; i < _transitionRenderers.Length; i++)
		{
			_transitionRenderers[i].SetPropertyBlock(s_matPropBlock);
			if (_transitionRenderers[i].enabled && _transitionFade == 0f)
			{
				_transitionRenderers[i].enabled = false;
			}
			else if (!_transitionRenderers[i].enabled && _transitionFade > 0f)
			{
				_transitionRenderers[i].enabled = true;
			}
		}

		_ownedCamera.SetImageEffectFade(1f - _transitionFade);
	}

	private void UpdateHologramTransforms()
	{
		_hologramGroup.transform.position = _slavePlatform.transform.position;
		_hologramGroup.transform.rotation = _slavePlatform.transform.rotation;
		var playerTransform = Locator.GetPlayerTransform();
		_playerHologram.position = TransformPoint(playerTransform.position, this, _slavePlatform);
		_playerHologram.rotation = TransformRotation(playerTransform.rotation, this, _slavePlatform);

		foreach (var item in _playerToHologram)
		{
			if (item.Value == null)
			{
				DebugLog.ToConsole($"Error - Gameobject for {item.Key} in _playerToHologram is null!", MessageType.Error);
				continue;
			}

			//var hologram = item.Value.transform.GetChild(0);
			var hologram = item.Value.transform;
			hologram.position = TransformPoint(item.Key.Body.transform.position, this, _slavePlatform);
			hologram.rotation = TransformRotation(item.Key.Body.transform.rotation, this, _slavePlatform);
		}

		if (_sharedStone != null)
		{
			var transform = _sharedStone.transform;
			_stoneHologram.position = TransformPoint(transform.position, this, _slavePlatform);
			_stoneHologram.rotation = TransformRotation(transform.rotation, this, _slavePlatform);
		}
		else
		{
			_stoneHologram.SetPositionAndRotation(new Vector3(0f, -2f, 0f), Quaternion.identity);
		}
	}

	private void OnSocketableRemoved(OWItem socketable)
	{
		if (_wasLocalInBounds)
		{
			new EnterLeaveMessage(EnterLeaveType.ExitPlatform, CustomPlatformList.IndexOf(this)).Send();
		}

		if (_slavePlatform == null)
		{
			return;
		}

		DisconnectCamera();
		_transitionStone.SetActive(false);
		_slavePlatform._transitionStone.SetActive(false);
		_socket.GetPedestalAnimator().PlayOpen();
		if (_transitionPedestalAnimator != null)
		{
			_transitionPedestalAnimator.PlayOpen();
		}

		if (_slavePlatform._pedestalAnimator != null)
		{
			_slavePlatform._pedestalAnimator.PlayOpen();
		}

		if (_slavePlatform._transitionPedestalAnimator != null)
		{
			_slavePlatform._transitionPedestalAnimator.PlayOpen();
		}

		_sharedStone = null;
		_platformActive = false;
		_wasLocalInBounds = false;
	}

	private void OnSocketableDonePlacing(OWItem socketable)
	{
		_platformActive = true;
		_sharedStone = socketable as SharedStone;
		if (_sharedStone == null)
		{
			Debug.LogError("Placed an empty item or a non SharedStone in a NomaiRemoteCameraPlatform");
		}

		_slavePlatform = GetPlatform(_sharedStone.GetRemoteCameraID());
		if (_slavePlatform == null)
		{
			Debug.LogError("Shared stone with Remote Camera ID: " + _sharedStone.GetRemoteCameraID() + " has no registered camera platform!");
		}

		if (_slavePlatform == this || !_slavePlatform.gameObject.activeSelf)
		{
			_sharedStone = null;
			_slavePlatform = null;
			return;
		}

		_transitionStone.SetActive(true);
		_slavePlatform._transitionStone.SetActive(true);
		_socket.GetPedestalAnimator().PlayClose();
		if (_transitionPedestalAnimator != null)
		{
			_transitionPedestalAnimator.PlayClose();
		}

		if (_slavePlatform._pedestalAnimator != null)
		{
			_slavePlatform._pedestalAnimator.PlayClose();
		}

		if (_slavePlatform._transitionPedestalAnimator != null)
		{
			_slavePlatform._transitionPedestalAnimator.PlayClose();
		}

		enabled = true;
	}

	private void SwitchToRemoteCamera()
	{
		new EnterLeaveMessage(EnterLeaveType.EnterPlatform, CustomPlatformList.IndexOf(this)).Send();
		GlobalMessenger.FireEvent("EnterNomaiRemoteCamera");
		_slavePlatform.RevealFactID();
		_slavePlatform._ownedCamera.Activate(this, _playerCamera);
		_slavePlatform._ownedCamera.SetImageEffectFade(0f);
		_alreadyOccupiedSectors.Clear();
		if (_slavePlatform._visualSector != null)
		{
			if (_visualSector.ContainsOccupant(DynamicOccupant.Player))
			{
				_alreadyOccupiedSectors.Add(_visualSector);
			}

			_slavePlatform._visualSector.AddOccupant(Locator.GetPlayerSectorDetector());
			var parentSector = _slavePlatform._visualSector.GetParentSector();
			while (parentSector != null)
			{
				if (parentSector.ContainsOccupant(DynamicOccupant.Player))
				{
					_alreadyOccupiedSectors.Add(parentSector);
				}

				parentSector.AddOccupant(Locator.GetPlayerSectorDetector());
				parentSector = parentSector.GetParentSector();
			}
		}

		if (_slavePlatform._visualSector2 != null)
		{
			_slavePlatform._visualSector2.AddOccupant(Locator.GetPlayerSectorDetector());
		}

		if (_slavePlatform._darkZone != null)
		{
			_slavePlatform._darkZone.AddPlayerToZone(true);
		}
	}

	private void SwitchToPlayerCamera()
	{
		if (QSBCore.Helper.Interaction.ModExists("xen.CommonCameraUtility"))
		{
			// this is a really fucking dumb fix, but i cannot be
			// bothered to rewrite this class to make this work better
			var ccuAssembly = QSBCore.Helper.Interaction.TryGetMod("xen.CommonCameraUtility").GetType().Assembly;
			var utilClass = ccuAssembly.GetType("CommonCameraUtil.CommonCameraUtil");
			var instance = utilClass.GetField("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
			var removeCameraMethod = utilClass.GetMethod("RemoveCamera", BindingFlags.Public | BindingFlags.Instance);
			removeCameraMethod.Invoke(instance, new object[] { _slavePlatform._ownedCamera._camera });
		}

		if (_slavePlatform._visualSector != null)
		{
			if (!_alreadyOccupiedSectors.Contains(_slavePlatform._visualSector))
			{
				_slavePlatform._visualSector.RemoveOccupant(Locator.GetPlayerSectorDetector());
			}

			var parentSector = _slavePlatform._visualSector.GetParentSector();
			while (parentSector != null)
			{
				if (!_alreadyOccupiedSectors.Contains(parentSector))
				{
					parentSector.RemoveOccupant(Locator.GetPlayerSectorDetector());
				}

				parentSector = parentSector.GetParentSector();
			}
		}

		if (_slavePlatform._visualSector2 != null)
		{
			_slavePlatform._visualSector2.RemoveOccupant(Locator.GetPlayerSectorDetector());
		}

		if (_slavePlatform._darkZone != null)
		{
			_slavePlatform._darkZone.RemovePlayerFromZone(true);
		}

		new EnterLeaveMessage(EnterLeaveType.ExitPlatform, CustomPlatformList.IndexOf(this)).Send();
		GlobalMessenger.FireEvent("ExitNomaiRemoteCamera");
		_slavePlatform._ownedCamera.Deactivate();
		_slavePlatform._ownedCamera.SetImageEffectFade(0f);
	}

	protected void RevealFactID()
	{
		if (_dataPointID.Length > 0)
		{
			Locator.GetShipLogManager().RevealFact(_dataPointID);
		}
	}

	private void ConnectCamera()
	{
		if (_cameraState == CameraState.Connected)
		{
			return;
		}

		var cameraState = _cameraState;
		if (cameraState is not CameraState.Disconnected and not CameraState.Disconnecting_FadeOut)
		{
			if (cameraState == CameraState.Disconnecting_FadeIn)
			{
				_cameraState = CameraState.Connecting_FadeOut;
			}
		}
		else
		{
			_cameraState = CameraState.Connecting_FadeIn;
		}

		_oneShotAudioSource.PlayOneShot(AudioType.NomaiRemoteCameraEntry);
		enabled = true;
	}

	private void DisconnectCamera()
	{
		if (_cameraState == CameraState.Disconnected)
		{
			return;
		}

		var cameraState = _cameraState;
		if (cameraState is not CameraState.Connected and not CameraState.Connecting_FadeOut)
		{
			if (cameraState == CameraState.Connecting_FadeIn)
			{
				_cameraState = CameraState.Disconnecting_FadeOut;
			}
		}
		else
		{
			_cameraState = CameraState.Disconnecting_FadeIn;
			_ambientAudioSource.FadeOut(0.5f);
			_oneShotAudioSource.PlayOneShot(AudioType.NomaiRemoteCameraExit);
			Locator.GetAudioMixer().UnmixRemoteCameraPlatform(_fadeOutLength);
		}
	}

	private void OnEnterBounds()
	{
		if (!_platformActive)
		{
			return;
		}

		if (_pedestalAnimator.HasMadeContact())
		{
			ConnectCamera();
		}
		else if (_cameraState == CameraState.Disconnected)
		{
			_cameraState = CameraState.WaitingForPedestalContact;
		}
	}

	private void OnLeaveBounds()
	{
		DisconnectCamera();
		new EnterLeaveMessage(EnterLeaveType.ExitPlatform, CustomPlatformList.IndexOf(this)).Send();
		if (_anyoneStillOnPlatform)
		{
			return;
		}

		_platformActive = false;
		if (_pedestalAnimator != null)
		{
			_pedestalAnimator.PlayOpen();
		}

		if (_transitionPedestalAnimator != null)
		{
			_transitionPedestalAnimator.PlayOpen();
		}

		if (_slavePlatform != null)
		{
			if (_slavePlatform._pedestalAnimator != null)
			{
				_slavePlatform._pedestalAnimator.PlayOpen();
			}

			if (_slavePlatform._transitionPedestalAnimator != null)
			{
				_slavePlatform._transitionPedestalAnimator.PlayOpen();
			}
		}
	}

	public static Vector3 TransformPoint(Vector3 worldPos, CustomNomaiRemoteCameraPlatform from, CustomNomaiRemoteCameraPlatform to)
	{
		var position = from.transform.InverseTransformPoint(worldPos);
		return to.transform.TransformPoint(position);
	}

	public static Quaternion TransformRotation(Quaternion worldRot, CustomNomaiRemoteCameraPlatform from, CustomNomaiRemoteCameraPlatform to)
	{
		var rhs = from.transform.InverseTransformRotation(worldRot);
		return to.transform.rotation * rhs;
	}

	public static CustomNomaiRemoteCameraPlatform GetPlatform(NomaiRemoteCameraPlatform.ID platformID)
	{
		if (CustomPlatformList != null)
		{
			for (var i = 0; i < CustomPlatformList.Count; i++)
			{
				if (CustomPlatformList[i]._id == platformID)
				{
					return CustomPlatformList[i];
				}
			}
		}

		return null;
	}

	public SharedStone GetSocketedStone() => _sharedStone;

	public bool IsPlatformActive() => _platformActive;

	public void OnRemovePlayer(PlayerInfo player)
	{
		if (player.IsLocalPlayer)
		{
			return;
		}

		if (!_playerToHologram.Any(x => x.Key == player))
		{
			return;
		}

		var hologram = _playerToHologram.First(x => x.Key == player).Value;
		if (hologram.activeSelf)
		{
			OnRemotePlayerExit(player.PlayerId);
		}

		_playerToHologram.Remove(player);
	}

	public void OnRemotePlayerEnter(uint playerId)
	{
		if (playerId == QSBPlayerManager.LocalPlayerId)
		{
			return;
		}

		_hologramGroup.SetActive(true);

		var player = QSBPlayerManager.GetPlayer(playerId);
		if (_playerToHologram.ContainsKey(player))
		{
			_playerToHologram[player].SetActive(true);
			return;
		}

		var hologramCopy = Instantiate(_playerHologram);
		hologramCopy.parent = _playerHologram.parent;
		Destroy(hologramCopy.GetChild(0).GetComponent<PlayerAnimController>());

		var mirror = hologramCopy.gameObject.AddComponent<AnimatorMirror>();

		hologramCopy.GetChild(0).Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_Head").gameObject.layer = 0;
		hologramCopy.GetChild(0).Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_Helmet").gameObject.layer = 0;

		if (player.AnimationSync.VisibleAnimator == null)
		{
			DebugLog.ToConsole($"Warning - {playerId}'s VisibleAnimator is null!", MessageType.Error);
		}

		mirror.Init(player.AnimationSync.VisibleAnimator, hologramCopy.GetChild(0).gameObject.GetComponent<Animator>(), null);

		_playerToHologram.Add(player, hologramCopy.gameObject);

		hologramCopy.gameObject.SetActive(true);
	}

	public void OnRemotePlayerExit(uint playerId)
	{
		if (playerId == QSBPlayerManager.LocalPlayerId)
		{
			return;
		}

		var player = QSBPlayerManager.GetPlayer(playerId);
		if (!_playerToHologram.ContainsKey(player))
		{
			return;
		}

		_playerToHologram[player].SetActive(false);

		if (!_platformActive)
		{
			_hologramGroup.SetActive(false);
		}
	}

	public enum CameraState
	{
		WaitingForPedestalContact,
		Connected,
		Connecting_FadeIn,
		Connecting_FadeOut,
		Disconnected,
		Disconnecting_FadeIn,
		Disconnecting_FadeOut
	}
}