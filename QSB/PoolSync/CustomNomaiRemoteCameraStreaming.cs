namespace QSB.PoolSync;

public class CustomNomaiRemoteCameraStreaming : SectoredMonoBehaviour
{
	private CustomNomaiRemoteCameraPlatform _remoteCameraPlatform;
	private StreamingGroup _streamingGroup;
	private NomaiRemoteCameraStreaming _oldStreaming;
	private bool _hasLoadedAssets;

	public override void Awake()
	{
		base.Awake();
		_oldStreaming = GetComponent<NomaiRemoteCameraStreaming>();
		SetSector(_oldStreaming.GetSector());
	}

	private void Start()
	{
		_remoteCameraPlatform = _oldStreaming._remoteCameraPlatform.GetComponent<CustomNomaiRemoteCameraPlatform>();
		enabled = false;
	}

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
				_streamingGroup.RequestRequiredAssets();
				_streamingGroup.RequestGeneralAssets();
			}
		}
	}

	public override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player && StreamingManager.isStreamingEnabled)
		{
			enabled = true;
		}
	}

	public override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			enabled = false;
		}
	}
}