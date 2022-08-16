using QSB.Player;

namespace QSB.EchoesOfTheEye.Ghosts;

public class GhostPlayer
{
	public PlayerInfo player;
	public QSBGhostSensorData sensor = new();
	public GhostLocationData playerLocation = new();
	public GhostLocationData lastKnownPlayerLocation = new();
	public QSBGhostSensorData lastKnownSensor = new();
	public QSBGhostSensorData firstUnknownSensor = new();
	public bool isPlayerLocationKnown;
	public bool wasPlayerLocationKnown;
	public float timeLastSawPlayer;
	public float timeSincePlayerLocationKnown = float.PositiveInfinity;
	public float playerMinLanternRange;
	public bool lostPlayerDueToOcclusion
		=> !isPlayerLocationKnown
		&& !lastKnownSensor.isPlayerOccluded
		&& firstUnknownSensor.isPlayerOccluded;
}
