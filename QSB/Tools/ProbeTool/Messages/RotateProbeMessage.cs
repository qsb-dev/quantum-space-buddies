using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Tools.ProbeTool.Messages;

public class RotateProbeMessage : QSBMessage<(RotationType rotationType, Vector2 cameraRotation)>
{
	public RotateProbeMessage(RotationType rotationType, Vector2 cameraRotation) : base((rotationType, cameraRotation)) { }

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		var playerProbe = QSBPlayerManager.GetPlayer(From).Probe;
		var rotatingCamera = playerProbe.GetRotatingCamera();

		if (Data.rotationType == RotationType.Horizontal)
		{
			rotatingCamera.RotateHorizontal(Data.cameraRotation.x);
		}
		else if (Data.rotationType == RotationType.Vertical)
		{
			rotatingCamera.RotateVertical(Data.cameraRotation.y);
		}
		else
		{
			rotatingCamera.ResetRotation();
		}
	}
}
