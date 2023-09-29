using QSB.EchoesOfTheEye.Prisoner.WorldObjects;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.Prisoner.Messages;

public class PrisonerEnterBehaviourMessage : QSBWorldObjectMessage<QSBPrisonerBrain, (PrisonerBehavior behaviour, int markerIndex)>
{
	public PrisonerEnterBehaviourMessage(PrisonerBehavior behaviour, PrisonerBehaviourCueMarker marker)
		: base((behaviour, marker != null ? marker.GetWorldObject<QSBPrisonerMarker>().ObjectId : -1)) { }

	public override void OnReceiveRemote()
	{
		var marker = (Data.markerIndex == -1) ? null : Data.markerIndex.GetWorldObject<QSBPrisonerMarker>().Transform;

		WorldObject.EnterBehaviour(Data.behaviour, marker);
	}
}
