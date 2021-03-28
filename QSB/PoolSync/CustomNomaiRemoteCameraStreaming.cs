using OWML.Utils;

namespace QSB.PoolSync
{
	internal class CustomNomaiRemoteCameraStreaming : SectoredMonoBehaviour
	{
		public CustomNomaiRemoteCameraPlatform _remoteCameraPlatform
		{
			get => _oldStreaming.GetValue<NomaiRemoteCameraPlatform>("_remoteCameraPlatform").GetComponent<CustomNomaiRemoteCameraPlatform>();
			set => _oldStreaming.SetValue("_remoteCameraPlatform", value.GetComponent<NomaiRemoteCameraStreaming>());
		}
		private StreamingGroup _streamingGroup;
		private NomaiRemoteCameraStreaming _oldStreaming;
		private bool _hasLoadedAssets;

		protected override void Awake()
		{
			base.Awake();
			_oldStreaming = GetComponent<NomaiRemoteCameraStreaming>();
			SetSector(_oldStreaming.GetSector());
		}

		private void Start()
			=> enabled = false;

		private void FixedUpdate()
		{
			var stone = _remoteCameraPlatform.GetSocketedStone();
			if (stone == null)
			{
				if (_hasLoadedAssets)
				{
					_hasLoadedAssets = false;
					_streamingGroup.ReleaseRequiredAssets();
					_streamingGroup.ReleaseGeneralAssets();
					_streamingGroup = null;
				}
			}
			else
			{
				if (!_hasLoadedAssets)
				{
					_hasLoadedAssets = true;
					_streamingGroup = StreamingGroup.GetStreamingGroup(NomaiRemoteCameraStreaming.NomaiRemoteCameraPlatformIDToSceneName(stone.GetRemoteCameraID()));
					_streamingGroup.RequestRequiredAssets(0);
					_streamingGroup.RequestGeneralAssets(0);
				}
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
				enabled = false;
			}
		}
	}
}
