namespace QSB.EchoesOfTheEye.Ghosts;

public class QSBGhostSensorData
{
	public bool isPlayerVisible;
	public bool isPlayerHeldLanternVisible;
	public bool isPlayerDroppedLanternVisible;
	public bool isPlayerHoldingLantern;
	public bool isIlluminatedByPlayer;
	public bool inContactWithPlayer;
	public bool isPlayerOccluded;
	public bool isPlayerIlluminated;
	public bool isPlayerIlluminatedByUs;
	public bool isPlayerInGuardVolume;

	public bool knowsPlayerVelocity => isPlayerVisible || isPlayerHeldLanternVisible;

	public void CopyFromOther(QSBGhostSensorData other)
	{
		isPlayerVisible = other.isPlayerVisible;
		isPlayerHeldLanternVisible = other.isPlayerHeldLanternVisible;
		isPlayerDroppedLanternVisible = other.isPlayerDroppedLanternVisible;
		isPlayerHoldingLantern = other.isPlayerHoldingLantern;
		isIlluminatedByPlayer = other.isIlluminatedByPlayer;
		inContactWithPlayer = other.inContactWithPlayer;
		isPlayerOccluded = other.isPlayerOccluded;
		isPlayerIlluminated = other.isPlayerIlluminated;
		isPlayerIlluminatedByUs = other.isPlayerIlluminatedByUs;
		isPlayerInGuardVolume = other.isPlayerInGuardVolume;
	}
}
