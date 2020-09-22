using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.ConversationSync
{
    public class ConversationManager : MonoBehaviour
    {
        public static ConversationManager Instance { get; private set; }

        void Start()
        {
            Instance = this;
        }

        public void SendPlayerOption(string text)
        {

        }

        public void SendCharacterDialogue(string text)
        {

        }
    }
}
