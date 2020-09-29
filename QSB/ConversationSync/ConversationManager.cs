using QSB.Events;
using UnityEngine;

namespace QSB.ConversationSync
{
    public class ConversationManager : MonoBehaviour
    {
        public static ConversationManager Instance { get; private set; }

        private void Start()
        {
            Instance = this;
        }

        public void SendPlayerOption(string text)
        {
            GlobalMessenger<int, string, ConversationType>.FireEvent(EventNames.QSBConversation, -1, text, ConversationType.Player);
        }

        public void SendCharacterDialogue(int id, string text)
        {
            GlobalMessenger<int, string, ConversationType>.FireEvent(EventNames.QSBConversation, id, text, ConversationType.Character);
        }
    }
}
