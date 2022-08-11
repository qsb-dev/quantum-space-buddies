using QSB.Animation.NPC.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.Animation.NPC.WorldObjects;

internal class QSBCharacterAnimController : WorldObject<CharacterAnimController>
{
    public override void SendInitialState(uint to)
        => this.SendMessage(new CharacterAnimControllerMessage(InConversation()) { To = to });

    public CharacterDialogueTree GetDialogueTree()
        => AttachedObject._dialogueTree;

    public void SetInConversation(bool inConversation)
    {
        AttachedObject._inConversation = inConversation;
        if (AttachedObject._hasTalkAnimation && AttachedObject._animator != null)
        {
            string animationState = inConversation ? "Talking" : "Idle";
            AttachedObject._animator.SetTrigger(animationState);
        }
    }

    public bool InConversation()
        => AttachedObject._inConversation;
}