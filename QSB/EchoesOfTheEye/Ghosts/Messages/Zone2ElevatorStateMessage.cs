using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

public class Zone2ElevatorStateMessage : QSBMessage<(int index, Zone2ElevatorState state)>
{
	public Zone2ElevatorStateMessage(int index, Zone2ElevatorState state) : base((index, state)) { }

	public override void OnReceiveRemote()
	{
		var zoneDirector = QSBWorldSync.GetUnityObject<GhostZone2Director>();

		if (Data.state == Zone2ElevatorState.TutorialElevator)
		{
			DebugLog.DebugWrite($"TUTORIAL ELEVATOR");
			zoneDirector._ghostTutorialElevator.GoToDestination(1);
			return;
		}

		if (QSBGhostZone2Director.ElevatorsStatus == null)
		{
			QSBGhostZone2Director.ElevatorsStatus = new QSBGhostZone2Director.ElevatorStatus[zoneDirector._elevators.Length];
			for (var j = 0; j < zoneDirector._elevators.Length; j++)
			{
				QSBGhostZone2Director.ElevatorsStatus[j].elevatorPair = zoneDirector._elevators[j];
				QSBGhostZone2Director.ElevatorsStatus[j].activated = false;
				QSBGhostZone2Director.ElevatorsStatus[j].deactivated = false;
				QSBGhostZone2Director.ElevatorsStatus[j].lightsDeactivated = false;
			}
		}

		if (Data.state == Zone2ElevatorState.LightsExtinguished)
		{
			DebugLog.DebugWrite($"LIGHTS EXTINGUISHED");

			foreach (var elevator in QSBGhostZone2Director.ElevatorsStatus)
			{
				elevator.elevatorPair.elevator.topLight.FadeTo(0, 0.2f);
			}

			return;
		}

		var elevatorStatus = QSBGhostZone2Director.ElevatorsStatus[Data.index];

		switch (Data.state)
		{
			case Zone2ElevatorState.GoToUndercity:
				DebugLog.DebugWrite($"{Data.index} GO TO UNDERCITY");
				elevatorStatus.elevatorPair.elevator.topLight.FadeTo(1, 0.2f);
				elevatorStatus.elevatorPair.elevator.GoToDestination(0);
				break;
			case Zone2ElevatorState.ReachedUndercity:
				DebugLog.DebugWrite($"{Data.index} REACHED UNDERCITY");
				elevatorStatus.elevatorPair.elevator.topLight.FadeTo(0, 0.2f);
				break;
			case Zone2ElevatorState.ReturnToCity:
				DebugLog.DebugWrite($"{Data.index} RETURN TO CITY");
				elevatorStatus.elevatorPair.elevator.GoToDestination(1);
				break;
		}
	}
}
