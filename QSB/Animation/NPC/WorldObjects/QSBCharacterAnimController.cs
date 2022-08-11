using QSB.Animation.NPC.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.Animation.NPC.WorldObjects;

internal class QSBCharacterAnimController : WorldObject<CharacterAnimController>
{
    //Not needed as by syncing the intial state of QSBCharacterDialogueTree this is automatically synced too
    public override void SendInitialState(uint to)
    {
    }
     //   => this.SendMessage(new CharacterAnimControllerMessage(InConversation()) { To = to });

    public CharacterDialogueTree GetDialogueTree()
        => AttachedObject._dialogueTree;

    //public void SetInConversation(bool inConversation)
    //{
    //    AttachedObject._inConversation = inConversation;
    //    if (inConversation)
    //        AttachedObject.OnStartConversation();
    //    else
    //        AttachedObject.OnEndConversation();
    //}

    public bool InConversation()
        => AttachedObject._inConversation;
}