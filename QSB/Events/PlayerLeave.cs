using System.Linq;
using QSB.Messaging;
using QSB.TransformSync;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class PlayerLeave : NetworkBehaviour
    {
        private MessageHandler<LeaveMessage> _leaveHandler;

        private void Awake()
        {
            _leaveHandler = new MessageHandler<LeaveMessage>();
            _leaveHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _leaveHandler.OnServerReceiveMessage += OnServerReceiveMessage;
        }

        public void Leave(uint playerId)
        {
            var message = new LeaveMessage
            {
                PlayerName = PlayerJoin.PlayerNames[playerId],
                SenderId = playerId
            };
            _leaveHandler.SendToAll(message);
        }

        private void OnServerReceiveMessage(LeaveMessage message)
        {
            _leaveHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(LeaveMessage message)
        {
            DebugLog.All(message.PlayerName, "left");

            PlayerJoin.PlayerNames.Remove(message.SenderId);

            CleanUpPlayer(message.SenderId);
            CleanUpShip(message.SenderId);
        }

        private void CleanUpPlayer(uint playerId)
        {
            var playerTransformSync = GameObject.FindObjectsOfType<PlayerTransformSync>()
                .FirstOrDefault(x => x.netId.Value == playerId);
            if (playerTransformSync == null)
            {
                return;
            }
            Destroy(playerTransformSync.PlayerModel.gameObject);
            Destroy(playerTransformSync.gameObject);
        }

        private void CleanUpShip(uint playerId)
        {
            var shipTransformSync = GameObject.FindObjectsOfType<ShipTransformSync>()
                .FirstOrDefault(x => x.netId.Value == playerId);
            if (shipTransformSync == null)
            {
                return;
            }
            Destroy(shipTransformSync.ShipModel.gameObject);
            Destroy(shipTransformSync.gameObject);
        }

    }
}
