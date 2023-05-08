using QSB.Anglerfish.Messages;
using QSB.Anglerfish.TransformSync;
using QSB.Messaging;
using QSB.Utility.LinkedWorldObject;
using UnityEngine;

namespace QSB.Anglerfish.WorldObjects;

public class QSBAngler : LinkedWorldObject<AnglerfishController, AnglerTransformSync>
{
	public override bool ShouldDisplayDebug() => false;

	public Transform TargetTransform;
	public Vector3 TargetVelocity { get; private set; }

	private Vector3 _lastTargetPosition;

	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.AnglerPrefab;
	protected override bool SpawnWithServerOwnership => false;

	public override void SendInitialState(uint to) =>
		this.SendMessage(new AnglerDataMessage(this) { To = to });

	public void UpdateTargetVelocity()
	{
		if (TargetTransform == null)
		{
			return;
		}

		var reference = Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().transform;
		var currentRelPosition = reference.InverseTransformPoint(TargetTransform.position);
		TargetVelocity = (currentRelPosition - _lastTargetPosition) / Time.fixedDeltaTime;
		_lastTargetPosition = currentRelPosition;
	}
}
