using OWML.Utils;
using QSB.Utility;
using UnityEngine;

namespace QSB.ItemSync
{
	class CustomNomaiRemoteCameraStreaming : SectoredMonoBehaviour
	{
		private OWTriggerVolume _owTriggerVolume;
		public CustomNomaiRemoteCameraPlatform _remoteCameraPlatform
		{
			get => _oldStreaming.GetValue<NomaiRemoteCameraPlatform>("_remoteCameraPlatform").GetComponent<CustomNomaiRemoteCameraPlatform>();
			set => _oldStreaming.SetValue("_remoteCameraPlatform", value.GetComponent<NomaiRemoteCameraStreaming>());
		}
		private StreamingGroup _streamingGroup;
		private ItemTool _itemTool;
		private SharedStone _heldStone;
		private bool _playerInVolume;
		private bool _preloadingRequiredAssets;
		private bool _preloadingGeneralAssets;
		private NomaiRemoteCameraStreaming _oldStreaming;

		protected override void Awake()
		{
			base.Awake();
			_oldStreaming = GetComponent<NomaiRemoteCameraStreaming>();
			SetSector(_oldStreaming.GetSector());
			_owTriggerVolume = GetComponent<OWTriggerVolume>();
			_owTriggerVolume.OnEntry += OnEntry;
			_owTriggerVolume.OnExit += OnExit;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			_owTriggerVolume.OnEntry -= OnEntry;
			_owTriggerVolume.OnExit -= OnExit;
		}

		private void Start()
		{
			_itemTool = Locator.GetToolModeSwapper().GetItemCarryTool();
			enabled = false;
		}

		private void FixedUpdate()
		{
			var heldItem = _itemTool.GetHeldItem();
			if (heldItem != _heldStone)
			{
				SharedStone sharedStone = null;
				if (heldItem != null && heldItem is SharedStone)
				{
					sharedStone = (heldItem as SharedStone);
				}
				if (sharedStone != null && sharedStone != _heldStone && _remoteCameraPlatform.GetSocketedStone() == null)
				{
					if (_streamingGroup != null)
					{
						UpdatePreloadingState(false, false);
					}
					_streamingGroup = StreamingGroup.GetStreamingGroup(NomaiRemoteCameraStreaming.NomaiRemoteCameraPlatformIDToSceneName(sharedStone.GetRemoteCameraID()));
				}
				_heldStone = sharedStone;
			}
			if (_streamingGroup != null)
			{
				var shouldBeLoadingRequiredAssets = _playerInVolume && (_heldStone != null || _remoteCameraPlatform.GetSocketedStone() != null);
				var shouldBeLoadingGeneralAssets = _playerInVolume && _remoteCameraPlatform.IsPlatformActive();
				UpdatePreloadingState(shouldBeLoadingRequiredAssets, shouldBeLoadingGeneralAssets);
			}
		}

		private void UpdatePreloadingState(bool shouldBeLoadingRequiredAssets, bool shouldBeLoadingGeneralAssets)
		{
			if (!_preloadingRequiredAssets && shouldBeLoadingRequiredAssets)
			{
				_streamingGroup.RequestRequiredAssets(0);
				_preloadingRequiredAssets = true;
			}
			else if (_preloadingRequiredAssets && !shouldBeLoadingRequiredAssets)
			{
				_streamingGroup.ReleaseRequiredAssets();
				_preloadingRequiredAssets = false;
			}
			if (!_preloadingGeneralAssets && shouldBeLoadingGeneralAssets)
			{
				_streamingGroup.RequestGeneralAssets(0);
				_preloadingGeneralAssets = true;
			}
			else if (_preloadingGeneralAssets && !shouldBeLoadingGeneralAssets)
			{
				_streamingGroup.ReleaseGeneralAssets();
				_preloadingGeneralAssets = false;
			}
		}

		protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
		{
			if (sectorDetector.GetOccupantType() == DynamicOccupant.Player && StreamingManager.isStreamingEnabled)
			{
				enabled = true;
			}
		}

		protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
		{
			if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
			{
				if (_streamingGroup != null)
				{
					UpdatePreloadingState(false, false);
				}
				_streamingGroup = null;
				_heldStone = null;
				enabled = false;
			}
		}

		private void OnEntry(GameObject hitObj)
		{
			var attachedOWRigidbody = hitObj.GetAttachedOWRigidbody(false);
			if (attachedOWRigidbody != null && attachedOWRigidbody.CompareTag("Player"))
			{
				_playerInVolume = true;
			}
		}

		private void OnExit(GameObject hitObj)
		{
			var attachedOWRigidbody = hitObj.GetAttachedOWRigidbody(false);
			if (attachedOWRigidbody != null && attachedOWRigidbody.CompareTag("Player"))
			{
				_playerInVolume = false;
			}
		}
	}
}
