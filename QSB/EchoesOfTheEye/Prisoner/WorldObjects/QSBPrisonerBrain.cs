using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.Ghosts;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.EchoesOfTheEye.Prisoner.Messages;
using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Prisoner.WorldObjects;

public class QSBPrisonerBrain : WorldObject<PrisonerBrain>, IGhostObject
{
	public override async UniTask Init(CancellationToken ct)
	{
		Start();
	}

	public QSBGhostController Controller => AttachedObject._controller.GetWorldObject<QSBGhostController>();
	public QSBPrisonerEffects Effects => AttachedObject._effects.GetWorldObject<QSBPrisonerEffects>();
	public QSBGhostSensors Sensors => AttachedObject._sensors.GetWorldObject<QSBGhostSensors>();
	public QSBGhostData Data;

	public override void DisplayLines()
	{
		ControllerLines(Controller);
		DataLines(Data, Controller);
	}

	private void ControllerLines(QSBGhostController controller)
	{
		Popcron.Gizmos.Sphere(controller.AttachedObject.transform.position, 2f, Color.white);

		if (controller.AttachedObject._followNodePath)
		{
			for (var i = controller.AttachedObject._nodePath.Count - 1; i >= 0; i--)
			{
				Popcron.Gizmos.Sphere(controller.AttachedObject.LocalToWorldPosition(controller.AttachedObject._nodePath[i].localPosition), 0.25f, Color.cyan, 3);

				var hasVisited = controller.AttachedObject._pathIndex < i;
				var color = hasVisited ? Color.white : Color.cyan;

				if (i != 0)
				{
					Popcron.Gizmos.Line(controller.AttachedObject.LocalToWorldPosition(controller.AttachedObject._nodePath[i].localPosition), controller.AttachedObject.LocalToWorldPosition(controller.AttachedObject._nodePath[i - 1].localPosition), color);
				}
			}

			if (controller.AttachedObject._hasFinalPathPosition)
			{
				Popcron.Gizmos.Sphere(controller.AttachedObject.LocalToWorldPosition(controller.AttachedObject._finalPathPosition), 0.3f, Color.red, 8);
			}
		}
	}

	private void DataLines(QSBGhostData data, QSBGhostController controller)
	{
		foreach (var player in data.players.Values)
		{
			if (player.timeSincePlayerLocationKnown != float.PositiveInfinity)
			{
				Popcron.Gizmos.Line(controller.AttachedObject.transform.position, controller.AttachedObject.LocalToWorldPosition(player.lastKnownPlayerLocation.localPosition), Color.magenta);
				Popcron.Gizmos.Sphere(controller.AttachedObject.LocalToWorldPosition(player.lastKnownPlayerLocation.localPosition), 1f, Color.magenta);
			}
		}
	}

	public void Start()
	{
		AttachedObject.enabled = false;
		AttachedObject._controller.GetDreamLanternController().enabled = false;
		Controller.Initialize(AttachedObject._nodeLayer, Effects);
		Data = new QSBGhostData();
		Sensors.Initialize(Data);
		Effects.Initialize(AttachedObject._controller.GetNodeRoot(), Controller, Data);
	}

	public void FixedUpdate()
	{
		Controller.AttachedObject.FixedUpdate_Controller();
		Sensors.FixedUpdate_Sensors();
		Data.FixedUpdate_Data(Controller.AttachedObject, Sensors.AttachedObject);

		if (!QSBCore.IsHost)
		{
			return;
		}

		if (Controller.AttachedObject.IsMoving())
		{
			// BUG: does this make it not stop for remote players?
			bool movementPaused = AttachedObject._blockMovementVolume.IsTrackingObject(Locator.GetPlayerDetector()) || !AttachedObject._allowMovementVolume.IsTrackingObject(Locator.GetPlayerDetector());
			Controller.AttachedObject.SetMovementPaused(movementPaused);
		}
	}

	public void Update()
	{
		Controller.AttachedObject.Update_Controller();
		Sensors.AttachedObject.Update_Sensors();
		Effects.Update_Effects();

		if (!QSBCore.IsHost)
		{
			return;
		}

		if (AttachedObject._pendingBehavior != PrisonerBehavior.None && Time.time > AttachedObject._pendingBehaviorEntryTime)
		{
			AttachedObject.ExitBehavior(AttachedObject._currentBehavior);
			var currentBehavior = AttachedObject._currentBehavior;
			AttachedObject._currentBehavior = AttachedObject._pendingBehavior;
			AttachedObject._behaviorCueMarker = AttachedObject._pendingBehaviorCueMarker;
			AttachedObject._pendingBehavior = PrisonerBehavior.None;
			AttachedObject._pendingBehaviorCueMarker = null;
			AttachedObject.EnterBehavior(AttachedObject._currentBehavior, currentBehavior);

			this.SendMessage(new PrisonerEnterBehaviourMessage(AttachedObject._currentBehavior, AttachedObject._behaviorCueMarker?.GetComponent<PrisonerBehaviourCueMarker>()));
		}
	}

	public void EnterBehaviour(PrisonerBehavior newBehaviour, Transform marker)
	{
		DebugLog.DebugWrite($"Enter Behaviour {newBehaviour}");

		var currentBehaviour = AttachedObject._currentBehavior;

		AttachedObject._currentBehavior = newBehaviour;
		AttachedObject._behaviorCueMarker = marker;

		AttachedObject.EnterBehavior(newBehaviour, currentBehaviour);
	}
}
