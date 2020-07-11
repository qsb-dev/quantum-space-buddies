using System.Collections;
using System.Collections.Generic;
using QSB.Messaging;
using QSB.TransformSync;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Events
{
    /// <summary>
    /// Client-only-side component for managing player joins.
    /// </summary>
    public class PlayerJoin : NetworkBehaviour
    {
        public static Dictionary<uint, string> PlayerNames = new Dictionary<uint, string>();
        public static string MyName { get; private set; }

        private MessageHandler<JoinMessage> _joinHandler;

        private void Awake()
        {
            _joinHandler = new MessageHandler<JoinMessage>();
            _joinHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _joinHandler.OnServerReceiveMessage += OnServerReceiveMessage;
        }

        public void Join(string playerName) // Called when joining a server
        {
            MyName = playerName;
            StartCoroutine(SendJoinMessage(playerName));
        }

        private IEnumerator SendJoinMessage(string playerName) // Send join message with player name and ID
        {
            yield return new WaitUntil(() => PlayerTransformSync.LocalInstance != null);
            var message = new JoinMessage
            {
                PlayerName = playerName,
                SenderId = PlayerTransformSync.LocalInstance.netId.Value
            };
            _joinHandler.SendToServer(message);
        }

        private void OnServerReceiveMessage(JoinMessage message) // Distrubute player join message to all connected clients
        {
            _joinHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(JoinMessage message)
        {
            PlayerNames[message.SenderId] = message.PlayerName;
            DebugLog.ToAll(message.PlayerName, "joined!");
        }

    }
}
