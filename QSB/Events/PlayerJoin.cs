using System.Collections;
using System.Collections.Generic;
using QSB.Messaging;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class PlayerJoin : NetworkBehaviour
    {
        public static readonly Dictionary<uint, string> PlayerNames = new Dictionary<uint, string>();
        public static string MyName { get; private set; }

        private MessageHandler<JoinMessage> _joinHandler;
        private MessageHandler<LeaveMessage> _leaveHandler;

        private void Awake()
        {
            _joinHandler = new MessageHandler<JoinMessage>();
            _joinHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _joinHandler.OnServerReceiveMessage += OnServerReceiveMessage;

            _leaveHandler = new MessageHandler<LeaveMessage>();
            _leaveHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _leaveHandler.OnServerReceiveMessage += OnServerReceiveMessage;
        }

        public void Join(string playerName)
        {
            MyName = playerName;
            StartCoroutine(SendJoinMessage(playerName));
        }

        private IEnumerator SendJoinMessage(string playerName)
        {
            yield return new WaitUntil(() => NetPlayer.LocalInstance != null);
            var message = new JoinMessage
            {
                PlayerName = playerName,
                SenderId = NetPlayer.LocalInstance.netId.Value
            };
            _joinHandler.SendToServer(message);
        }

        public void Leave(uint playerId) // called by server
        {
            var message = new LeaveMessage
            {
                PlayerName = PlayerNames[playerId],
                SenderId = playerId
            };
            _leaveHandler.SendToAll(message);
        }

        private void OnServerReceiveMessage(JoinMessage message)
        {
            _joinHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(JoinMessage message)
        {
            PlayerNames[message.SenderId] = message.PlayerName;
            DebugLog.All(message.PlayerName, "joined!");
        }

        private void OnServerReceiveMessage(LeaveMessage message)
        {
            _leaveHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(LeaveMessage message)
        {
            if (PlayerNames.ContainsKey(message.SenderId))
            {
                PlayerNames.Remove(message.SenderId);
            }
            DebugLog.All(message.PlayerName, "left");
        }

    }
}
