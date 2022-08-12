using Mirror;
using QSB.ConversationSync.WorldObjects;
using QSB.Messaging;

namespace QSB.ConversationSync.Messages
{
    internal class CharacterDialogueTreeMessage : QSBWorldObjectMessage<QSBCharacterDialogueTree, CharacterDialogueTreeStateData>
    {
        public CharacterDialogueTreeMessage(string currentText, uint playerID) : base(new CharacterDialogueTreeStateData(currentText, playerID)) { }

        public override void OnReceiveRemote()
        {
            WorldObject.SetInConversation(Data.currentText, Data.playerID);
        }
    }
    public struct CharacterDialogueTreeStateData 
    {
        public string currentText;
        public uint playerID;

        public CharacterDialogueTreeStateData(string currentText, uint playerID)
        {
            this.currentText = currentText;
            this.playerID = playerID;
        }
    }
}

