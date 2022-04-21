using QSB.EchoesOfTheEye.Ghosts.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.EchoesOfTheEye.Ghosts;

public static class QSBGhostZone2Director
{
	public static ElevatorStatus[] ElevatorsStatus;

	public struct ElevatorStatus
	{
		public GhostZone2Director.ElevatorPair elevatorPair;
		public bool activated;
		public bool lightsDeactivated;
		public bool deactivated;
		public float timeSinceArrival;
		public QSBElevatorWalkAction elevatorAction;
		public GhostController ghostController;
	}
}
