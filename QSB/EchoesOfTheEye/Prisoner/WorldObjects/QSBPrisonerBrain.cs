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

internal class QSBPrisonerBrain : WorldObject<PrisonerBrain>
{
	public override void SendInitialState(uint to)
	{

	}

	public override async UniTask Init(CancellationToken ct)
	{
		Start();
	}

	public QSBGhostController Controller => AttachedObject._controller.GetWorldObject<QSBGhostController>();
	public QSBPrisonerEffects Effects => AttachedObject._effects.GetWorldObject<QSBPrisonerEffects>();
	public QSBGhostSensors Sensors => AttachedObject._sensors.GetWorldObject<QSBGhostSensors>();
	public QSBGhostData Data;

	public void Start()
	{
		AttachedObject.enabled = false;
		AttachedObject._controller.GetDreamLanternController().enabled = false;
		Controller.Initialize(AttachedObject._nodeLayer, Effects);
		Data = new QSBGhostData();
		Sensors.Initialize(Data, null);
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
