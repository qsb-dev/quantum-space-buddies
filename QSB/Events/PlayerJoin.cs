using System.Collections;
using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class PlayerJoin : NetworkBehaviour
    {
        public static string MyName { get; private set; }

        private MessageHandler<JoinMessage> _joinHandler;

        private void Awake()
        {
            _joinHandler = new MessageHandler<JoinMessage>();
            _joinHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _joinHandler.OnServerReceiveMessage += OnServerReceiveMessage;
        }

        // Called when joining a server
        public void Join(string playerName)
        {
            MyName = playerName;
            StartCoroutine(SendJoinMessage(playerName));
        }

        // Send join message with player name and ID
        private IEnumerator SendJoinMessage(string playerName)
        {
            yield return new WaitUntil(() => PlayerTransformSync.LocalInstance != null);
            var message = new JoinMessage
            {
                PlayerName = playerName,
                SenderId = PlayerTransformSync.LocalInstance.netId.Value
            };
            _joinHandler.SendToServer(message);
        }

        private void OnServerReceiveMessage(JoinMessage message)
        {
            _joinHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(JoinMessage message)
        {
            var player = PlayerRegistry.CreatePlayer(message.SenderId);
            player.Name = message.PlayerName;
            player.IsReady = true;
            DebugLog.ToAll(message.PlayerName, "joined!");
        }
    }
}
